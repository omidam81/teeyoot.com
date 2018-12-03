using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Services
{
    public class ProfitService : IProfitService
    {
        public IRepository<ProductRecord> Products { get; set; }
        public IRepository<CampaignRecord> Campaings { get; set; }
        public IRepository<OrderRecord> Orders { get; set; }
        public ProfitService(IRepository<OrderRecord> _orders, IRepository<CampaignRecord> _campaings, IRepository<ProductRecord> _products)
        {
            this.Products = _products;
            this.Campaings = _campaings;
            this.Orders = _orders;
        }

        public double CalculateProfitForCampain(OrderRecord order)
        {
            return 0;
        }
        public double CalculateProfitForProduct(CampaignRecord camp)
        {
            return 0;
        }
        public double CalCulateProfitForOrder(ProductRecord product)
        {
            return 0;

        }
        public double PriceCalculator(double baseprice, int soldCount, int target)
        {
            return 0;
        }

        public double CalculateProfitForCampain()
        {
            throw new NotImplementedException();
        }

        public double CalculateProfitForProduct()
        {
            throw new NotImplementedException();
        }

        public double CalCulateProfitForOrder()
        {
            throw new NotImplementedException();
        }
    }
}