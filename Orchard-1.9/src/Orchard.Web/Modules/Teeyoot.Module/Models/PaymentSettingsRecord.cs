using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teeyoot.Module.Models
{
    public class PaymentSettingsRecord
    {
        public virtual int Id { get; set; }
        public virtual string Culture { get; set; }
        //public virtual int PaymentMethod { get; set; }
        public virtual int Environment { get; set; }
        public virtual string PublicKey { get; set; }
        public virtual string PrivateKey { get; set; }
        public virtual string MerchantId { get; set; }
        public virtual string ClientToken { get; set; }
      

        //Payment Method
        public virtual bool CashDeliv{ get; set; }
        public virtual bool PayPal { get; set; }
        public virtual bool Mol { get; set; }
        public virtual bool CreditCard { get; set; }

        public virtual string MerchantIdMol { get; set; }
        public virtual string VerifyKey { get; set; }

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



        public bool Paypal_ { get; set; }

        public string PaypalTabName_ { get; set; }

        public string PaypalClientID_ { get; set; }

        public string PaypalSecret_ { get; set; }

        public string Ipay88Note { get; set; }
        public string PayPalNote_ { get; set; }


        public virtual CountryRecord CountryRecord { get; set; }


        ///

        public virtual bool BlueSnap { get; set; }
        public virtual string BlueSnapTabName { get; set; }
        public virtual string BlueSnapDesc { get; set; }
        public virtual string BlueSnapKey { get; set; }
        public virtual string BlueSnapPass { get; set; }
    }
}