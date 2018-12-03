using Orchard.Data;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;


namespace Teeyoot.Module.Services
{
    public class Paypal : IPaypal
    {
        private readonly ICountryService _countryService;
        private readonly IOrderService _OrderService;
        private readonly IRepository<PaymentSettingsRecord> _paymentSetting;
        private readonly IMultiCountryService _multiCountryService;
        private readonly IPriceConversionService _priceConverationService;


        public Paypal(ICountryService CountryService, IOrderService OrderService, IRepository<PaymentSettingsRecord> setting, IPriceConversionService priceConverationService, IMultiCountryService multiCountryService)
        {
            _countryService = CountryService;
            _OrderService = OrderService;
            _paymentSetting = setting;
            _priceConverationService = priceConverationService;
            _multiCountryService = multiCountryService;
        }


        public APIContext GetAPIContext()
        {
            APIContext apiContext = new APIContext(GetAccessToken());
            return apiContext;
        }

        private string oAuthToken = "";
        public string GetAccessToken()
        {
            var culture = "en-MY";
            try
            {
                var country = _multiCountryService.GetCountry();
                culture = country.CountryCultures.FirstOrDefault() != null ? country.CountryCultures.FirstOrDefault().CultureRecord.Culture : "en-MY";//HttpContext.Current.
            }
            catch
            {

            }
            var setting = _paymentSetting.Table.FirstOrDefault(aa => aa.Culture == culture);

            var clientId = setting.PaypalClientID_;//ConfigurationManager.AppSettings["clientId"];
            var secretToken = setting.PaypalSecret_; //ConfigurationManager.AppSettings["secretToken"];
            var config = new Dictionary<string, string> { { "mode", "sandbox" } };
            if (HttpContext.Current.Session["oAuthToken"] == null)
                HttpContext.Current.Session["oAuthToken"] = new OAuthTokenCredential(clientId, secretToken, config).GetAccessToken();
            return HttpContext.Current.Session["oAuthToken"].ToString();
        }

        public Payment CreatePayment(OrderRecord order, string cancelUrl, string returnUrl)
        {
            var currency = (order.CurrencyRecord == null) ? "MYR" : order.CurrencyRecord.Code;
            currency = order.selectedCurrency == null ? currency : order.selectedCurrency;


            Amount amount = CreateAmount(order);
            var item_list = new ItemList();
            item_list.items = new List<Item>();
            foreach (var item in order.Products)
            {
                item_list.items.Add(new Item()
                {
                    currency = currency,
                    description = item.CampaignProductRecord.ProductRecord.Details,
                    name = item.CampaignProductRecord.ProductRecord.Name,
                    quantity = item.Count.ToString(),
                    price = _priceConverationService.ConvertPrice(item.Count * item.CampaignProductRecord.Price, order.Campaign.CurrencyRecord, currency).Value.ToString("0.00").Replace(",", ".")
                });
            }
            if (order.TotalPriceWithPromo > 0)
            {
                item_list.items.Add(new Item()
                {
                    currency = currency,
                    description = "Promotion",
                    name = "Promotion",
                    quantity = "1",
                    price = _priceConverationService.ConvertPrice(order.TotalPriceWithPromo - order.TotalPrice, order.Campaign.CurrencyRecord, currency).Value.ToString("0.00").Replace(",", ".")
                });
            }

            var payment = new Payment
            {
                transactions = new List<Transaction> 
                                             { 
                                                new Transaction
                                                {
                                                    amount = amount,
                                                    description = "Purchase From Teeyoot.com, Amount:" + amount.total,
                                                    invoice_number = order.Id.ToString(),
                                                    item_list = item_list,
                                                   
                                                } 
                                             },
                intent = "sale",
                payer = new Payer
                {
                    payment_method = "paypal",
                    payer_info = new PayerInfo()
                    {
                        first_name = order.FirstName,
                        //email = order.Email,
                        last_name = order.LastName,
                        billing_address = new Address()
                        {
                            line1 = order.StreetAddress,
                            city = order.City,
                            postal_code = order.PostalCode,
                            state = order.State, 
                            country_code = "US"
                        }
                    }
                },
                redirect_urls = new RedirectUrls
                {
                    cancel_url = cancelUrl,
                    return_url = returnUrl
                }
            };
            payment = payment.Create(GetAPIContext());
            return payment;
        }

        public Payment ConfirmPayment(string token, string payerId, OrderRecord order)
        {
            var paymentExecution = new PaymentExecution
            {
                payer_id = payerId
            };

            var payment = new Payment { id = token };

            return payment.Execute(GetAPIContext(), paymentExecution);

        }

        public Amount CreateAmount(OrderRecord order)
        {
            var currency = (order.CurrencyRecord == null) ? "MYR" : order.CurrencyRecord.Code;

            currency = order.selectedCurrency == null ? currency : order.selectedCurrency;



            var subtotal = _priceConverationService.ConvertPrice((order.TotalPriceWithPromo > 0 ? order.TotalPriceWithPromo : order.TotalPrice), order.Campaign.CurrencyRecord, currency).Value.ToString("0.00").Replace(",", ".");
            var shipping = _priceConverationService.ConvertPrice(order.Delivery, order.Campaign.CurrencyRecord, currency).Value.ToString("0.00").Replace(",", ".");
            var total = float.Parse(subtotal) + float.Parse(shipping); //_priceConverationService.ConvertPrice(((order.TotalPriceWithPromo > 0 ? order.TotalPriceWithPromo : order.TotalPrice) + order.Delivery), order.Campaign.CurrencyRecord, currency).Value;



            var details = new Details
            {
                //(order.TotalPriceWithPromo > 0 ? order.TotalPriceWithPromo : order.TotalPrice)
                subtotal = subtotal,
                shipping = shipping,
                tax = "0.00".Replace(",", "."),
                fee = total.ToString("0.00").Replace(",", ".") //total.ToString("0.00").Replace(",", ".")
            };

            //var total = order.TotalPrice;

            var amount = new Amount
            {
                currency = currency,
                details = details,
                total = total.ToString("0.00").Replace(",", ".")
            };

            return amount;
        }


