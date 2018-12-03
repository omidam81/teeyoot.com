using Orchard;
using Orchard.DisplayManagement;
using Orchard.Settings;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teeyoot.FeaturedCampaigns.Services;
using Teeyoot.Module.Models;
using Teeyoot.FeaturedCampaigns.ViewModels;
using Orchard.UI.Navigation;
using Orchard.Localization;
using Teeyoot.Module.Services;
using Orchard.Logging;
using Teeyoot.Module.Common.Enums;
using Orchard.Data;
using Teeyoot.Module.Services.Interfaces;
using Orchard.Users.Models;

namespace Teeyoot.FeaturedCampaigns.Controllers
{
    [Admin]
    public class AdminFeaturedCampaignsController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly ICampaignService _campaignService;
        private readonly IOrderService _orderService;
        private IOrchardServices Services { get; set; }
        private readonly ITeeyootMessagingService _teeyootMessagingService;
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        private readonly IExportExcelService _exportToExel;
        private readonly IRepository<UserPart> _users;
        private dynamic Shape { get; set; }


        public Localizer T { get; set; }

        public ILogger Logger { get; set; }
        private readonly IWorkContextAccessor _workContextAccessor;

        public AdminFeaturedCampaignsController(ISiteService siteService, 
                                                IShapeFactory shapeFactory, 
                                                IOrchardServices services, 
                                                ICampaignService campaignService,
                                                IOrderService orderService,
                                                ITeeyootMessagingService teeyootMessagingService,
                                                IRepository<CurrencyRecord> currencyRepository,
                                                IWorkContextAccessor workContextAccessor,
                                                IExportExcelService ExportToExcel,
                                                IRepository<UserPart> Users
            )
        {
            _siteService = siteService;
            _campaignService = campaignService;
            _orderService = orderService;
            _teeyootMessagingService = teeyootMessagingService;
            _currencyRepository = currencyRepository;
            Shape = shapeFactory;
            Services = services;
            Logger = NullLogger.Instance;

            _workContextAccessor = workContextAccessor;
            _exportToExel = ExportToExcel;
            _users = Users;
        }

        // GET: Admin
        public ActionResult Index2(PagerParameters pagerParameters, int? filterCurrencyId = null)
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            
            var skip = pager.Page > 0 ? (pager.Page - 1) * pager.PageSize : 0;
            var take = pager.PageSize == 0 ? int.MaxValue : pager.PageSize;

            var campaigns = _campaignService.GetAllCampaigns().
                Where(c => (null == filterCurrencyId) || (c.CurrencyRecord.Id == filterCurrencyId));
            var yesterday = DateTime.UtcNow.AddDays(-1);
            var last24hoursOrders = _orderService.GetAllOrders().Where(o => o.IsActive && o.Created >= yesterday && o.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() && o.OrderStatusRecord.Name != OrderStatus.Pending.ToString());

            var featuredCampaigns = new FeaturedCampaignViewModel[] { };

            var total =_campaignService.GetAllCampaigns().Count();

            var totalNotApproved = _campaignService.GetAllCampaigns().Where(c => c.IsApproved == false && c.Rejected == false).Count();

            if (total > 0)
            {
                featuredCampaigns = campaigns
                    .Select(c => new CampaignViewModel
                    { 
                        Id = c.Id,
                        Goal = c.ProductCountGoal,
                        Sold = c.ProductCountSold,
                        IsFeatured = c.IsFeatured,
                        Title = c.Title,
                        IsActive  = c.IsActive,
                        Alias = c.Alias,
                        CreatedDate = c.StartDate.ToLocalTime(),
                        IsApproved = c.IsApproved,
                        Minimum = c.ProductMinimumGoal,
                        Rejected = c.Rejected,
                        Currency = c.CurrencyRecord,
                        FilterCurrencyId = filterCurrencyId,
                        Seller = c.Seller //(c.TeeyootUserId.HasValue) ? _users.Get(c.TeeyootUserId.Value) : null
                    })
                    .Select(c => new FeaturedCampaignViewModel
                    {
                        
                        Campaign = c,
                        Last24HoursSold =
                                    last24hoursOrders
                                        .SelectMany(o => o.Products)
                                        .Where(p => p.CampaignProductRecord.CampaignRecord_Id == c.Id)
                                        .Sum(p => (int?)p.Count) ?? 0
                    })
                     
                    .OrderBy(c => c.Campaign.Id)              
                    .ToArray();
                campaigns.OrderByDescending(c => c.Id);
            }

