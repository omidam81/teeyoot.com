using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard.Data;
using Orchard.Themes;
using Teeyoot.Module.Messaging.CampaignService;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;
using Teeyoot.Search.Services;
using Teeyoot.Search.ViewModels;

namespace Teeyoot.Search.Controllers
{
    [Themed]
    public class SearchController : Controller
    {
        private readonly IPriceConversionService _priceConversionService;
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        private readonly ICampaignService _campaignService;
        private readonly ICampaignCategoriesService _campaignCategoriesService;

        private const int Take = 16;

        private List<SearchCampaignItem> _searchCampaignItems;

        public SearchController(
            IPriceConversionService priceConversionService,
            IRepository<CurrencyRecord> currencyRepository,
            ICampaignService campaignService,
            ICampaignCategoriesService campaignCategoriesService)
        {
            _priceConversionService = priceConversionService;
            _currencyRepository = currencyRepository;
            _campaignService = campaignService;
            _campaignCategoriesService = campaignCategoriesService;
        }

        [HttpGet]
        [Themed, OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Search(string filter, int? page, string showall, string checkfilter)
        {
            if (string.IsNullOrWhiteSpace(filter) && showall != "true" && !page.HasValue && checkfilter != "no")
            {
                ViewBag.NoSearchTerm = true;
                ViewBag.Message = "Nothing to search for, Please enter something to search for";
                return View(new SearchViewModel());
                
            }
            page = page ?? 0;
            var skip = (int)page * Take;

            filter = filter.Trim();

            SearchCampaignsResponse searchCampaignsResponse;

            if (!string.IsNullOrEmpty(filter))
            {
                var searchCampaignsRequest = new SearchCampaignsRequest
                {
                    Filter = filter,
                    Skip = skip,
                    Take = Take
                };

                searchCampaignsResponse = _campaignService.SearchCampaignsForFilter(searchCampaignsRequest);
                _searchCampaignItems = searchCampaignsResponse.Campaigns;

                if (_searchCampaignItems.Count == 0 && (page == null || page == 0))
                {

                    ViewBag.NoResult = true;


                    searchCampaignsRequest.Filter = "";
                    filter = "";
                    searchCampaignsRequest = new SearchCampaignsRequest
                    {
                        Skip = skip,
                        Take = Take
                    };

                    searchCampaignsResponse = _campaignService.SearchCampaigns(searchCampaignsRequest);
                    _searchCampaignItems = searchCampaignsResponse.Campaigns;
                }
            }
            else
            {
                var searchCampaignsRequest = new SearchCampaignsRequest
                {
                    Skip = skip,
                    Take = Take
                };

                searchCampaignsResponse = _campaignService.SearchCampaigns(searchCampaignsRequest);
                _searchCampaignItems = searchCampaignsResponse.Campaigns;
            }

            var campaignFirstProductPrices = GetCampaignFirstProductPrices(_searchCampaignItems);

            if (Request.IsAjaxRequest())
            {
                var searchViewModel = new SearchViewModel
                {
                    NotResult = !_searchCampaignItems.Any(),
                    Filter = filter,
                    Campaigns = _searchCampaignItems,
                    CampaignFirstProductPrices = campaignFirstProductPrices.ToArray()
                };

                return PartialView("_CustomerRow", searchViewModel);
            }
            else
            {
                var searchViewModel = new SearchViewModel
                {
                    NotResult = !_searchCampaignItems.Any(),
                    Filter = filter,
                    Campaigns = _searchCampaignItems,
                    CampaignFirstProductPrices = campaignFirstProductPrices.ToArray()
                };

                return View(searchViewModel);
            }
        }

        public ActionResult CategoriesSearch(string categoriesName)
        {
            categoriesName = categoriesName.Trim();

            var campCategList = _campaignCategoriesService.GetAllCategories().ToList();
            var findCampCateg = campCategList.Find(x => x.Name.ToLower() == categoriesName.ToLower());
            var notFoundCateg = false;

            if (findCampCateg != null)
            {
                var searchCampaignsRequest = new SearchCampaignsRequest
                {
                    Tag = categoriesName.ToLowerInvariant(),
                    Skip = 0,
                    Take = Take
                };

                var searchCampaignsResponse = _campaignService.SearchCampaignsForTag(searchCampaignsRequest);
                _searchCampaignItems = searchCampaignsResponse.Campaigns;

                campCategList.Remove(findCampCateg);
            }
            else
            {
                notFoundCateg = true;
            }

            var campaignFirstProductPrices = GetCampaignFirstProductPrices(_searchCampaignItems);

            var searchViewModel = new SearchViewModel
            {
                NotResult = !_searchCampaignItems.Any(),
                Filter = categoriesName,
                Campaigns = _searchCampaignItems,
                NewRow = 0,
                NotFoundCategories = notFoundCateg,
                CampCategList = campCategList,
                CampaignFirstProductPrices = campaignFirstProductPrices.ToArray()
            };

            return View(searchViewModel);
        }

        private IEnumerable<PriceViewModel> GetCampaignFirstProductPrices(
            IEnumerable<SearchCampaignItem> searchCampaignItems)
        {
            var currencies = _currencyRepository.Table.ToList();

            var prices = new List<PriceViewModel>();

            foreach (var searchCampaignItem in searchCampaignItems)
            {
                var currencyFrom = currencies.First(c => c.Id == searchCampaignItem.CampaignCurrencyId);

                var price = _priceConversionService
                    .ConvertPrice(searchCampaignItem.CampaignFirstProductPrice, currencyFrom);

                var priceViewModel = new PriceViewModel
                {
                    Price = price.Value,
                    CurrencyCode = price.Currency.Code
                };

                prices.Add(priceViewModel);
            }

            return prices;
        }
    }
}