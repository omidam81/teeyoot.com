﻿using System.Collections.Generic;
using System.Linq;
using Orchard;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Messaging.CampaignService;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface ICampaignService : IDependency
    {
        SearchCampaignsResponse SearchCampaigns(SearchCampaignsRequest request);
        SearchCampaignsResponse SearchCampaignsForTag(SearchCampaignsRequest request);
        SearchCampaignsResponse SearchCampaignsForFilter(SearchCampaignsRequest request);
        IQueryable<CampaignRecord> GetAllCampaigns();
        CampaignRecord GetCampaignByAlias(string alias);
        CampaignRecord GetCampaignById(int id);
        List<CampaignRecord> GetCampaignsForTheFilter(string filter, int skip = 0, int take = 16, bool tag = false);
        CampaignRecord CreateNewCampiagn(LaunchCampaignData data);
        IQueryable<CampaignProductRecord> GetProductsOfCampaign(int campaignId);
        IQueryable<CampaignRecord> GetCampaignsOfUser(int userId);
        CampaignProductRecord GetCampaignProductById(int id);
        IQueryable<CampaignCategoriesRecord> GetAllCategories();
        void UpdateCampaign(CampaignRecord campiagn);
        bool DeleteCampaignFromCategoryById(int campId, int categId);
        void CheckExpiredCampaigns();
        IQueryable<CampaignProductRecord> GetAllCampaignProducts();
        bool DeleteCampaign(int id);
        bool PrivateCampaign(int id, bool change);
        void SetCampaignStatus(int id, CampaignStatus status);
        void ReservCampaign(int id, string email);
        IQueryable<BringBackCampaignRecord> GetReservedRequestsOfCampaign(int id);
        void CalculateCampaignProfit(int campaignID, bool save = false);
        CampaignRecord ReLaunchCampiagn(int productCountGoal, string campaignProfit, int campaignLength, int minimum,
            RelaunchProductInfo[] baseCost, int id);
        void CreatePayoutData(int campaignId);
        IQueryable<string> GetBuyersEmailOfReservedCampaign(int id);
        int GetCountOfReservedRequestsOfCampaign(int id);
        float CalculateBaseCost(int campaignID, int productID, int soldcount);
        void UpdateCampaginSoldCount(int campaignId);
    }
}
