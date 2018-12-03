using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;
using Orchard.Users.Models;

namespace Teeyoot.Module.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository<OrderRecord> _orderRepository;
        private readonly IRepository<LinkOrderCampaignProductRecord> _ocpRepository;
        private readonly IRepository<ProductSizeRecord> _sizeRepository;
        private readonly IRepository<   OrderStatusRecord> _orderStatusRepository;
        private readonly IRepository<OrderHistoryRecord> _orderHistoryRepository;
        private readonly ICampaignService _campaignService;
        private readonly IRepository<ProductColorRecord> _colorRepository;
        private readonly IRepository<CampaignRecord> _campaignRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<UserPartRecord> _userPartRepository;


        private readonly IRepository<CurrencyRecord> _currencyRrpository;

        public OrderService(
            IRepository<OrderRecord> orderRepository,
            IRepository<LinkOrderCampaignProductRecord> ocpRepository,
            ICampaignService campaignService,
            IRepository<ProductSizeRecord> sizeRepository,
            IRepository<OrderStatusRecord> orderStatusRepository,
            IRepository<OrderHistoryRecord> orderHistoryRepository,
            IRepository<ProductColorRecord> colorRepository,
            IRepository<CampaignRecord> campaignRepository,
            IOrchardServices orchardServices,
            IRepository<UserPartRecord> UserPartRepository,
            IRepository<CurrencyRecord> currencyRrpository,
            ICampaignService CampaignService
            
            )
        {
            _orderRepository = orderRepository;
            _ocpRepository = ocpRepository;
            _campaignService = campaignService;
            _sizeRepository = sizeRepository;
            _orderStatusRepository = orderStatusRepository;
            _orderHistoryRepository = orderHistoryRepository;
            _colorRepository = colorRepository;
            _campaignRepository = campaignRepository;
            _orchardServices = orchardServices;
            _userPartRepository = UserPartRepository;
            _currencyRrpository = currencyRrpository;
        }

        public IQueryable<OrderRecord> GetAllOrders()
        {
            return _orderRepository.Table;
        }

        public OrderRecord GetOrderById(int id)
        {
            return _orderRepository.Table.FirstOrDefault(r => r.Id == id);
        }

        public OrderRecord GetOrderByPublicId(string id)
        {
            return _orderRepository.Table.FirstOrDefault(r => r.OrderPublicId == id);
        }

        public IQueryable<OrderRecord> GetActiveOrdersByEmailForLastTwoMoth(string email)
        {
            return
                _orderRepository.Table.Where(
                    r => r.Email == email && r.IsActive && r.Created >= DateTime.Now.AddDays(-60));

        }

        public OrderRecord GetActiveOrderById(int id)
        {
            return _orderRepository.Table.FirstOrDefault(r => r.Id == id && r.IsActive);
        }

        public OrderRecord GetActiveOrderByPublicId(string id)
        {
            return _orderRepository.Table.FirstOrDefault(r => r.OrderPublicId == id && r.IsActive);
        }

        public void UpdateOrder(OrderRecord order)
        {
            _orderRepository.Update(order);
        }

        public OrderRecord CreateOrder(IEnumerable<OrderProductViewModel> products)
        {
            try
            {
                var campaignProduct = _campaignService.GetCampaignProductById(products.First().ProductId);
                var Campaign = _campaignService.GetCampaignById(campaignProduct.CampaignRecord_Id); 
                var Seller = _userPartRepository.Get(Campaign.TeeyootUserId.Value);

                if (Campaign.ProductCountSold < 0)
                {
                    Campaign.ProductCountSold = 0;
                }

               


                var order = new OrderRecord
                {
                    Created = DateTime.UtcNow,
                    CurrencyRecord = Campaign.CurrencyRecord, //_currencyRrpository.Get(products.First().CurrencyId),
                    OrderPublicId = "",
                    IsActive = false,
                    Campaign = Campaign,  //_campaignRepository.Get(campaignProduct.CampaignRecord_Id),
                    Seller = Seller
                };

                order.OrderStatusRecord =(_orderStatusRepository.Get(int.Parse(OrderStatus.Pending.ToString("d"))));

                _orderRepository.Create(order);

                var ticks = DateTime.Now.Date.Ticks;

                while (ticks%10 == 0)
                {
                    ticks = ticks/10;
                }

                order.OrderPublicId = (ticks + order.Id).ToString();
                _orderRepository.Update(order);

                var productsList = new List<LinkOrderCampaignProductRecord>();
                double totalPrice = 0;
                int totalCount = 0;
                foreach (var product in products)
                {
                    var xcampaignProduct = _campaignService.GetCampaignProductById(product.ProductId);

                    var productSize = _sizeRepository.Get(product.SizeId);
                    totalCount += product.Count;

                    var campaignProductSize = xcampaignProduct.ProductRecord.SizesAvailable
                        .First(s => s.Id == product.ProductSizeId);

                    var orderProduct = new LinkOrderCampaignProductRecord
                    {
                        Count = product.Count,
                        ProductSizeRecord = productSize,
                        CampaignProductRecord = xcampaignProduct,
                        OrderRecord = order,
                        ProductColorRecord = _colorRepository.Get(product.ColorId)
                    };

                    totalPrice += (xcampaignProduct.Price + campaignProductSize.SizeCost) * product.Count;

                    _ocpRepository.Create(orderProduct);
                    productsList.Add(orderProduct);
                }

                order.TotalPrice = totalPrice;
                order.Products = productsList;

                order.TotalSold = totalCount;

                var campaignId = order.Products.First().CampaignProductRecord.CampaignRecord_Id;
                var campaign = _campaignRepository.Get(campaignId);

                // It is impossible that TeeyootUserId equals null
                if (campaign.TeeyootUserId == null)
                {
                    throw new ApplicationException(
                        "It is impossible that TeeyootUserId equals null but this finally has happened.");
                }

                var teeyootUser = _orchardServices.ContentManager.Get<TeeyootUserPart>(campaign.TeeyootUserId.Value);

                order.SellerCountry = teeyootUser.CountryRecord;
                order.SellerCurrency = teeyootUser.CurrencyRecord;

                _orderRepository.Update(order);
                _campaignService.UpdateCampaginSoldCount(campaignId);
                return order;
            }
            catch
            {
                throw;
            }
        }

        public IQueryable<LinkOrderCampaignProductRecord> GetProductsOrderedOfCampaigns(int[] ids)
        {
            return _ocpRepository.Table.Where(p => ids.Contains(p.CampaignProductRecord.CampaignRecord_Id));
        }

        public IQueryable<LinkOrderCampaignProductRecord> GetProductsOrderedOfCampaign(int campaignId)
        {
            return _ocpRepository.Table.Where(p => p.OrderRecord.Email != null && p.OrderRecord.IsActive == true && p.CampaignProductRecord.CampaignRecord_Id == campaignId);
        }

        public IQueryable<LinkOrderCampaignProductRecord> GetActiveProductsOrderedOfCampaign(int campaignId)
        {
            return
                _ocpRepository.Table.Where(
                    p =>
                        p.CampaignProductRecord.CampaignRecord_Id == campaignId && p.OrderRecord.IsActive &&
                        p.OrderRecord.OrderStatusRecord.Name != "Cancelled" &&
                        p.OrderRecord.OrderStatusRecord.Name != "Pending");
        }

        public IQueryable<LinkOrderCampaignProductRecord> GetAllOrderedProducts()
        {
            return _ocpRepository.Table;
        }

        public Task<int> GetProfitOfCampaign(int id)
        {
            return Task.Run<int>(() => GetProductsOrderedOfCampaign(id)
                .Select(p => new {Profit = p.Count*(p.CampaignProductRecord.Price - p.CampaignProductRecord.BaseCost)})
                .Sum(entry => (int?) entry.Profit) ?? 0);
        }

        public double GetProfitActiveOrdersOfCampaign(int id)
        {
            return GetActiveProductsOrderedOfCampaign(id)
                .Select(p => new {Profit = p.Count*(p.CampaignProductRecord.Price - p.CampaignProductRecord.BaseCost)})
                .Sum(entry => (double?) entry.Profit) ?? 0;
        }

        public void UpdateOrder(OrderRecord order, OrderStatus status)
        {
            order.OrderStatusRecord = (_orderStatusRepository.Get(int.Parse(status.ToString("d"))));
            _orderRepository.Update(order);
            _orderRepository.Flush();
        }

        public void DeleteOrder(int orderId)
        {
           



            var order = GetOrderById(orderId);

            // first, reduce product sold count of campaign

            if (order.OrderStatusRecord.Name == OrderStatus.Delivered.ToString() || order.OrderStatusRecord.Name == OrderStatus.Approved.ToString() || order.OrderStatusRecord.Name == OrderStatus.Printing.ToString() || order.OrderStatusRecord.Name == OrderStatus.Shipped.ToString())
            {
                if (order.Products != null && order.Products.Count > 0)
                {
                    var campaign = _campaignService.GetCampaignById(order.Products[0].CampaignProductRecord.CampaignRecord_Id);
                    campaign.ProductCountSold -= order.TotalSold; //order.Products.Sum(p => (int?) p.Count) ?? 0;
                    _campaignService.UpdateCampaign(campaign);
                }
               
            }
            
            // second, delete products

            foreach (var p in order.Products.ToList())
            {
                _ocpRepository.Delete(p);
            }
            _ocpRepository.Flush();

            // third, delete history

            foreach (var e in order.Events.ToList())
            {
                _orderHistoryRepository.Delete(e);
            }
            _orderHistoryRepository.Flush();

            // fourth, delete order itself
            var campaignId = order.Campaign.Id;

            _orderRepository.Delete(order);


            //update sold count
            try
            {
                var campaign = _campaignService.GetCampaignById(campaignId);
                int totalSold = 0;
                var orders = GetAllOrders().Where(aa => aa.Campaign.Id == campaign.Id);
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

                _campaignService.UpdateCampaign(campaign);

            }
            catch (Exception ex)
            {

            }



            _orderRepository.Flush();
        }

        public bool IsOrdersForCampaignHasStatusDeliveredAndPaid(int campignId)
        {
            var allOrderForThisCampaign =
                _ocpRepository.Table.Where(
                    l =>
                        l.CampaignProductRecord.CampaignRecord_Id == campignId &&
                        l.OrderRecord.OrderStatusRecord.Id != int.Parse(OrderStatus.Cancelled.ToString("d")) &&
                        l.OrderRecord.OrderStatusRecord.Id != int.Parse(OrderStatus.Pending.ToString("d"))).Count();
            var allOrderForThisCampignsStatusPaidAndDelivered =
                _ocpRepository.Table.Where(
                    l =>
                        l.CampaignProductRecord.CampaignRecord_Id == campignId && l.OrderRecord.ProfitPaid == true &&
                        l.OrderRecord.OrderStatusRecord.Id == int.Parse(OrderStatus.Delivered.ToString("d"))).Count();
            var allProductSoldByOrder =
                _ocpRepository.Table.Where(
                    l =>
                        l.CampaignProductRecord.CampaignRecord_Id == campignId &&
                        l.OrderRecord.OrderStatusRecord.Id != int.Parse(OrderStatus.Cancelled.ToString("d")) &&
                        l.OrderRecord.OrderStatusRecord.Id != int.Parse(OrderStatus.Pending.ToString("d")))
                    .Sum(l => (int?) l.Count) ?? 0;

            if (allOrderForThisCampaign == allOrderForThisCampignsStatusPaidAndDelivered &&
                _campaignService.GetCampaignById(campignId).ProductCountSold <= allProductSoldByOrder)
            {
                return true;
            }

            return false;
        }

        public double GetProfitByCampaign(int campaignId)
        {
            return
                _ocpRepository.Table.Where(
                    l =>
                        l.CampaignProductRecord.CampaignRecord_Id == campaignId &&
                        l.OrderRecord.OrderStatusRecord.Id != int.Parse(OrderStatus.Cancelled.ToString("d")) &&
                        l.OrderRecord.OrderStatusRecord.Id != int.Parse(OrderStatus.Pending.ToString("d")))
                    .Select(
                        p => new {Profit = p.Count*(p.CampaignProductRecord.Price - p.CampaignProductRecord.BaseCost)})
                    .Sum(entry => (double?) entry.Profit) ?? 0;
        }

        public IQueryable<OrderRecord> GetOrdersByCampaignID(int campaignId)
        {
            return _orderRepository.Table.Where(aa => aa.Campaign.Id == campaignId && aa.Email != null && aa.IsActive);
        }


        public decimal GetOrderTotalAmount(int p)
        {
            return new decimal(100);
            //throw new NotImplementedException();
        }
    }
}
