using System;

namespace Teeyoot.Module.Messaging.CampaignService
{
    public class SearchCampaignItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public DateTime EndDate { get; set; }
        public string Url { get; set; }
        public int ProductCountSold { get; set; }
        public int ProductMinimumGoal { get; set; }
        public bool BackSideByDefault { get; set; }
        public int CampaignFirstProductId { get; set; }
        public double CampaignFirstProductPrice { get; set; }
        public int CampaignCurrencyId { get; set; }
        public string FlagFileName { get; set; }
    }
}
