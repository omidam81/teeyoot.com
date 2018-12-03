using Orchard.Data;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Services
{
    public class BlueSnap : IBlueSnap
    {
        private readonly ICountryService _countryService;
        private readonly IOrderService _OrderService;
        private readonly IRepository<PaymentSettingsRecord> _paymentSetting;

        private readonly IMultiCountryService _multiCountryService;
        private readonly IPriceConversionService _priceConversation;


        public BlueSnap(ICountryService CountryService, IOrderService OrderService, IRepository<PaymentSettingsRecord> setting, IMultiCountryService multiCountryService,
            IPriceConversionService priceConversation
            )
        {
            _countryService = CountryService;
            _OrderService = OrderService;
            _paymentSetting = setting;
            _multiCountryService = multiCountryService;
            _priceConversation = priceConversation;

        }


        public BlueSnapReponseCardTransaction createPayment(Models.OrderRecord record, Models.CreditCardInfo CriditCardInfo)
        {
            var culture = (_multiCountryService.GetCountry().CountryCultures.FirstOrDefault() != null) ? _multiCountryService.GetCountry().CountryCultures.FirstOrDefault().CultureRecord.Culture : "en-MY"; // "en-MY";


            var setting = _paymentSetting.Table.FirstOrDefault(aa => aa.Culture == culture);
            if (setting == null) setting = _paymentSetting.Table.FirstOrDefault(aa => aa.Culture == "en-MY");


            try
            {
                //Sanbox Address https://sandbox.bluesnap.com/services/2/transactions
                //live Address https://ws.bluesnap.com/services/2/transactions


                System.Net.WebRequest request = WebRequest.Create("https://ws.bluesnap.com/services/2/transactions");
                string authInfo = string.Format("{0}:{1}", setting.BlueSnapKey, setting.BlueSnapPass);
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                request.Headers["Authorization"] = "Basic " + authInfo;
                request.ContentType = "application/xml";
                request.Method = "POST";
                byte[] buffer = Encoding.GetEncoding("UTF-8").GetBytes(GetXmlToSend(record, CriditCardInfo));
                string result = System.Convert.ToBase64String(buffer);
                Stream reqstr = request.GetRequestStream();
                reqstr.Write(buffer, 0, buffer.Length);
                reqstr.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK) return null;
                byte[] b = null;
                using (Stream stream = response.GetResponseStream())
                using (MemoryStream ms = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        byte[] buf = new byte[1024];
                        count = stream.Read(buf, 0, 1024);
                        ms.Write(buf, 0, count);
                    } while (stream.CanRead && count > 0);
                    b = ms.ToArray();
                }

                var repo = Encoding.Default.GetString(b);
                //var xDoc = XDocument.Load(response.GetResponseStream());
                var obj = XDocument.Parse(repo);
                //xDoc.LoadXml(repo);

                //dynamic root = new ExpandoObject();

                //XmlToDynamic.Parse(root, xDoc.Elements().First());

                BlueSnapReponseCardTransaction B = new BlueSnapReponseCardTransaction();

                foreach (var node in obj.Root.Elements().ToArray())
                {
                    switch (node.Name.LocalName.ToLower())
                    {
                        case "card-transaction-type":
                            B.cardTransactionType = node.Value;
                            break;
                        case "transaction-id":
                            B.transactionId = node.Value;
                            break;
                        case "recurring-transaction":
                            B.recurringTransaction = node.Value;

                            break;
                        case "soft-descriptor":
                            break;
                        case "amount":
                            B.amount = node.Value;
                            break;
                        case "currency":
                            B.currency = node.Value;
                            break;
                        case "first-name":
                            B.cardHolderInfoFirstName = node.Value;
                            break;
                        case "last-name":
                            B.cardHolderInfoLastName = node.Value;
                            break;
                        case "card-last-four-digits":
                            B.creditCardCardLastFourDigits = node.Value;
                            break;
                        case "card-type":
                            B.cardTransactionType = node.Value;
                            break;
                        case "card-sub-type":
                            B.creditCardCardSubType = node.Value;
                            break;
                        case "processing-status":
                            B.processingInfoProcessingStatus = node.Value;
                            break;
                        case "processing-info":
                            B.processingInfoProcessingStatus = node.Elements().FirstOrDefault(aa => aa.Name.LocalName.ToLower() == "processing-status").Value;
                            break;
                        default:
                            break;
                    }
                }


                return B;
            }
            catch (HttpException ex)
            {
                return new BlueSnapReponseCardTransaction() { ErrorMessage = "Something wrong!" };
            }
            catch (Exception ex2)
            {
                return new BlueSnapReponseCardTransaction() { ErrorMessage = "Something wrong!" };
            }
        }

        public string GetXmlToSend(Models.OrderRecord rec, Models.CreditCardInfo cCardInfo)
        {
            //var currency = "MYR";
            var total = ((rec.TotalPriceWithPromo > 0 ? rec.TotalPriceWithPromo : rec.TotalPrice) + rec.Delivery);
            var currency = rec.Campaign.CurrencyRecord.Code;

            if (!string.IsNullOrWhiteSpace(rec.selectedCurrency))
            {
                currency = rec.selectedCurrency;
                total = _priceConversation.ConvertPrice(total, rec.Campaign.CurrencyRecord, currency).Value;
            }
            var xmlToSend = "<card-transaction xmlns=\"http://ws.plimus.com\">" +
                               "<card-transaction-type>AUTH_CAPTURE</card-transaction-type>" +
                               "<recurring-transaction>ECOMMERCE</recurring-transaction>" +
                               "<soft-descriptor>DescTest</soft-descriptor>" +
                               "<amount>" + total.ToString("0.00") + "</amount>" +
                               "<currency>" + currency + "</currency>" +
                               "<card-holder-info>" +
                                  "<first-name>" + rec.FirstName + "</first-name>" +
                                  "<last-name>" + rec.LastName + "</last-name>" +
                               "</card-holder-info>" +
                               "<credit-card>" +
                                  "<encrypted-card-number>" + cCardInfo.CardNumber + "</encrypted-card-number>" +
                                  "<encrypted-security-code>" + cCardInfo.SecurityCode + "</encrypted-security-code>" +
                                  "<expiration-month>" + cCardInfo.ExpirationMonth + "</expiration-month>" +
                                  "<expiration-year>" + cCardInfo.ExpirationYear + "</expiration-year>" +
                                  "<card-last-four-digits>" + cCardInfo.CardLastFour + "</card-last-four-digits>" +
                               "</credit-card>" +
                            "</card-transaction>";
            return xmlToSend;
        }
    }

    public class BlueSnapReponseCardTransaction
    {

        public string cardTransactionType { get; set; }
        public string transactionId { get; set; }
        public string recurringTransaction { get; set; }
        public string amount { get; set; }
        public string currency { get; set; }
        public string cardHolderInfoFirstName { get; set; }
        public string cardHolderInfoLastName { get; set; }
        public string creditCardCardLastFourDigits { get; set; }
        public string creditCardCardType { get; set; }
        public string creditCardCardSubType { get; set; }
        public string processingInfoProcessingStatus { set; get; } //>success</processing-status>

        public string ErrorMessage { get; set; }
    }


    public class XmlToDynamic
    {
        public static void Parse(dynamic parent, XElement node)
        {
            if (node.HasElements)
            {
                if (node.Elements(node.Elements().First().Name.LocalName).Count() > 1)
                {
                    //list
                    var item = new ExpandoObject();
                    var list = new List<dynamic>();
                    foreach (var element in node.Elements())
                    {
                        Parse(list, element);
                    }

                    AddProperty(item, node.Elements().First().Name.LocalName, list);
                    AddProperty(parent, node.Name.ToString(), item);
                }
                else
                {
                    var item = new ExpandoObject();

                    foreach (var attribute in node.Attributes())
                    {
                        AddProperty(item, attribute.Name.ToString(), attribute.Value.Trim());
                    }

                    //element
                    foreach (var element in node.Elements())
                    {
                        Parse(item, element);
                    }

                    AddProperty(parent, node.Name.ToString(), item);
                }
            }
            else
            {
                AddProperty(parent, node.Name.ToString(), node.Value.Trim());
            }
        }

        private static void AddProperty(dynamic parent, string name, object value)
        {
            if (parent is List<dynamic>)
            {
                (parent as List<dynamic>).Add(value);
            }
            else
            {
                (parent as IDictionary<String, object>)[name] = value;
            }
        }
    }

}