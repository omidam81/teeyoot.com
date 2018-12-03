using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.FAQ.Models;

namespace Teeyoot.Module.ViewModels
{
    public class PaymentSettingsViewModel
    {
        public bool CashDeliv { get; set; }
        public bool PayPal { get; set; }
        public bool Mol { get; set; }
        public bool CreditCard { get; set; }
        public bool SettingEmpty { get; set; }

        public string publicKey { get; set; }
        public string privateKey { get; set; }
        public string merchantId { get; set; }
        public string clientToken { get; set; }


        public string merchantIdMol { get; set; }
        public string verifyKey { get; set; }

        //
        //
        // Tab names for payment methods
        public string CashDelivTabName { get; set; }
        public string PayPalTabName { get; set; }
        public string MolTabName { get; set; }
        public string CreditCardTabName { get; set; }
        // Notes for payment methods
        public string CashDelivNote { get; set; }
        public string PayPalNote { get; set; }
        public string MolNote { get; set; }
        public string CreditCardNote { get; set; }


        public bool Ipay88 { get; set; }
        public string Ipay88MerchantCode { get; set; }
        public int Ipay88PaymentId { get; set; }
        public string Ipay88TabName { get; set; }
        public string Ipay88MerchantKey { get; set; }
        public string Ipay88Note { get; set; }

        public bool Paypal_ { get; set; }

        public string PaypalTabName_ { get; set; }

        public string PaypalClientID_ { get; set; }

        public string PaypalSecret_ { get; set; }

        public string PayPalNote_ { get; set; }

        public bool BlueSnap { get; set; }
        public string BlueSnapTabName { get; set; }
        public string BlueSnapDesc { get; set; }
        public string BlueSnapKey { get; set; }
        public string BlueSnapPass { get; set; }

    }
}