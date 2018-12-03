using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Messaging.ViewModels;
using Mandrill;
using Mandrill.Model;
using System.Collections.Specialized;
using Orchard.Data;
using Teeyoot.Module.Services.Interfaces;
using Orchard.Localization.Records;


namespace Teeyoot.Module.Controllers
{
    [Admin]
    public class AdminMessageController : Controller, IUpdateModel
    {
        private readonly IMailChimpSettingsService _settingsService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<MailTemplateSubjectRecord> _mailTemplateSubjectRecordRepository;

        private readonly ICountryService _countryService;
        private readonly IRepository<CultureRecord> _cultureRepository;

        public AdminMessageController(
            IMailChimpSettingsService settingsService, 
            IOrchardServices services, 
            IRepository<MailTemplateSubjectRecord> mailTemplateSubjectRecordRepository, 
            IWorkContextAccessor workContextAccessor,
            ICountryService countryService,
            IRepository<CultureRecord> cultureRepository)
        {
            _settingsService = settingsService;
            Services = services;
            _mailTemplateSubjectRecordRepository = mailTemplateSubjectRecordRepository;
            _workContextAccessor = workContextAccessor;

            _countryService = countryService;
            _cultureRepository = cultureRepository; 
        }

        private IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(MailChimpListViewModel model)
        {
            int countryId = 0;
            int cultureId = 0;
            IList<CountryRecord> countries;
            IList<CultureRecord> cultures = null;
            string culture = "en-MY";

            countries = _countryService.GetAllCountry().ToList();
            if (model.CountryId > 0)
            {
                cultures = _countryService.GetCultureByCountry(model.CountryId).ToList();
                countryId = model.CountryId;
                cultureId = model.CultureId;
            }
            else
            {
                if (countries.Count > 0)
                {
                    cultures = _countryService.GetCultureByCountry(countries.First().Id).ToList();
                    countryId = countries.First().Id;
                    if (cultures.Count > 0)
                    {
                        cultureId = cultures.First().Id;
                    }
                }
            }
            var cultureRecord = _cultureRepository.Get(model.CultureId == 0 ? cultureId : model.CultureId);
            if (cultureRecord != null)
            {
                culture = cultureRecord.Culture;
            }


            MailChimpListViewModel setting;
            var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/" + culture + "/");
            var settings = _settingsService.GetSettingByCulture(culture).List().Select(s => new MailChimpListViewModel
            {
                Id = s.Id,
                ApiKey = s.ApiKey,
                SellerTemplate = System.IO.File.Exists(pathToTemplates + "seller-template.html") ? "seller-template.html" : "No file!",
                WelcomeTemplate = System.IO.File.Exists(pathToTemplates +  "welcome-template.html") ? "welcome-template.html" : "No file!",
                RelaunchApprovedSellerTemplate = System.IO.File.Exists(pathToTemplates +  "relaunch.html") ? "relaunch.html" : "No file!",
                RelaunchApprovedBuyerTemplate = System.IO.File.Exists(pathToTemplates +  "relaunch-buyer.html") ? "relaunch-buyer.html" : "No file!",
                RelaunchAdminSellerTemplate = System.IO.File.Exists(pathToTemplates +  "relaunch-to-admin-seller.html") ? "relaunch-to-admin-seller.html" : "No file!",
                LaunchTemplate = System.IO.File.Exists(pathToTemplates +  "launch-template.html") ? "launch-template.html" : "No file!",
                WithdrawTemplate = System.IO.File.Exists(pathToTemplates +  "withdraw-template.html") ? "withdraw-template.html" : "No file!",
                PlaceOrderTemplate = System.IO.File.Exists(pathToTemplates +  "place-order-template.html") ? "place-order-template.html" : "No file!",
                NewOrderTemplate = System.IO.File.Exists(pathToTemplates +  "new-order-template.html") ? "new-order-template.html" : "No file!",
                ShippedOrderTemplate = System.IO.File.Exists(pathToTemplates +  "shipped-order-template.html") ? "shipped-order-template.html" : "No file!",
                DeliveredOrderTemplate = System.IO.File.Exists(pathToTemplates +  "delivered-order-template.html") ? "delivered-order-template.html" : "No file!",
                CancelledOrderTemplate = System.IO.File.Exists(pathToTemplates +  "cancelled-order-template.html") ? "cancelled-order-template.html" : "No file!",
                OrderIsPrintingBuyerTemplate = System.IO.File.Exists(pathToTemplates +  "order-is-printing-buyer-template.html") ? "order-is-printing-buyer-template" + culture + ".html" : "No file!",
                CampaignIsPrintingSellerTemplate = System.IO.File.Exists(pathToTemplates +  "campaign-is-printing-seller-template.html") ? "campaign-is-printing-seller-template.html" : "No file!",
                PaidCampaignTemplate = System.IO.File.Exists(pathToTemplates +  "paid-campaign-template.html") ? "paid-campaign-template.html" : "No file!",
                UnpaidCampaignTemplate = System.IO.File.Exists(pathToTemplates +  "unpaid-campaign-template.html") ? "unpaid-campaign-template.html" : "No file!",
                CampaignNotReachGoalBuyerTemplate = System.IO.File.Exists(pathToTemplates +  "not-reach-goal-buyer-template.html") ? "not-reach-goal-seller-template.html" : "No file!",
                CampaignNotReachGoalSellerTemplate = System.IO.File.Exists(pathToTemplates +  "not-reach-goal-seller-template.html") ? "not-reach-goal-buyer-template.html" : "No file!",
                //PartiallyPaidCampaignTemplate = System.IO.File.Exists(pathToTemplates + "partially-paid-campaign-template.html") ? "partially-paid-campaign-template.html" : "No file!",
                CampaignPromoTemplate = System.IO.File.Exists(pathToTemplates +  "campaign-promo-template.html") ? "campaign-promo-template.html" : "No file!",
                AllOrderDeliveredTemplate = System.IO.File.Exists(pathToTemplates +  "all-orders-delivered-seller-template.html") ? "all-orders-delivered-seller-template.html" : "No file!",
                //CampaignIsFinishedTemplate = System.IO.File.Exists(pathToTemplates + "campaign-is-finished-template.html") ? "campaign-is-finished-template.html" : "No file!",
                DefinitelyGoSellerTemplate = System.IO.File.Exists(pathToTemplates +  "definitely-go-to-print-seller-template.html") ? "definitely-go-to-print-seller-template.html" : "No file!",
                DefinitelyGoBuyerTemplate = System.IO.File.Exists(pathToTemplates +  "definitely-go-to-print-buyer-template.html") ? "definitely-go-to-print-buyer-template.html" : "No file!",
                EditedCampaignTemplate = System.IO.File.Exists(pathToTemplates +  "edited-campaign-template.html") ? "edited-campaign-template.html" : "No file!",
                ExpiredMetMinimumTemplate = System.IO.File.Exists(pathToTemplates +  "expired-campaign-met-minimum-admin-template.html") ? "expired-campaign-met-minimum-admin-template.html" : "No file!",
                ExpiredNotSuccessfullTemplate = System.IO.File.Exists(pathToTemplates +  "expired-campaign-notSuccessfull-admin-template.html") ? "expired-campaign-notSuccessfull-admin-template.html" : "No file!",
                ExpiredSuccessfullTemplate = System.IO.File.Exists(pathToTemplates +  "expired-campaign-successfull-admin-template.html") ? "expired-campaign-successfull-admin-template.html" : "No file!",
                MakeTheCampaignTemplate = System.IO.File.Exists(pathToTemplates +  "make_the_campaign_seller.html") ? "make_the_campaign_seller.html" : "No file!",
                NewCampaignAdminTemplate = System.IO.File.Exists(pathToTemplates +  "new-campaign-admin-template.html") ? "new-campaign-admin-template.html" : "No file!",
                //NewOrderBuyerTemplate = System.IO.File.Exists(pathToTemplates + "new-order-buyer-template.html") ? "new-order-buyer-template.html" : "No file!",
                NotReachGoalMetMinimumTemplate = System.IO.File.Exists(pathToTemplates +  "not-reach-goal-met-minimum-seller-template.html") ? "not-reach-goal-met-minimum-seller-template.html" : "No file!",
                RecoverOrdersTemplate = System.IO.File.Exists(pathToTemplates +  "recover_orders_for_buyer.html") ? "recover_orders_for_buyer.html" : "No file!",
                RejectTemplate = System.IO.File.Exists(pathToTemplates +  "reject-template.html") ? "reject-template.html" : "No file!",
                Shipped3DayAfterTemplate = System.IO.File.Exists(pathToTemplates +  "shipped-order-3day-after-template.html") ? "shipped-order-3day-after-template.html" : "No file!",
                TermsConditionsTemplate = System.IO.File.Exists(pathToTemplates +  "terms-conditions-template.html") ? "terms-conditions-template.html" : "No file!",
                WithdrawCompletedTemplate = System.IO.File.Exists(pathToTemplates +  "withdraw-completed-template.html") ? "withdraw-completed-template.html" : "No file!",
                WithdrawSellerTemplate = System.IO.File.Exists(pathToTemplates +  "withdraw-seller-template.html") ? "withdraw-seller-template.html" : "No file!"
            });
            if (settings.FirstOrDefault() == null)
            {
                setting = new MailChimpListViewModel();
            }
            else
            {
                setting = settings.FirstOrDefault();
            }

            setting.Countries = countries;
            setting.Cultures = cultures;
            setting.CountryId = countryId;
            setting.CultureId = cultureId;
            setting.Culture = culture;

            return View(setting);
        }

