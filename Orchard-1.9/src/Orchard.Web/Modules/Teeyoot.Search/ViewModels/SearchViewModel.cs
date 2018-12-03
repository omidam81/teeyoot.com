using System.Collections.Generic;
using Teeyoot.Module.Messaging.CampaignService;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Search.ViewModels
{
    public class SearchViewModel
    {
        public bool NotResult { get; set; }
        public string Filter { get; set; }
        public IEnumerable<SearchCampaignItem> Campaigns { get; set; }
        public PriceViewModel[] CampaignFirstProductPrices { get; set; }
        public int NewRow { get; set; }
        public bool NotFoundCategories { get; set; }
        public List<CampaignCategoriesRecord> CampCategList { get; set; }
    }
}
