using System.Collections.Generic;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.ViewModels
{
    public class CampaignIndexViewModel
    {
        private readonly IPriceConversionService _priceConversionService;

        public IPriceConversionService PriceConversionService
        {
            get { return _priceConversionService; }
        }

        public CampaignIndexViewModel(
            IPriceConversionService priceConversionService)
        {
            _priceConversionService = priceConversionService;
        }

        public CampaignRecord Campaign { get; set; }
        public string PromoId { get; set; }
        public int CntRequests { get; set; }
        public double PromoSize { get; set; }
        public string PromoType { get; set; }
        public string FBDescription { get; set; }

        public string SellerFbPixel { get; set; }
        public string FacebookCustomAudiencePixel { get; set; }

        public Dictionary<int, Dictionary<string, double>>  Prices { get; set; }

        public string currency { get; set; }

        public string exchangeRate { get; set; }
    }
}
