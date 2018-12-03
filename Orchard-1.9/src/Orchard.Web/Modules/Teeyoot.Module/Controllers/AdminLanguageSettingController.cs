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
using Teeyoot.Module.Services.Interfaces;
using Orchard.Users.Models;
using Teeyoot.Module.Common.Enums;

namespace Teeyoot.Module.Controllers
{
     [Admin]
    public class AdminLanguageSettingController : Controller
    {
        private readonly ISiteService _siteService;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CountryRecord> _countryRepository;
        private readonly IRepository<CultureRecord> _cultureRepository;
        private readonly IOrderService OrderService;
        private readonly IRepository<UserPartRecord> Users;
        private readonly ICampaignService CampaignServce;

        private dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ILogger Logger { get; set; }

        public AdminLanguageSettingController(
            ISiteService siteService,
            IOrchardServices orchardServices,
            IShapeFactory shapeFactory,
            IRepository<CountryRecord> countryRepository,
            IRepository<CultureRecord> cultureRepository, 
            IOrderService orderService,
            ICampaignService campaignServce,
            IRepository<UserPartRecord> users
            )
        {
            _siteService = siteService;
            _orchardServices = orchardServices;
            Shape = shapeFactory;
            _countryRepository = countryRepository;
            _cultureRepository = cultureRepository;
            OrderService = orderService;
            Users = users;
            Logger = NullLogger.Instance;
            CampaignServce = campaignServce;
        }


        // GET: AdminExchnageRate
        public ActionResult Index()
        {
            return View();
        
        }

        public ActionResult FixDatabaseProblems()
        {

            //var affectedRow = 0;
            //foreach (var order in OrderService.GetAllOrders())
            //{
            //    if (order.Email == null) { order.IsActive = false; affectedRow++; OrderService.UpdateOrder(order); continue;}
            //    if (order.Products == null || order.Products.Count() == 0 || order.Products.First() == null || order.Products.First().CampaignProductRecord == null) { order.IsActive = false; affectedRow++;  OrderService.UpdateOrder(order); continue;}
            //    if (CampaignServce.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id) == null) { order.IsActive = false; affectedRow++; OrderService.UpdateOrder(order); continue; }
            //    if (!CampaignServce.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id).TeeyootUserId.HasValue) { order.IsActive = false; affectedRow++; OrderService.UpdateOrder(order); continue; }
            //    var seller = Users.Get(CampaignServce.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id).TeeyootUserId.Value);
            //    order.Seller = seller;
            //    order.Campaign = CampaignServce.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id);
            //    OrderService.UpdateOrder(order);
            //}
            foreach (var campaign in CampaignServce.GetAllCampaigns())
            {
                //CampaignServce.CalculateCampaignProfit(campaign.Id);
                if (campaign.IsActive)
                {
                    var totalSold = 0;
                    var orders = OrderService.GetAllOrders().Where(aa => aa.Campaign.Id == campaign.Id);
                    foreach (var item in orders)
                    {

                        if (item.OrderStatusRecord.Name == OrderStatus.Approved.ToString() ||
                            item.OrderStatusRecord.Name == OrderStatus.Delivered.ToString() ||
                            item.OrderStatusRecord.Name == OrderStatus.Printing.ToString() ||
                            item.OrderStatusRecord.Name == OrderStatus.Shipped.ToString())
                        {
                            totalSold += item.TotalSold;
                        }
                    }
                    campaign.ProductCountSold = totalSold;

                    CampaignServce.UpdateCampaign(campaign);
                }
            }
            return Redirect("Index");

        }
    }
}