        public JsonResult GetCultureByCountry(int countryId)
        {
            var model = new MailChimpListViewModel();
            model.Cultures = _countryService.GetCultureByCountry(countryId).ToList();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddSetting(string returnUrl)
        {
            MailChimpSettingsPart mailChimpSettingsPart = Services.ContentManager.New<MailChimpSettingsPart>("MailChimpSettings");

            if (mailChimpSettingsPart == null)
                return HttpNotFound();
            try
            {
                var model = Services.ContentManager.BuildEditor(mailChimpSettingsPart);
                return View(model);
            }
            catch (Exception exception)
            {
                Logger.Error(T("Creating setting failed: {0}", exception.Message).Text);
                Services.Notifier.Error(T("Creating setting failed: {0}", exception.Message));
                return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
            }
        }

        [HttpPost, ActionName("AddSetting")]
        public ActionResult AddSettingPOST(string returnUrl, string culture)
        {
            Uri myUri = new Uri(returnUrl);
            var mailChimpSettingPart = _settingsService.CreateMailChimpSettingsPart("", culture);
            if (mailChimpSettingPart == null)
                return HttpNotFound();

            var model = Services.ContentManager.UpdateEditor(mailChimpSettingPart, this);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage))
                {
                    Services.Notifier.Error(T(error));
                }
                return View(model);
            }

