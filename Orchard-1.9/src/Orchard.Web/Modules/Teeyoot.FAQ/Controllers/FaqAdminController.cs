﻿using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Teeyoot.FAQ.Models;
using Teeyoot.FAQ.Services;
using Teeyoot.FAQ.ViewModels;
using Orchard.Localization.Records;
using Orchard.Data;


namespace Teeyoot.FAQ.Controllers
{
    [ValidateInput(false), Admin]
    public class FaqAdminController : Controller, IUpdateModel
    {
        private readonly ITeeyootFaqService _faqService;
        private readonly ISiteService _siteService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICultureManager _cultureManager;
        private readonly ICountryCulturesProvider _countryCulturesProvider; 
        
        //private readonly string cultureUsed = string.Empty;

        public FaqAdminController(IOrchardServices services, 
                                  IShapeFactory shapeFactory, 
                                  ITeeyootFaqService faqService, 
                                  ISiteService siteService, 
                                  IWorkContextAccessor workContextAccessor,
                                  ICultureManager cultureManager,
                                ICountryCulturesProvider countryCulturesProvider)
        {
            _faqService = faqService;
            _siteService = siteService;
            Services = services;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
            _workContextAccessor = workContextAccessor;
            _cultureManager = cultureManager;
            _countryCulturesProvider = countryCulturesProvider;

            //var culture = _workContextAccessor.GetContext().CurrentCulture;
            //cultureUsed = culture == "en-SG" ? "en-SG" : (culture == "id-ID" ? "id-ID" : "en-MY");
        }

        private IOrchardServices Services { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        private dynamic Shape { get; set; }

        public ActionResult Index(PagerParameters pagerParameters, FaqEntriesIndexViewModel indexModel)
        {
            var cultureData = _countryCulturesProvider.GetData(indexModel.CountryId, indexModel.CultureId);

            FaqEntrySearchViewModel search = indexModel.Search;

            var sections = _faqService.GetFaqSections(cultureData.Culture);

            var faqQuery = _faqService.GetFaqEntries(cultureData.Culture, search.SectionId).Join<BodyPartRecord>().List().Where(fe => fe.Language == cultureData.Culture);

            if (!string.IsNullOrWhiteSpace(search.SearchString))
            {
                var searchString = search.SearchString.Trim();
                faqQuery = faqQuery.Where(f => f.Question.Contains(searchString) || f.Body.Text.Contains(searchString));
            }

            var entriesProjection = faqQuery.Select(e => { return Shape.FaqEntry(
                                         Id: e.Id,
                                         Question: e.Question,
                                         Text: e.Body.Text
                                    ); });
                                   
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize);
            var entries = entriesProjection.Skip(pager.GetStartIndex()).Take(pager.PageSize);           
            var pagerShape = Shape.Pager(pager).TotalItemCount(faqQuery.Count());

            var model = new FaqEntriesIndexViewModel(entries, sections, search, pagerShape);

            
            model.CountryId = cultureData.CountryId;
            model.CultureId = cultureData.CultureId;
            model.Countries = cultureData.Countries;
            model.Cultures = cultureData.Cultures;
            model.Culture = cultureData.Culture;

            return View(model);
        }

        public JsonResult GetCultureByCountry(int countryId)
        {
            var model = new FaqEntriesIndexViewModel();
            model.Cultures = _countryCulturesProvider.GetCountryCultures(countryId);

            return Json(model, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AddFaqEntry(string returnUrl, string culture)
        {
            FaqEntryPart faqEntryPart = Services.ContentManager.New<FaqEntryPart>("FaqEntry");
            if (faqEntryPart == null)
                return HttpNotFound();

            try
            {
                faqEntryPart.Section = _faqService.GetDefaultSection(culture);
                faqEntryPart.Language = culture;
                var model = Services.ContentManager.BuildEditor(faqEntryPart);
                return View(model);
            }
            catch (Exception exception)
            {
                Logger.Error(T("Creating FAQ entry failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Creating FAQ entry failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("AddFaqEntry")]
        public ActionResult AddFaqEntryPOST([Bind(Prefix = "FaqEntryPart.SectionId")] int section, 
            [Bind(Prefix = "Body.Text")] string text,
            [Bind(Prefix = "FaqEntryPart.Language")] string language, 
             string returnUrl)
        {
            var faqEntryPart = _faqService.CreateFaqEntry("", section, language);
            if (faqEntryPart == null)
                return HttpNotFound();

            var model = Services.ContentManager.UpdateEditor(faqEntryPart, this);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage))
                {
                    Services.Notifier.Error(T(error));
                }
                return View(model);
            }

            Services.Notifier.Information(T("New FAQ topic has been added."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult EditFaqEntry(int id)
        {
            var faqEntryPart = _faqService.GetFaqEntry(id);
           
            if (faqEntryPart == null)
                return new HttpNotFoundResult();

            var model = Services.ContentManager.BuildEditor(faqEntryPart);
            return View(model);
        }

        [HttpPost, ActionName("EditFaqEntry")]
        public ActionResult EditFaqEntryPOST(int id, FormCollection input, string returnUrl)
        {
            var faqEntryPart = _faqService.GetFaqEntry(id);

            faqEntryPart.Section = _faqService.GetFaqSectionById(int.Parse(input["FaqEntryPart.SectionId"]));

            var model = Services.ContentManager.UpdateEditor(faqEntryPart, this);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage))
                {
                    Services.Notifier.Error(T(error));
                }
                return View(model);
            }

            Services.Notifier.Information(T("The FAQ topic has been updated."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [HttpPost]
        public ActionResult Delete(int id, string returnUrl)
        {
            _faqService.DeleteFaqEntry(id);
            Services.Notifier.Information(T("The FAQ topic has been deleted."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }


        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties)
        {
            return base.TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage)
        {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
	}
}