using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Models;
using Orchard.Data;
using Orchard.Users.Models;
using System.Web.Mvc;

namespace Teeyoot.FeaturedCampaigns.ViewModels
{
    public class AdminFeaturedCampaignsViewModel
    {
        public FeaturedCampaignViewModel[] Campaigns { get; set; }

        public dynamic Pager { get; set; }

        public int StartedIndex { get; set; }

        public int NotApprovedTotal { get; set; }

        public IRepository<CurrencyRecord> Currencies { get; set; }

        public int FilterCurrencyId { get; set; }

    }

    public class FeaturedCampaignViewModel
    {
        public CampaignViewModel Campaign { get; set; }

        public int Last24HoursSold { get; set; }
    }

    public class AdminCampaignViewModel
    {
        public IQueryable<CampaignRecord> Items { get; set; }

        public dynamic Pager { get; set; }

        public CampaingSearch Search { get; set; }

        public Orchard.UI.Navigation.PagerParameters PagerParameters { get; set; }
    }


    public class CampaingSearch
    {
        public string campaignName { get; set; }
        public string Approved { get; set; }
        public string Active { get; set; }
        public string Featured { get; set; }
        public string ReadyToPrint { get; set; }
        public string Seller { get; set; }
        public string Currency { get; set; }

        public string sortBy { get; set; }
        public string orderbyOrder { get; set; }

        public int TotalSold { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string IsSuccessfull { get; set; }

        public List<SelectListItem> ActiveOption { get; set; }
        public List<SelectListItem> ApprovedOption { get; set; }
        public List<SelectListItem> FeaturedOption { get; set; }
        public List<SelectListItem> ReadyToPrintOption { get; set; }
        public List<SelectListItem> CurrecnyOption { get; set; }
    }

    public class CampaignViewModel
    {
        public int Id { get; set; }

        public bool IsFeatured { get; set; }

        public int Sold { get; set; }

        public int Goal { get; set; }

        public int Minimum { get; set; }

        public string Title { get; set; }

        public bool IsActive { get; set; }

        public bool IsApproved { get; set; }

        public bool Rejected { get; set; }

        public string Alias { get; set; }

        public DateTime CreatedDate { get; set; }

        public CurrencyRecord Currency { get; set; }

        public int? FilterCurrencyId { get; set; }

        public UserPartRecord Seller { get; set; }
    }
}