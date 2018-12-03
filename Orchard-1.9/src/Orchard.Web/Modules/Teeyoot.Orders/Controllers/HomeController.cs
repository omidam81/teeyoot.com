using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Users.Models;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Orders.Controllers
{
    [Admin]
    public class HomeController : Controller
    {
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        private readonly IRepository<OrderStatusRecord> _orderStatusRepository;
        private readonly IOrderService _orderService;
        private readonly ICampaignService _campaignService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;
        private readonly IPayoutService _payoutService;
        private readonly INotifier _notifierService;
        private readonly ITeeyootMessagingService _teeyootMessagingService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IExportExcelService _exportexcelservice;
        private readonly IRepository<ProductSizeRecord> _sizes;
        private readonly IRepository<ProductColorRecord> _colors;
        private readonly IPriceConversionService _priceConversationService;
        public ILogger Logger { get; set; }
        private dynamic Shape { get; set; }
        // GET: Home

        public HomeController(
            IRepository<CurrencyRecord> currencyRepository,
            IRepository<OrderStatusRecord> orderStatusRepository,
            IOrderService orderService,
            ICampaignService campaignService,
            IShapeFactory shapeFactory,
            IContentManager contentManager,
            ISiteService siteService,
            IPayoutService payoutService,
            INotifier notifierService,
            ITeeyootMessagingService teeyootMessagingService,
            IWorkContextAccessor workContextAccessor,
            IExportExcelService exportexcelservice,
            IRepository<ProductColorRecord> Colors,
            IRepository<ProductSizeRecord> Sizes,
            IPriceConversionService priceConversationService
            )
        {
            _currencyRepository = currencyRepository;
            _orderStatusRepository = orderStatusRepository;
            _orderService = orderService;
            _campaignService = campaignService;
            _contentManager = contentManager;
            _siteService = siteService;
            _payoutService = payoutService;
            _notifierService = notifierService;
            _teeyootMessagingService = teeyootMessagingService;
            Shape = shapeFactory;
            _exportexcelservice = exportexcelservice;
            T = NullLocalizer.Instance;
            _workContextAccessor = workContextAccessor;
            _sizes = Sizes;
            _colors = Colors;
            _priceConversationService = priceConversationService;
        }

        public Localizer T { get; set; }

        public ActionResult Index2(int? page_id, int? page_size, int? filterCurrencyId, int? status, int? paymentStatus, PagerParameters pagerParameters)
        {
            var orders = _orderService.GetAllOrders()
               .Where(item => item.IsActive && item.Email != null && item.Products.Count > 0);

            if (status.HasValue)
            {
                orders = orders.Where(aa => aa.OrderStatusRecord.Id == status.Value);
            }

            if (filterCurrencyId.HasValue)
            {
                var filterCurrency = _currencyRepository.Get(filterCurrencyId.Value);
                orders = orders.Where(o => o.CurrencyRecord == filterCurrency);
            }

            if (paymentStatus.HasValue)
            {
                if (paymentStatus.Value != -1)
                {
                    orders = orders.Where(aa => ((paymentStatus.Value == 0) ? aa.ProfitPaid : !aa.ProfitPaid));
                }
            }
            var orderEntities = new AdminOrderViewModel();
            foreach (var item in orders)
            {

                CampaignProductRecord campRec = null;
                campRec = item.Products.First().CampaignProductRecord;

                //if (item.Products == null || item.Products.Count() == 0 || ()== null) continue;
                var campaignId = campRec.CampaignRecord_Id;
                if (campaignId == null)
                    continue;

                var campaign = _campaignService.GetCampaignById(campaignId);

                try
                {

                    var seller = _contentManager.Query<UserPart, UserPartRecord>()
                        .Where(user => user.Id == campaign.TeeyootUserId)
                        .List()
                        .First();
                    double orderProfit = 0;

                    foreach (var product in item.Products)
                    {
                        var prof = product.CampaignProductRecord.Price - product.CampaignProductRecord.BaseCost;
                        foreach (var size in product.CampaignProductRecord.ProductRecord.SizesAvailable)
                        {
                            if (size.Id == product.ProductSizeRecord.Id)
                                prof = prof - size.SizeCost;
                        }
                        orderProfit = orderProfit + (prof * product.Count);
                        orderProfit = Math.Round(orderProfit, 2);
                    }


                    //if (string.IsNullOrWhiteSpace(searchString) || campaign.Title.ToLower().Contains(searchString.ToLower()))
                    //{
                    orderEntities.Orders.Add(new AdminOrder
                    {
                        PublicId = item.OrderPublicId,
                        Products = item.Products,
                        Status = item.OrderStatusRecord.Name,
                        EmailBuyer = item.Email,
                        CampaignId = campaign.Id,
                        CampaignName = campaign.Title,
                        CampaignAlias = campaign.Alias,
                        Id = item.Id,
                        Profit = orderProfit,
                        SellerId = seller != null ? seller.Id : 0,
                        Payout = item.ProfitPaid,
                        CreateDate = item.Created,
                        UserNameSeller = seller != null ? seller.UserName : "",
                        Currency = item.CurrencyRecord.ShortName
                    });
                    //}
                }
                catch (Exception ex)
                {

                    Logger.Error(ex, campaign.TeeyootUserId + "  ERROOORRRRRRRRRRRR ");
                    throw;
                }
            }
            //var qwe = new List<SelectListItem>();

            //var entriesProjection = orderEntities.Orders.Select(e =>
            //{
            //    return Shape.FaqEntry(
            //        PublicId: e.PublicId,
            //        Products: e.Products,
            //        Status: e.Status,
            //        EmailBuyer: e.EmailBuyer,
            //        Id: e.Id,
            //        Profit: e.Profit,
            //        UserNameSeller: e.UserNameSeller,
            //        Payout: e.Payout,
            //        CampaignId: e.CampaignId,
            //        CampaignName: e.CampaignName,
            //        CampaignAlias: e.CampaignAlias,
            //        SellerId: e.SellerId,
            //        CreateDate: e.CreateDate.ToString("dd/MM/yyyy")
            //        );
            //});
            //var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters.Page, pagerParameters.PageSize);

            //var pagerShape = Shape.Pager(pager).TotalItemCount(entriesProjection.Count());
            var orderStatuses = _orderStatusRepository.Table
                .Select(s => new OrderStatusItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToList();

            var currencies = _currencyRepository.Table
                .Select(c => new CurrencyItemViewModel
                {
                    Id = c.Id,
                    Name = c.ShortName
                });

            return View("Index2", new AdminOrderViewModel
            {
                DynamicOrders = orderEntities.Orders.ToArray(),
                OrderStatuses = orderStatuses,
                SelectedCurrencyFilterId = filterCurrencyId,
                Currencies = currencies
            });
        }



        public ActionResult Index(int? pageID, int? pageSize, int? filterCurrencyId, int? status, int? paymentStatus, string orderby,
            string search_order_id,
            string search_order_number,
            string search_order_buyer,
            string search_order_campaign,
            string search_order_seller, string orderbyOrder,
            string search_payment_type, 
            PagerParameters pagerParameters)
        {
            var orders = _orderService.GetAllOrders()
               .Where(item => item.IsActive && item.Email != null && item.Products.Count > 0).OrderByDescending(aa => aa.Created).AsQueryable();
            ViewBag.status = status;
            ViewBag.filterCurrencyId = filterCurrencyId;
            ViewBag.paymentStatus = paymentStatus;
            ViewBag.pageID = pageID;
            var xOrderDesc = !string.IsNullOrWhiteSpace(orderbyOrder);
            if (!xOrderDesc) ViewBag.orderby = orderby;

            ViewBag.pageSize = pageSize;

            ViewBag.search_order_id = search_order_id;
            ViewBag.search_order_number = search_order_number;
            ViewBag.search_order_buyer = search_order_buyer;
            ViewBag.search_order_campaign = search_order_campaign;
            ViewBag.search_order_seller = search_order_seller;
            ViewBag.search_payment_type = search_payment_type;
            ViewBag.PriceConversionService = _priceConversationService;

            if (status.HasValue && status != -1)
            {
                orders = orders.Where(aa => aa.OrderStatusRecord.Id == status.Value);
            }

            if (filterCurrencyId.HasValue && filterCurrencyId != -1)
            {
                var filterCurrency = _currencyRepository.Get(filterCurrencyId.Value);
                orders = orders.Where(o => o.CurrencyRecord == filterCurrency);
            }

            if (paymentStatus.HasValue && paymentStatus != -1)
            {
                if (paymentStatus.Value != -1)
                {
                    if (paymentStatus == 0)
                    {
                        orders = orders.Where(aa => aa.ProfitPaid);
                    }
                    else
                    {
                        orders = orders.Where(aa => !aa.ProfitPaid);
                    }
                }
            }


            if (!pageID.HasValue) pageID = 0;
            if (!pageSize.HasValue) pageSize = 30;
            orders = applySeachTerms(orders, search_order_buyer, search_order_campaign, search_order_id, search_order_number, search_order_seller, search_payment_type);

            ViewBag.Cuurent = pageID;
            ViewBag.DataCount = orders.Count();
            ViewBag.PageSize = pageSize;
            ViewBag.CampaignService = _campaignService;
            ViewBag.ContentManager = _contentManager;
            ViewBag.Currencies = _currencyRepository.Table

                .Select(c => new CurrencyItemViewModel
                {
                    Id = c.Id,
                    Name = c.ShortName
                });
            ViewBag.OrderStatuses = _orderStatusRepository.Table
                .Select(s => new OrderStatusItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToList();




            var orderviewmodel = orders.AsEnumerable().Select(order =>
            {
                //var soldCount = 0;
                //if (order.Products != null && order.Products.Count != 0)
                //    foreach (var item in order.Products)
                //        soldCount += item.Count;
                return new OrderViewModel()
                {
                    SoldCount = order.TotalSold,
                    Seller = order.Seller,
                    Order = order,
                    Campaign = order.Campaign,
                };
            });
            if (!string.IsNullOrWhiteSpace(orderby))
            {
                switch (orderby)
                {
                    case "date":
                        if (xOrderDesc)
                        {
                            orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.Created).AsEnumerable();

                        }
                        else
                        {
                            orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.Created).AsEnumerable();
                        }
                        break;
                    case "Buyer":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.Email).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.Email).AsEnumerable(); }

                        break;
                    case "Campaign ID":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Campaign.Id).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Campaign.Id).AsEnumerable(); }

                        break;
                    case "Campaign":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Campaign.Title).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Campaign.Title).AsEnumerable(); }

                        break;
                    case "Seller":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Seller.Id).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Seller.Id).AsEnumerable(); }
                        break;
                    case "TotalAmount":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.TotalPrice).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.TotalPrice).AsEnumerable(); }

                        break;
                    case "Status":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.OrderStatusRecord.Id).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.OrderStatusRecord.Id).AsEnumerable(); }

                        break;
                    case "Payment Status":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.ProfitPaid).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.ProfitPaid).AsEnumerable(); }

                        break;
                    case "Currency":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.CurrencyRecord.ShortName).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.CurrencyRecord.ShortName).AsEnumerable(); }

                        break;
                    case "orderid":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.Id).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.Id).AsEnumerable(); }
                        break;
                    case "paymentType":
                        if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderBy(aa => aa.Order.PaymentMethod).AsEnumerable(); }
                        else { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.PaymentMethod).AsEnumerable(); }
                        break;
                    case "SoldCount":
                         if (xOrderDesc) { orderviewmodel = orderviewmodel.OrderBy(aa => aa.SoldCount).AsEnumerable(); }
                         else { orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.SoldCount).AsEnumerable(); }
                         break;
                }
            }
            else
            {
                orderviewmodel = orderviewmodel.OrderByDescending(aa => aa.Order.Created).AsEnumerable();
            }
            return View("Index", orderviewmodel.Skip(pageID.Value * pageSize.Value).Take(pageSize.Value));
        }

        [NonAction]
        private IQueryable<OrderRecord> applySeachTerms(IQueryable<OrderRecord> orders, string search_order_buyer, string search_order_campaign, string search_order_id, string search_order_number, string search_order_seller, string search_payment_type)
        {
            //throw new NotImplementedException();

            if (!string.IsNullOrWhiteSpace(search_order_buyer))
            {
                orders = orders.Where(aa => aa.Email != null && aa.Email.Contains(search_order_buyer));
            }

            if (!string.IsNullOrWhiteSpace(search_order_campaign))
            {
                orders = orders.Where(aa => aa.Campaign != null && (
                    aa.Campaign.Id.ToString().Contains(search_order_campaign) ||
                    aa.Campaign.Title.Contains(search_order_campaign) ||
                    aa.Campaign.URL.Contains(search_order_campaign) ||
                    aa.Campaign.Alias.Contains(search_order_campaign)));
            }

            if (!string.IsNullOrWhiteSpace(search_order_id))
            {
                orders = orders.Where(aa => aa.Id.ToString().Contains(search_order_id));
            }

            if (!string.IsNullOrWhiteSpace(search_order_number))
            {
                orders = orders.Where(aa => aa.OrderPublicId.Contains(search_order_number));
            }

            if (!string.IsNullOrWhiteSpace(search_order_seller))
            {
                orders =
                    orders.Where(aa => ((aa.Seller != null) &&
                        (
                            aa.Seller.Email.Contains(search_order_seller) ||
                            aa.Seller.Id.ToString().Contains(search_order_seller)
                        )));
            }

            if (!string.IsNullOrEmpty(search_payment_type))
            {
                orders = orders.Where(aa => aa.PaymentMethod == search_payment_type);
            }
            return orders;

        }

        [Admin]
        public JsonResult bulkAction(string paid, string status, string ids, string public_ids, string sellerids, string profits)
        {
            if (

                string.IsNullOrWhiteSpace(paid) || string.IsNullOrWhiteSpace(status) || string.IsNullOrWhiteSpace(ids)
                || string.IsNullOrWhiteSpace(public_ids)
                || string.IsNullOrWhiteSpace(sellerids)
                || string.IsNullOrWhiteSpace(profits)

                ) throw new Exception("Invalid paramater");

            var indecies = ids.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Select(aa => { return int.Parse(aa); });
            var pids = public_ids.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);//.Select(aa => { aa); });
            var sellers = sellerids.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Select(aa => { return int.Parse(aa); });
            var pros = profits.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Select(aa => { return double.Parse(aa.Replace(".", ",")); });


            if (paid != "-1")
            {
                if (paid == "Paid")
                {
                    for (int i = 0; i < indecies.Count(); i++)
                    {
                        EditStatusPayout(pids[i], pros.ToArray()[i], sellers.ToArray()[i]);


                    }
                }
                else if (paid != "Don't Change")
                {
                    for (int i = 0; i < indecies.Count(); i++)
                    {
                        DeletePayout(pids[i]);
                    }
                }

            }

            if (status != "-1" && status != "Don't Change")
            {
                foreach (var id in indecies)
                {
                    ApplyStatus(id, status, true);
                }
                if (indecies != null && indecies.Count() > 0)
                    sendEmails(indecies.ToArray());
            }
            _notifierService.Information(T("Successfully updated order status "));
            return Json("done", JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult GetOrderInfo(string orderID)
        {
            var order = _orderService.GetOrderByPublicId(orderID.Trim().ToString());
            var campaign = _campaignService.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id);
            ViewBag.Campaign = campaign;
            ViewBag.PriceConversionService = _priceConversationService;
            return PartialView(order);
        }
        public JsonResult GetOrderInfirmation(string publicId)
        {
            var order = _orderService.GetOrderByPublicId(publicId.Trim(' '));

            var products = order.Products.Select(o => new
            {
                Name = o.CampaignProductRecord.ProductRecord.Name,
                Count = o.Count,
                Currency = o.CampaignProductRecord.CurrencyRecord.Code,
                Price =
                    o.CampaignProductRecord.Price +
                    Pricing(o.CampaignProductRecord.ProductRecord.SizesAvailable, o.ProductSizeRecord.Id),
                Size = o.ProductSizeRecord.SizeCodeRecord.Name,
                Color =
                    o.ProductColorRecord == null
                        ? o.CampaignProductRecord.ProductColorRecord.Name
                        : o.ProductColorRecord.Name
            });

            var totalPrice = order.TotalPriceWithPromo > 0.0 ? order.TotalPriceWithPromo : order.TotalPrice;
            totalPrice += order.Delivery;

            var result = new { products, totalPrice };
            ViewBag.PriceConversionService = _priceConversationService;

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private float Pricing(IList<LinkProductSizeRecord> SizesAvailable, int productSizeRecord)
        {
            float sizeC = 0;
            foreach (var size in SizesAvailable)
            {
                if (size.ProductSizeRecord.Id == productSizeRecord)
                    sizeC = size.SizeCost;
            }
            return sizeC;
        }

        public JsonResult GetBuyerInfirmation(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            return Json(new
            {
                firstName = order.FirstName,
                lastName = order.LastName,
                streetAdress = order.StreetAddress,
                city = order.City,
                country = order.Country,
                phoneNumber = order.PhoneNumber,
                state = order.State,
                postalCode = order.PostalCode,
                id = order.Id,
                email = order.Email
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdaeBuyerInfirmation(string hiddenId, string firstName,
                string lastName,
                string streetAdress,
                string city,
                string country,
                string phoneNumber,
                string state,
                string postalCode,
                string email
            )
        {
            var order = _orderService.GetOrderById(int.Parse(hiddenId));

            order.FirstName = firstName;
            order.LastName = lastName;
            order.StreetAddress = streetAdress;
            order.City = city;
            order.Country = country;
            order.PhoneNumber = phoneNumber;
            order.State = state;
            order.PostalCode = postalCode;
            order.Email = email;
            _orderService.UpdateOrder(order);


            return Json("done", JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateOrder(FormCollection formsValue)
        {
            var orderID = int.Parse(formsValue["orderid"]);
            var productCount = int.Parse(formsValue["productcount"]);

            var _order = _orderService.GetOrderById(orderID);



            for (int i = 0; i < productCount; i++)
            {
                var productID = int.Parse(formsValue["product_" + i]);
                var size = int.Parse(formsValue["size_" + i]);
                //var count = int.Parse(formsValue["count_" + i]);
                var color = int.Parse(formsValue["color_" + i]);


                var product = _order.Products.First(aa => aa.Id == productID);
                //product.Count = count;
                product.ProductSizeRecord = _sizes.Get(size);
                product.ProductColorRecord = _colors.Get(color);
                ///product.ProductSizeRecord.

            }

            _orderService.UpdateOrder(_order);

            return Json("done", JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditStatusPayout(string publicId, double profit, int sellerId)
        {
            var order = _orderService.GetOrderByPublicId(publicId.Trim(' '));


            double orderProfit = 0;

            foreach (var product in order.Products)
            {
                var prof = product.CampaignProductRecord.Price - product.CampaignProductRecord.BaseCost;
                foreach (var size in product.CampaignProductRecord.ProductRecord.SizesAvailable)
                {
                    if (size.Id == product.ProductSizeRecord.Id)
                        prof = prof - size.SizeCost;
                }
                orderProfit = orderProfit + (prof * product.Count);
                orderProfit = Math.Round(orderProfit, 2);
            }


            order.Paid = DateTime.Now.ToUniversalTime();
            var campaignId = order.Products.First().CampaignProductRecord.CampaignRecord_Id;
            var campaign = _campaignService.GetCampaignById(campaignId);
            order.ProfitPaid = true;
            _orderService.UpdateOrder(order);
            _payoutService.AddPayout(new PayoutRecord
            {
                Date = DateTime.Now.ToUniversalTime(),
                Currency_Id = order.CurrencyRecord.Id,
                Amount = orderProfit,
                IsPlus = true,
                Status = "Completed",
                UserId = sellerId,
                Event = publicId.Trim(' '),
                IsOrder = true
            });
            return Json("done!", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeletePayout(string publicId)
        {
            if (true || _payoutService.DeletePayoutByOrderPublicId(publicId.Trim(' ')))
            {
                var order = _orderService.GetOrderByPublicId(publicId.Trim(' '));
                order.Paid = null;
                order.ProfitPaid = false;
                _orderService.UpdateOrder(order);
            }
            return Json("done!", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ApplyStatus(int orderId, string orderStatus, bool bulkAction = false)
        {
            var order = _orderService.GetOrderById(orderId);
            var campId = order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id;

            if (order.OrderStatusRecord != null && (order.OrderStatusRecord.Name == OrderStatus.Cancelled.ToString() || order.OrderStatusRecord.Name == OrderStatus.Pending.ToString() || order.OrderStatusRecord.Name == OrderStatus.Refunded.ToString()) &&
                (orderStatus != OrderStatus.Cancelled.ToString() || orderStatus != OrderStatus.Pending.ToString() || orderStatus != OrderStatus.Refunded.ToString()))
            {
                var sum = order.TotalSold;

                var campaign =
                    _campaignService.GetCampaignById(
                        order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id);
                campaign.ProductCountSold += sum;
                _campaignService.UpdateCampaign(campaign);
            }

            if (order.OrderStatusRecord != null && (order.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() || order.OrderStatusRecord.Name != OrderStatus.Pending.ToString() || order.OrderStatusRecord.Name != OrderStatus.Refunded.ToString()) &&
                (orderStatus == OrderStatus.Cancelled.ToString() || orderStatus == OrderStatus.Pending.ToString() || orderStatus == OrderStatus.Refunded.ToString()))
            {
                var sum = order.TotalSold; //.Products.Select(o => o.Count).Sum();
                var campaign =
                    _campaignService.GetCampaignById(
                        order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id);
                campaign.ProductCountSold -= sum;
                _campaignService.UpdateCampaign(campaign);
            }
            int i = 0;
            OrderStatus newStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), orderStatus);
            if (orderStatus == "Shipped")
            {
                order.WhenSentOut = DateTime.UtcNow;
            }

            order.OrderStatusRecord = (_orderStatusRepository.Get(int.Parse(newStatus.ToString("d"))));
            _orderService.UpdateOrder(order);

           // _orderService.UpdateOrder(order, newStatus);

            if (bulkAction)
            {
                return Json(T("Successfully updated order status "), JsonRequestBehavior.AllowGet);
            }
            var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
            var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
            //ToDo: delete order if new status is "Cancelled"

            var ordersForCampaign = _orderService.GetProductsOrderedOfCampaign(campId);
            foreach (var item in ordersForCampaign)
            {

                if (item.OrderRecord.OrderStatusRecord != null &&
                    item.OrderRecord.OrderStatusRecord.Name != OrderStatus.Delivered.ToString() &&
                    item.OrderRecord.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
                    item.OrderRecord.OrderStatusRecord.Name != OrderStatus.Refunded.ToString())
                {
                    i++;
                }
            }


            if (i == 0)
            {
                if (!order.Campaign.IsActive && order.Campaign.ProductCountGoal <= order.Campaign.ProductCountSold)
                    _teeyootMessagingService.SendAllOrderDeliveredMessageToSeller((order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id));
            }
            _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, orderId, orderStatus);
           // _notifierService.Information(T("Successfully updated order status "));
            return Json(T("Successfully updated order status "), JsonRequestBehavior.AllowGet);
        }

        public void sendEmails(int[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                for (int j = 0; j < ids.Length; j++)
                {
                    var order = _orderService.GetOrderById(ids[j]);
                    
                    var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                    var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');
                    //ToDo: delete order if new status is "Cancelled"
                    int i = 0;
                    var ordersForCampaign = _orderService.GetProductsOrderedOfCampaign(order.Campaign.Id);
                    foreach (var item in ordersForCampaign)
                    {

                        if (item.OrderRecord.OrderStatusRecord != null &&
                            item.OrderRecord.OrderStatusRecord.Name != OrderStatus.Delivered.ToString() &&
                            item.OrderRecord.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
                            item.OrderRecord.OrderStatusRecord.Name != OrderStatus.Refunded.ToString())
                        {
                            i++;
                        }
                    }


                    if (i == 0)
                    {
                        if (!order.Campaign.IsActive && order.Campaign.ProductCountGoal <= order.Campaign.ProductCountSold)
                            _teeyootMessagingService.SendAllOrderDeliveredMessageToSeller(
                                (order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id));
                    }
                    _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, ids[j], order.OrderStatusRecord.Name);
                }
            }
        }
        
        public ActionResult Orders()
        {
            return View("Orders");
        }


        public JsonResult JSON_Orders(int pageSize = 30, int pageID = 0)
        {
            //Order ID	Order Number	order date	Buyer	Campaign	Seller	Profit	Status	Payment Status	Currency
            var data = _orderService.GetAllOrders().Where(aa => aa.IsActive == true && aa.Email != null)/*.Skip(pageSize * pageID).Take(pageSize)*/.Select(
                    aa =>
                    new
                    {
                        OrderID = aa.Id,
                        OrderNumber = aa.OrderPublicId,
                        OrderDate = aa.Created.ToShortDateString(),
                        Buyer = aa.Email,
                        CampaignTitle = aa.Campaign.Title,
                        CampaginId = aa.Campaign.Id,
                        SellerId = aa.Seller.Id,
                        SellerName = aa.Seller.UserName,
                        Profit = 100,
                        Status = aa.OrderStatusRecord.Name.ToString(),
                        PaymentStatus = aa.Paid,
                        Currency = aa.CurrencyRecord.Code
                    }
                );

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        //Expot to excel

        public FileResult ExportToExcel(int orderID)
        {

            var _order = _orderService.GetOrderById(orderID);
            var fileName = _exportexcelservice.ExportOrderToInvoiceExcelFile(orderID);
            return File(fileName, "Excel", fileName.Replace("\\", "/").Substring(fileName.Replace("\\", "/").LastIndexOf("/") + 1));
        }

        public ActionResult ExpotToExcelType2(

                int? FilterCurrencyId,
             int? status,

            int? campaignID,
            string fromDate,
            string toDate,
            int? paymentStatus
            )
        {


            if (!FilterCurrencyId.HasValue &&
               !status.HasValue &&
               !campaignID.HasValue &&
               string.IsNullOrWhiteSpace(fromDate) &&
               string.IsNullOrWhiteSpace(toDate) &&
               !paymentStatus.HasValue) return RedirectToAction("Index");
            var orders = _orderService.GetAllOrders()
               .Where(item => item.IsActive && item.Email != null && item.Products.Count > 0);


            if (!string.IsNullOrWhiteSpace(fromDate))
            {
                DateTime d = DateTime.Parse(fromDate);
                orders = orders.Where(aa => aa.Created >= d);
            }

            if (!string.IsNullOrWhiteSpace(toDate))
            {
                DateTime d = DateTime.Parse(toDate);
                orders = orders.Where(aa => aa.Created <= d);
            }

            if (status.HasValue)
            {
                orders = orders.Where(aa => aa.OrderStatusRecord.Id == status);
            }

            if (FilterCurrencyId.HasValue)
            {
                var filterCurrency = _currencyRepository.Get(FilterCurrencyId.Value);
                orders = orders.Where(o => o.CurrencyRecord.Id == FilterCurrencyId);
            }

            if (paymentStatus.HasValue)
            {
                if (paymentStatus != -1)
                {
                    orders = orders.Where(aa => (paymentStatus == 0) ? aa.ProfitPaid : !aa.ProfitPaid);
                }
            }

            if (campaignID.HasValue)
            {
                orders = orders.Where(aa => aa.Campaign != null && aa.Campaign.Id == campaignID);
            }

            if (orders.Count() <= 0) return RedirectToAction("Index");
            var filename = _exportexcelservice.ExpotZip(orders);
            return File(filename, "application/zip", filename);

        }
    }
}