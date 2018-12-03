using System.Collections.Generic;
using Teeyoot.Module.Models;

namespace Teeyoot.Module.ViewModels
{
    public class PaymentViewModel
    {
        public OrderRecord Order { get; set; }
        public PromotionRecord Promotion { get; set; }
        public string Result { get; set; }
        public string ClientToken { get; set; }
        public bool CashDeliv { get; set; }
        public bool PayPal { get; set; }
        public bool Mol { get; set; }
        public bool CreditCard { get; set; }
        public string CountryName { get; set; }
        //
        //
        public string CashOnDeliveryAvailabilityMessage { get; set; }
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
        //
        public string CheckoutPageRightSideContent { get; set; }
        public int SelectedDeliverableCountryId { get; set; }
        public IEnumerable<CountryItemViewModel> DeliverableCountries { get; set; }
        public int SellerCountryId { get; set; }
        public int BuyerCountryId { get; set; }
        public double ExchangeRate { get; set; }
        public string CurrencyCode { get; set; }

        public bool IPay88 { get; set; }
        public string Ipay88MerchantCode { get; set; }
        
        public string Ipay88Note { get; set; }

        public int Ipay88PaymentId { get; set; }
        public string Ipay88TabName { get; set; }


        public bool Paypal_ { get; set; }

        public string PaypalTabName_ { get; set; }

        public string PaypalClientID_ { get; set; }

        public string PaypalSecret_ { get; set; }

        public string PayPalNote_ { get; set; }

        public string exchangeRate { get; set; }

        public string SellerFbPixel { get; set; }

        public string FacebookCustomAudiencePixel { get; set; }

        public string BTClientToken { get; set; }

        public bool BlueSnap { get; set; }

        public string BlueSnapDesc { get; set; }

        public string BlueSnapKey { get; set; }

        public string BlueSnapPass { get; set; }

        public string BlueSnapTabName { get; set; }
    }
}