using System;
using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Teeyoot.Module.Models
{
    public class CampaignRecord
    {
        public virtual int Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Alias { get; set; }
        public virtual int ProductCountGoal { get; set; }
        public virtual int ProductCountSold
        {
            get;
            set;
        }
        public virtual int ProductMinimumGoal { get; set; }

        //[StringLengthMax]
        //public virtual string Design { get; set; }

        [StringLengthMax]
        public virtual string Description { get; set; }

        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual string URL { get; set; }
        public virtual bool IsForCharity { get; set; }
        public virtual bool BackSideByDefault { get; set; }
        public virtual int? TeeyootUserId { get; set; }
        public virtual CampaignStatusRecord CampaignStatusRecord { get; set; }
        public virtual bool IsFeatured { get; set; }
        public virtual bool IsActive { get; set; }
        public virtual bool IsApproved { get; set; }
        public virtual bool IsArchived { get; set; }
        public virtual int? BaseCampaignId { get; set; }
        public virtual bool Rejected { get; set; }
        public virtual DateTime? WhenDeleted { get; set; }
        public virtual DateTime? WhenApproved { get; set; }
        public virtual bool IsPrivate { get; set; }
        public virtual int CntFrontColor { get; set; }
        public virtual int CntBackColor { get; set; }
        public virtual string CampaignProfit { get; set; }
        public virtual string CampaignCulture { get; set; }
        public virtual CountryRecord CountryRecord { get; set; }
        public virtual CurrencyRecord CurrencyRecord { get; set; }
        public virtual IList<CampaignProductRecord> Products { get; set; }
        public virtual IList<LinkCampaignAndCategoriesRecord> Categories { get; set; }
        public virtual IList<BringBackCampaignRecord> BackCampaign { get; set; }

        public virtual Orchard.Users.Models.UserPartRecord Seller { get; set; }

        public virtual CampaignDesign CampaignDesign { get; set; }

        public virtual string FBPixelId { get; set; }
        public virtual string PinterestPixelId { get; set; }
        public virtual string GooglePixelId { get; set; }

        public virtual int NumOfApprovedButNotDeliverd { get; set; }

        public virtual bool CampaignEndAndDelivered { get; set; }
        public virtual bool CampaignEndButNotDeliverd { get; set; }

        public virtual double UnclaimableProfit { get; set; }
        public virtual double ClaimableProfit { get; set; }


        public virtual TShirtCostRecord TShirtCostRecord { get; set; }



        public CampaignRecord()
        {
            Products = new List<CampaignProductRecord>();
            Categories = new List<LinkCampaignAndCategoriesRecord>();
            BackCampaign = new List<BringBackCampaignRecord>();
        }



        public virtual int SoldLast24Hours { get; set; }
        public virtual int SoldYesterDay { get; set; }
        public virtual int TotalSold { get; set; }
        public virtual int ActiveSold { get; set; }



        public virtual double Last24HoursProfit { get; set; }
        public virtual double ActiveProfit { get; set; }
        public virtual double TotalProfit { get; set; }
        public virtual double YesterDayProfit { get; set; }


        public virtual int Tareget { get; set; }

        public virtual bool TargetSet { get; set; }
    }
}
