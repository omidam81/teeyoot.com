using System;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Localization.Records;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Module.Controllers
{
    [Admin]
    public class AdminCountriesController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CountryRecord> _countryRepository;
        private readonly IRepository<CultureRecord> _cultureRepository;

        private dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ILogger Logger { get; set; }

        public AdminCountriesController(
            ISiteService siteService,
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IRepository<CountryRecord> countryRepository,
            IRepository<CultureRecord> cultureRepository)
        {
            _siteService = siteService;
            _orchardServices = orchardServices;
            Shape = shapeFactory;
            _countryRepository = countryRepository;
            _cultureRepository = cultureRepository;

            Logger = NullLogger.Instance;
        }

        public ActionResult Index(PagerParameters pagerParameters)
        {
            var viewModel = new CountriesViewModel();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize);

            var countries = _countryRepository.Table
                .FetchMany(c => c.CountryCultures)
                .ThenFetch(c => c.CultureRecord)
                .OrderBy(c => c.Name)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            var countryViewModels = countries.Select(c =>
            {
                var countryViewModel = new CountryViewModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Cultures = c.CountryCultures.Select(countryCulture => new SelectedCultureItem
                    {
                        Culture = countryCulture.CultureRecord.Culture
                    })
                };

                return countryViewModel;
            });

            var countriesTotalCount = _countryRepository.Table.Count();

            viewModel.Countries = countryViewModels;

            var pagerShape = Shape.Pager(pager).TotalItemCount(countriesTotalCount);
            viewModel.Pager = pagerShape;

            return View(viewModel);
        }

        public ActionResult AddCountry()
        {
            var viewModel = new CountryViewModel();

            var cultures = _cultureRepository.Table
                .Select(c => new SelectedCultureItem
                {
                    Id = c.Id,
                    Culture = c.Culture
                });

            viewModel.Cultures = cultures;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult AddCountry(CountryViewModel viewModel)
        {
            var country = new CountryRecord
            {
                Code = viewModel.Code,
                Name = viewModel.Name
            };

            if (viewModel.SelectedCultures != null)
            {
                foreach (var cultureId in viewModel.SelectedCultures)
                {
                    var culture = _cultureRepository.Get(cultureId);

                    var countryCulture = new LinkCountryCultureRecord
                    {
                        CountryRecord = country,
                        CultureRecord = culture
                    };

                    country.CountryCultures.Add(countryCulture);
                }
            }

            var defaultCulture = _cultureRepository.Get(viewModel.DefaultCultureId);

            var defaultCountryCulture = country.CountryCultures
                .FirstOrDefault(c => c.CultureRecord == defaultCulture);

            if (defaultCountryCulture == null)
            {
                defaultCountryCulture = new LinkCountryCultureRecord
                {
                    CountryRecord = country,
                    CultureRecord = defaultCulture
                };

                country.CountryCultures.Add(defaultCountryCulture);
            }

            country.DefaultCulture = defaultCountryCulture;

            _countryRepository.Create(country);

            _orchardServices.Notifier.Information(T("Country has been added."));
            return RedirectToAction("Index");
        }

        public ActionResult DeleteCountry(int id)
        {
            var country = _countryRepository.Get(id);

            try
            {
                _countryRepository.Delete(country);
                _countryRepository.Flush();
            }
            catch (Exception)
            {
                _orchardServices.TransactionManager.Cancel();
                _orchardServices.Notifier.Error(T("Error deleting country!"));
                return RedirectToAction("Index");
            }

            _orchardServices.Notifier.Information(T("Country has been deleted."));
            return RedirectToAction("Index");
        }

        public ActionResult EditCountry(int id)
        {
            var country = _countryRepository.Get(id);

            var viewModel = new CountryViewModel
            {
                Id = country.Id,
                Code = country.Code,
                Name = country.Name
            };

            var cultures = _cultureRepository.Table.ToList();

            var cultureItemViewModels = cultures
                .Select(c => new SelectedCultureItem
                {
                    Id = c.Id,
                    Culture = c.Culture,
                    Selected = country.CountryCultures
                        .Any(countryCulture => countryCulture.CultureRecord == c)
                })
                .ToList();

            viewModel.Cultures = cultureItemViewModels;
            viewModel.DefaultCultureId = country.DefaultCulture.CultureRecord.Id;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult EditCountry(CountryViewModel viewModel)
        {
            var country = _countryRepository.Get(viewModel.Id);

            country.Code = viewModel.Code;
            country.Name = viewModel.Name;

            country.CountryCultures.Clear();

            if (viewModel.SelectedCultures != null)
            {
                foreach (var cultureId in viewModel.SelectedCultures)
                {
                    var culture = _cultureRepository.Get(cultureId);

                    var countryCulture = new LinkCountryCultureRecord
                    {
                        CountryRecord = country,
                        CultureRecord = culture
                    };

                    country.CountryCultures.Add(countryCulture);
                }
            }

            var defaultCulture = _cultureRepository.Get(viewModel.DefaultCultureId);

            var defaultCountryCulture = country.CountryCultures
                .FirstOrDefault(c => c.CultureRecord == defaultCulture);

            if (defaultCountryCulture == null)
            {
                defaultCountryCulture = new LinkCountryCultureRecord
                {
                    CountryRecord = country,
                    CultureRecord = defaultCulture
                };

                country.CountryCultures.Add(defaultCountryCulture);
            }

            country.DefaultCulture = defaultCountryCulture;

            _orchardServices.Notifier.Information(T("Country has been edited."));
            return RedirectToAction("Index");
        }
    }
}