            //foreach (var campaign in campaigns)
            //{
            //    int soldCount = 0;

            //    var xsoldCount = _orderService.GetAllOrders().Where(aa => aa.Campaign.Id == campaign.Id);//.Sum(aa => aa.TotalSold);
            //    foreach (var xxx in xsoldCount)
            //    {
            //        if (xxx.OrderStatusRecord.Name == "Cancelled" || xxx.OrderStatusRecord.Name == "Pending" || xxx.OrderStatusRecord.Name == "Refunded") continue;
            //        soldCount += xxx.TotalSold;
            //    }
            //    campaign.ProductCountSold = soldCount;
            //}
            return View("Index", new AdminFeaturedCampaignsViewModel { Campaigns = featuredCampaigns,NotApprovedTotal= totalNotApproved,
                                        Currencies = _currencyRepository});
        }


        public ActionResult Index(PagerParameters pagerParameters, CampaingSearch search)
        {
            var campaigns = _campaignService.GetAllCampaigns().Where(aa => aa.WhenDeleted == null);
            

            campaigns = ApplySearch(search, campaigns);
            campaigns = ApplyOrder(search, campaigns);


            Pager pager = new Pager(Services.WorkContext.CurrentSite, pagerParameters);

            dynamic pagerShape = Services.New.Pager(pager).TotalItemCount(campaigns.Count());
            
            List<SelectListItem> ActiveItems = new List<SelectListItem>();
            
            ActiveItems.Add(new SelectListItem
            {
                Text = "Select One",
                Value = "-1",
                Selected = true
            });
            ActiveItems.Add(new SelectListItem
            {
                Text = "Yes",
                Value = "1"
            });
            ActiveItems.Add(new SelectListItem
            {
                Text = "No",
                Value = "0"
            });
            search.CurrecnyOption = new List<SelectListItem>();
            search.CurrecnyOption.Add(new SelectListItem()
            {
                Text = "Select One",
                Value = "-1"
            });


            search.CurrecnyOption.AddRange(_currencyRepository.Table.Select(aa => new SelectListItem
            {
                Text = aa.Code,
                Value = aa.Code
            }).ToList());



            var yesterDayOrders = _orderService.GetAllOrders().Where(aa => aa.Created >= DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0, 0))).ToArray();
            var items = campaigns.Skip(pager.GetStartIndex()).Take(pager.PageSize);

            foreach (var item in items)
            {
                item.SoldLast24Hours = yesterDayOrders.Where(order => order.Campaign.Id == item.Id &&
                            (order.OrderStatusRecord.Name == OrderStatus.Approved.ToString() ||
                             order.OrderStatusRecord.Name == OrderStatus.Delivered.ToString() ||
                             order.OrderStatusRecord.Name == OrderStatus.Shipped.ToString() ||
                            order.OrderStatusRecord.Name == OrderStatus.Printing.ToString())).Select(order => order.TotalSold).ToArray().Sum();

            }
            return View((object)new AdminCampaignViewModel
            {
                Items = items,
                Pager = pagerShape,
                PagerParameters = pagerParameters,
                Search = new CampaingSearch()
                {
                    Active = string.IsNullOrEmpty(search.Active) ? "-1" : search.Active,
                    Approved = string.IsNullOrEmpty(search.Approved) ? "-1" : search.Approved,
                    campaignName = search.campaignName,
                    Currency = string.IsNullOrEmpty(search.Currency) ? "-1" : search.Currency,
                    Featured = string.IsNullOrEmpty(search.Featured) ? "-1" : search.Featured,
                    ReadyToPrint = string.IsNullOrEmpty(search.ReadyToPrint) ? "-1" : search.ReadyToPrint,
                    Seller = search.Seller,
                    ActiveOption = ActiveItems,
                    ApprovedOption = ActiveItems,
                    FeaturedOption = ActiveItems,
                    ReadyToPrintOption = ActiveItems,
                    CurrecnyOption = search.CurrecnyOption,
                    sortBy = string.IsNullOrWhiteSpace(search.orderbyOrder) ? search.sortBy : "",
                    orderbyOrder = search.orderbyOrder, 
                    IsSuccessfull  = search.IsSuccessfull, EndDate = search.EndDate, StartDate = search.StartDate, TotalSold = 0
                }
            });
        }


        public JsonResult ChangePublic(int id, bool change)
        {
            _campaignService.PrivateCampaign(id, change);
            return Json(change, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<CampaignRecord> ApplyOrder(CampaingSearch search, IQueryable<CampaignRecord> campaigns)
        {

            switch (search.sortBy)
            {
                case "Campaign":

                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.Title);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.Title);
                    }

                    break;
                case "Create":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.StartDate);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.StartDate);
                    }
                    break;
                case "Active":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.IsActive);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.IsActive);
                    }
                    break;
                case "Soldlast24":
                    if (search.orderbyOrder == "asc")
                    {

                    }
                    else
                    {

                    }
                    break;
                case "Sold":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.ProductCountSold);

                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.ProductCountSold);
                    }
                    break;
                case "Approved":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.IsApproved);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.IsApproved);
                    }
                    break;
                case "Featured":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.IsFeatured);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.IsFeatured);
                    }
                    break;
                case "Ready":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.ProductCountSold > aa.ProductMinimumGoal);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.ProductCountSold > aa.ProductMinimumGoal);
                    }
                    break;
                case "Seller":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.Seller.UserName);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.Seller.UserName);
                    }
                    break;
                case "Currency":
                    if (search.orderbyOrder == "asc")
                    {
                        campaigns = campaigns.OrderBy(aa => aa.CurrencyRecord.Code);
                    }
                    else
                    {
                        campaigns = campaigns.OrderByDescending(aa => aa.CurrencyRecord.Code);
                    }
                    break;
                default:
                    break;
            }

            return campaigns;
            //throw new NotImplementedException();
        }

        private IQueryable<CampaignRecord> ApplySearch(CampaingSearch search, IQueryable<CampaignRecord> campaigns)
        {
            //throw new NotImplementedException();

            if (search.Active == "1") { campaigns = campaigns.Where(aa => aa.IsActive); }
            else if (search.Active == "0") { campaigns = campaigns.Where(aa => !aa.IsActive); }

            if (search.Approved == "1") { campaigns = campaigns.Where(aa => aa.IsApproved); }
            else if (search.Approved == "0") { campaigns = campaigns.Where(aa => !aa.IsApproved); }

            if (!string.IsNullOrWhiteSpace(search.campaignName)) { campaigns = campaigns.Where(aa => aa.Id.ToString().Contains(search.campaignName) || aa.Title.Contains(search.campaignName) || aa.Alias.Contains(search.campaignName)); }

            if (!string.IsNullOrWhiteSpace(search.Seller)) { campaigns = campaigns.Where(aa => aa.Seller != null && (aa.Seller.Id.ToString().Contains(search.Seller) || aa.Seller.UserName.Contains(search.Seller))); }

            if (search.Featured == "1") { campaigns = campaigns.Where(aa => aa.IsFeatured); }
            else if (search.Featured == "0") { campaigns = campaigns.Where(aa => !aa.IsFeatured); }

            if (!string.IsNullOrWhiteSpace(search.Currency) &&  search.Currency != "-1")
            {
                campaigns = campaigns.Where(aa => aa.CurrencyRecord.Code == search.Currency);
            }


            if (search.ReadyToPrint == "1") { campaigns = campaigns.Where(aa => aa.ProductCountSold > aa.ProductMinimumGoal); }
            else if (search.ReadyToPrint == "0") { campaigns = campaigns.Where(aa => aa.ProductCountSold < aa.ProductMinimumGoal); }

            if (search.StartDate.Year > 2014)
            {
                campaigns = campaigns.Where(aa => aa.StartDate >= search.StartDate);
            }
            if (search.EndDate.Year > 2014)
            {
                campaigns = campaigns.Where(aa => aa.EndDate <= search.EndDate);
            }

            if (search.IsSuccessfull == "on")
            {
                campaigns = campaigns.Where(aa => aa.ProductCountSold >= aa.ProductMinimumGoal);
            }
            return campaigns;
        }

        public ActionResult ChangeVisible(PagerParameters pagerParameters, int id, bool visible)
        {
            var featuredCampaigns = _campaignService.GetAllCampaigns().Where(c => c.IsFeatured);

            //if (featuredCampaigns.Count() >= 6 && visible)
            //{
            //    Services.Notifier.Add(Orchard.UI.Notify.NotifyType.Error, T("Can not update campaign, because already selected 6 companies!"));
            //}
            //else
            //{
            var campUpdate = _campaignService.GetCampaignById(id);
            campUpdate.IsFeatured = visible;

            try
            {
                _campaignService.UpdateCampaign(campUpdate);
                Services.Notifier.Add(Orchard.UI.Notify.NotifyType.Information, T("The campaign has successfully updated."));
            }
            catch (Exception e)
            {
                Logger.Error("Error when tring to update campaign ----------------------------> " + e.ToString());
                Services.Notifier.Add(Orchard.UI.Notify.NotifyType.Error, T("Can not update campaign. Try again later!"));
            }
            //}
            return RedirectToAction("Index", new { PagerParameters = pagerParameters });
        }

        public ActionResult DeleteCampaign(PagerParameters pagerParameters, int id)
        {
            if (_campaignService.DeleteCampaign(id))
            {
                Services.Notifier.Add(Orchard.UI.Notify.NotifyType.Information, T("The campaign was deleted successfully!"));
            }
            else
            {
                Services.Notifier.Add(Orchard.UI.Notify.NotifyType.Error, T("The company could not be removed. Try again!"));
            }

            return this.RedirectToAction("Index", new { pagerParameters = pagerParameters});
        }

        public ActionResult Approve(PagerParameters pagerParameters, int id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            campaign.IsApproved = true;
            campaign.Rejected = false;
            campaign.WhenApproved = DateTime.UtcNow;

            var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
            var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

            if (!campaign.IsArchived && campaign.BaseCampaignId != null)
            {
                _teeyootMessagingService.SendReLaunchApprovedCampaignMessageToSeller(pathToTemplates, pathToMedia, campaign.Id);
                _teeyootMessagingService.SendReLaunchApprovedCampaignMessageToBuyers(pathToTemplates, pathToMedia, campaign.Id);
            }
            else
            {               
                _teeyootMessagingService.SendLaunchCampaignMessage(pathToTemplates, pathToMedia, campaign.Id);
            }
            
            return RedirectToAction("Index", new { PagerParameters = pagerParameters });
        }



        public ActionResult Reject(PagerParameters pagerParameters, int id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            campaign.Rejected = true;
            campaign.IsApproved = false;
            var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
            var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
            _teeyootMessagingService.SendRejectedCampaignMessage(pathToTemplates, pathToMedia, campaign.Id);
            return RedirectToAction("Index", new { PagerParameters = pagerParameters });
        }

        public FileResult ExportToExcel(int id)
        {
            //string file = _exportToExel.ExportCampaign(id);
            string fileName = _exportToExel.ExportCampaign(id);
            return File(fileName, "Excel", fileName.Replace("\\", "/").Substring(fileName.Replace("\\", "/").LastIndexOf("/") + 1));
        }


        public FileResult ExportToExcel2(int id)
        {
            string fileName = _exportToExel.ExportCampaign2(id);
            return File(fileName, "Excel", fileName.Replace("\\", "/").Substring(fileName.Replace("\\", "/").LastIndexOf("/") + 1));
        }

        public JsonResult CalculateProfit(int id)
        {
            _campaignService.CalculateCampaignProfit(id);
            var campaign = _campaignService.GetCampaignById(id);


            return Json(new
            {
                ClaimableProfit = campaign.ClaimableProfit,
                UnclaimableProfit = campaign.UnclaimableProfit,
                TotalProfit = campaign.CampaignProfit,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}