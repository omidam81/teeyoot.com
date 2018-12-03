using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Models;
using Teeyoot.FeaturedCampaigns.Services;
using Orchard.ContentManagement.Drivers;
using Teeyoot.Module.Services;
using Orchard;
using Teeyoot.Module.Services.Interfaces;
using Orchard.Themes;
using System.Web.Mvc;
using Orchard.Data;

namespace Teeyoot.FeaturedCampaigns.Drivers
{
    public class FeaturedCampaignsWidget : ContentPartDriver<FeaturedCampaignsWidgetPart>
    {
        private readonly ICampaignService _campaignsService;
        private readonly IFeaturedCampaignsService _featuredCampaignsService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IMultiCountryService _countries;
        private readonly IRepository<CurrencyRecord> _currencies;
        private readonly IPriceConversionService _priceconvertorservice;

        public FeaturedCampaignsWidget(ICampaignService campaignsService, IFeaturedCampaignsService featuredCampaignsService, IWorkContextAccessor workContextAccessor, IMultiCountryService countries, IRepository<CurrencyRecord> currencies,
            IPriceConversionService priceconvertorservice
            )
        {
            _campaignsService = campaignsService;
            _featuredCampaignsService = featuredCampaignsService;

            _workContextAccessor = workContextAccessor;
            _countries = countries;
            _currencies = currencies;
            _priceconvertorservice = priceconvertorservice;
        }
        [Themed, OutputCache(NoStore = true, Duration = 0)]
        protected override DriverResult Display(FeaturedCampaignsWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var culture = _workContextAccessor.GetContext().CurrentCulture.Trim();

            var campaignsInFeatured = _campaignsService.GetAllCampaigns().Where(c => c.IsFeatured && !c.IsPrivate && c.IsActive && c.IsApproved).OrderByDescending(c => c.ProductCountSold).ToList();
            var featuredCampaigns = new List<CampaignRecord>();
            if (campaignsInFeatured.Count >= 8)
            {
                featuredCampaigns = campaignsInFeatured;
            }
            else
            {
                featuredCampaigns = campaignsInFeatured;
                int countTopCamp = 8 - campaignsInFeatured.Count;
                var ordersFromOneDay = _featuredCampaignsService.GetOrderForOneDay().Where(c => c.Products != null && c.Products.Count > 0).ToList();
                if (ordersFromOneDay != null && ordersFromOneDay.Count > 0)
                {
                    int[] ordersIdFromOneDay = ordersFromOneDay.Select(c => c.Id).ToArray();
                    Dictionary<CampaignRecord, int> campaignsFromOrderForDay = _featuredCampaignsService.GetCampaignsFromOrderForOneDay(ordersIdFromOneDay);

                    int take = campaignsFromOrderForDay.Count > 16 ? 16 : campaignsFromOrderForDay.Count;
                    campaignsFromOrderForDay = campaignsFromOrderForDay.OrderByDescending(c => c.Value).OrderBy(c => c.Key.Title).Skip(0).Take(take).ToDictionary(p => p.Key, p => p.Value);

                    Random rand = new Random();
                    int insertCamp = campaignsFromOrderForDay.Count() <= countTopCamp ? campaignsFromOrderForDay.Count() : countTopCamp;
                    for (int i = 0; i < insertCamp; i++)
                    {
                        var campNum = rand.Next(take);
                        var campKey = campaignsFromOrderForDay.ElementAt(campNum).Key;
                        if (!featuredCampaigns.Contains(campKey))
                        {
                            featuredCampaigns.Add(campKey);
                        }

                    }
                }

                if (featuredCampaigns.Count() < 8)
                {
                    countTopCamp = 8 - featuredCampaigns.Count();
                    var otherCampaigns = _campaignsService.GetAllCampaigns().Where(c => !c.IsPrivate && c.IsActive && c.IsApproved).ToList();
                    foreach (var camp in campaignsInFeatured)
                    {
                        if (otherCampaigns.Exists(c => c.Id == camp.Id))
                        {
                            otherCampaigns.Remove(camp);
                        }
                    }
                    int max = otherCampaigns.Count();
                    if ((max + featuredCampaigns.Count()) < 8)
                    {
                        featuredCampaigns.AddRange(otherCampaigns.ToArray());

                    }
                    else
                    {

                        Random rand = new Random();
                        for (int i = 0; i < countTopCamp; i++)
                        {
                            var res = false;
                            while (!res)
                            {
                                var camp = otherCampaigns.ElementAt(rand.Next(max));
                                if (!featuredCampaigns.Exists(c => c.Id == camp.Id))
                                {
                                    featuredCampaigns.Add(camp);
                                    
                                    res = true;
                                }
                            }
                        }
                    }
                }
            }
            var currenciesandprices = new Dictionary<int, Dictionary<string,double>>();

            foreach (var c in featuredCampaigns)
            {
                Dictionary<string, double> prices = new Dictionary<string, double>();
                foreach (var item in _currencies.Table)
                {
                    var price = c.Products.First().Price;
                    prices.Add(item.Code , _priceconvertorservice.ConvertPrice(price, c.CurrencyRecord, item).Value);
                    
                }

                
                currenciesandprices.Add(c.Id, prices);
            }
            return ContentShape("Parts_FeaturedCampaignsWidget", () =>
                shapeHelper.Parts_FeaturedCampaignsWidget(Campaigns: featuredCampaigns, currency: _countries.GetDefaultCurrecny().Code, prices: currenciesandprices));
        }
    }
}