        public string PayWithCreditCard(CreditCardInfo info, OrderRecord rec)
        {
            try
            {
                var apiContext = GetAccessToken();


                var currency = (rec.CurrencyRecord == null) ? "MYR" : rec.CurrencyRecord.Code;
                currency = rec.selectedCurrency == null ? currency : rec.selectedCurrency;


                Amount amount = CreateAmount(rec);
                var item_list = new ItemList();
                item_list.items = new List<Item>();
                foreach (var item in rec.Products)
                {
                    item_list.items.Add(new Item()
                    {
                        currency = rec.CurrencyRecord.Code,
                        description = item.CampaignProductRecord.ProductRecord.Details,
                        name = item.CampaignProductRecord.ProductRecord.Name,
                        quantity = item.Count.ToString(),
                        price = _priceConverationService.ConvertPrice((rec.TotalPriceWithPromo > 0 ? rec.TotalPriceWithPromo : rec.TotalPrice), rec.Campaign.CurrencyRecord, currency).Value.ToString("0.00").Replace(",", ".")
                    });
                }
                if (rec.TotalPriceWithPromo > 0)
                {
                    item_list.items.Add(new Item()
                    {
                        currency = rec.CurrencyRecord.Code,
                        description = "Promotion",
                        name = "Promotion",
                        quantity = "1",
                        price = _priceConverationService.ConvertPrice((rec.TotalPriceWithPromo > 0 ? rec.TotalPriceWithPromo : rec.TotalPrice), rec.Campaign.CurrencyRecord, currency).Value.ToString("0.00").Replace(",", ".")
                    });
                }

                var transaction = new Transaction()
                {
                    amount = amount,
                    description = "Purchase From Teeyoot.com, Amount:" + amount.total,
                    invoice_number = rec.Id.ToString(),
                    item_list = item_list,

                };
                // A resource representing a Payer that funds a payment.
                var payer = new Payer()
                {
                    payment_method = "credit_card",
                    funding_instruments = new List<FundingInstrument>()
                {
                    new FundingInstrument()
                    {
                        credit_card = new CreditCard()
                        {
                            billing_address = new Address()
                            {
                                city= rec.City,
                                country_code = "US",
                                line1 = rec.StreetAddress,
                                postal_code = rec.PostalCode,
                                state = rec.State
                            },
                            cvv2 = info.SecurityCode,
                            expire_month = int.Parse(info.ExpirationMonth),
                            expire_year = int.Parse(info.ExpirationYear),
                            first_name = rec.FirstName,
                            last_name = rec.LastName, 
                            number = info.CardNumber, type= CardTypeInfo.GetCardType(info.CardNumber)//"visa"//GetCardTypeFromNumber(info.CardNumber)
                        }
                    }
                },
                    payer_info = new PayerInfo
                    {
                        email = rec.Email
                    }
                };
                var payment = new Payment()
                {
                    intent = "sale",
                    payer = payer,
                    transactions = new List<Transaction>() { transaction }
                };
                var xauth = new APIContext(apiContext);
                var createdPayment = payment.Create(xauth);
                PaymentExecution exe = new PaymentExecution();
                //exe.payer_id = PaymentExecution
                createdPayment.Execute(xauth, exe);
                return "success";
            }
            catch (Exception)
            {
                return "faild";
            }

        }
    }

    public enum CardType
    {
        Unknown = 0,
        MasterCard = 1,
        VISA = 2,
        Amex = 3,
        Discover = 4,
        DinersClub = 5,
        JCB = 6,
        enRoute = 7
    }

    // Class to hold credit card type information
    public class CardTypeInfo
    {
        public CardTypeInfo(string regEx, int length, CardType type)
        {
            RegEx = regEx;
            Length = length;
            Type = type;
        }

        public string RegEx { get; set; }
        public int Length { get; set; }
        public CardType Type { get; set; }

        // Array of CardTypeInfo objects.
        // Used by GetCardType() to identify credit card types.
        private static CardTypeInfo[] _cardTypeInfo =
{
  new CardTypeInfo("^(51|52|53|54|55)", 16, CardType.MasterCard),
  new CardTypeInfo("^(4)", 16, CardType.VISA),
  new CardTypeInfo("^(4)", 13, CardType.VISA),
  new CardTypeInfo("^(34|37)", 15, CardType.Amex),
  new CardTypeInfo("^(6011)", 16, CardType.Discover),
  new CardTypeInfo("^(300|301|302|303|304|305|36|38)", 
                   14, CardType.DinersClub),
  new CardTypeInfo("^(3)", 16, CardType.JCB),
  new CardTypeInfo("^(2131|1800)", 15, CardType.JCB),
  new CardTypeInfo("^(2014|2149)", 15, CardType.enRoute),
};

        public static string GetCardType(string cardNumber)
        {
            foreach (CardTypeInfo info in _cardTypeInfo)
            {
                if (cardNumber.Length == info.Length &&
                    Regex.IsMatch(cardNumber, info.RegEx))
                    return info.Type.ToString().ToLower();
            }

            return CardType.Unknown.ToString().ToLower();
        }
    }
}