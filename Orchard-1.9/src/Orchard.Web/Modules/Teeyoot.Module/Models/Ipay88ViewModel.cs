﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teeyoot.Module.Models
{
    public class Ipay88ViewModel
    {
        public string MerchantCode { get; set; }

        public string RefNo { get; set; }

        public string Amount { get; set; }

        public string Currency { get; set; }

        public string ProdDesc { get; set; }

        public int PaymentId { get; set; }

        public string UserName { get; set; }

        public string UserEmail { get; set; }

        public string UserContact { get; set; }

        public string Remark { get; set; }

        public string Lang { get; set; }

        public string Signature { get; set; }

        public string ResponseURL { get; set; }

        public string BackendURL { get; set; }
    }
}