            Services.Notifier.Information(T("New setting has been added."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }


        public ActionResult EditMailChimpSetting(int id)
        {
            var mailChimpSettingPart = _settingsService.GetSetting(id);
            if (mailChimpSettingPart == null)
                return new HttpNotFoundResult();

            var model = Services.ContentManager.BuildEditor(mailChimpSettingPart);
            return View(model);
        }

        [HttpPost, ActionName("EditMailChimpSetting")]
        public ActionResult EditMailChimpSettingPOST(int id, FormCollection input, string returnUrl)
        {
            var mailChimpSettingPart = _settingsService.GetSetting(id);

            var model = Services.ContentManager.UpdateEditor(mailChimpSettingPart, this);

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage))
                {
                    Services.Notifier.Error(T(error));
                }
                return View(model);
            }

            Services.Notifier.Information(T("The setting has been updated."));
            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult Delete(string templateName, string returnUrl, string culture)
        {
            System.IO.File.Delete(Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/") + templateName +"-" + culture + ".html");
            Services.Notifier.Information(T("File has been deleted."));
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

        [HttpPost]
        public RedirectToRouteResult UploadTemplate(HttpPostedFileBase file, string templateName, string culture)
        {
            if (file != null && file.ContentLength > 0)
            {
                string[] allowed = { ".html" };
                var extension = System.IO.Path.GetExtension(file.FileName);
                if (allowed.Contains(extension))
                {
                    string fileExt = Path.GetExtension(file.FileName);
                    var path = Path.Combine(Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/"), culture, templateName + fileExt);
                    file.SaveAs(path);
                    Services.Notifier.Information(T("File has been added!"));
                    return RedirectToAction("Index");
                }
                else
                {
                    Services.Notifier.Error(T("Wrong file extension!"));  
                }
            }
            else
            {
                Services.Notifier.Error(T("Error! No file selected to upload."));             
            }
            return RedirectToAction("Index");
        }

        public void Download(string fileName, string culture)
        {
            if (string.IsNullOrWhiteSpace(culture)) culture = "en-MY";
            var newfileName = culture + "/" + fileName + ".html";
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            Response.ContentType = "text/HTML";
            String Header = "Attachment; Filename=" + newfileName;
            Response.AppendHeader("Content-Disposition", Header);
            System.IO.FileInfo Dfile = new System.IO.FileInfo(pathToTemplates + newfileName);
            Response.WriteFile(Dfile.FullName);
            Response.End();
        }

        [HttpGet]
        public ActionResult EditEmailTemplateSubject(string templateName, string culture)
        {
            var subject = _mailTemplateSubjectRecordRepository.Table
                .FirstOrDefault(s => s.TemplateName == templateName && s.Culture == culture);

            var viewModel = new EditEmailTemplateSubjectViewModel
            {
                TemplateName = templateName,
                Subject = subject == null ? "" : subject.Subject,
                Culture = culture
            };

            return PartialView("EditEmailTemplateSubject", viewModel);
        }

        [HttpPost]
        public ActionResult EditEmailTemplateSubject(EditEmailTemplateSubjectViewModel viewModel)
        {
            var subject = _mailTemplateSubjectRecordRepository.Table
                .FirstOrDefault(s => s.TemplateName == viewModel.TemplateName && s.Culture == viewModel.Culture);
            var subjectToCreateOrUpdate = subject ?? new MailTemplateSubjectRecord();

            subjectToCreateOrUpdate.TemplateName = viewModel.TemplateName;
            subjectToCreateOrUpdate.Culture = viewModel.Culture;
            subjectToCreateOrUpdate.Subject = viewModel.Subject;

            if (subject == null)
            {
                _mailTemplateSubjectRecordRepository.Create(subjectToCreateOrUpdate);
            }
            else
            {
                _mailTemplateSubjectRecordRepository.Update(subjectToCreateOrUpdate);
            }

            return RedirectToAction("Index");
        }
    }
}