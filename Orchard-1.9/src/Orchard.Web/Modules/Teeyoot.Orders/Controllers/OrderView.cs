using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teeyoot.Orders.Controllers
{
    public class OrderViewModel
    {
        public Module.Models.OrderRecord Order { get; set; }

        public Module.Models.CampaignRecord Campaign { get; set; }

        public Orchard.Users.Models.UserPartRecord Seller { get; set; }

        public double OrderProfit { get; set; }

        public int SoldCount { get; set; }
    }
}
