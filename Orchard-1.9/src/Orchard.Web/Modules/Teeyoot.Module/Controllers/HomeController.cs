using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Braintree;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Themes;
using Orchard.UI.Notify;
using RM.Localization.Services;
using Teeyoot.Localization;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Common.Utils;
using Teeyoot.Module.DTOs;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;
using System.Threading.Tasks;
using Orchard.Localization.Records;
using Teeyoot.Localization.LocalizationStorage;
using Orchard.Caching;
using PayPal;
using Orchard.Mvc.AntiForgery;
using System.Web;

namespace Teeyoot.Module.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IPromotionService _promotionService;
        private readonly ICampaignService _campaignService;
        private readonly INotifier _notifier;
        private readonly IimageHelper _imageHelper;
        private readonly IPayoutService _payoutService;
        private readonly IRepository<UserRolesPartRecord> _userRolesPartRepository;
        private readonly ITeeyootMessagingService _teeyootMessagingService;
        private readonly IPaymentSettingsService _paymentSettingsService;
        private readonly IRepository<CommonSettingsRecord> _commonSettingsRepository;
        private readonly IRepository<OrderStatusRecord> _orderStatusRepository;

        private readonly IRepository<CheckoutCampaignRequest> _checkoutRequestRepository;
        private readonly IRepository<TeeyootUserPartRecord> _userRepository;
        private readonly string _cultureUsed;
        private readonly ICookieCultureService _cookieCultureService;
        private readonly ICountryService _countryService;
        private readonly IRepository<CountryRecord> _countryRepository;
        private readonly IRepository<DeliverySettingRecord> _deliverySettingRepository;
        private readonly IRepository<DeliveryInternationalSettingRecord> _deliveryInternationalSettingRepository;
        private readonly IRepository<CurrencyExchangeRecord> _currencyExchangeRepository;
        private readonly IRepository<CultureRecord> _cultureRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IPriceConversionService _ipriceconversionservice;
        private readonly ICacheManager _cacheManager;
        private readonly IPaypal _payPal;
        private readonly IRepository<CurrencyRecord> _currencies;
        private readonly IMultiCountryService _multiCountryService;
        private readonly IPriceConversionService _priceconversationService;
        private readonly IBlueSnap _blueSnapService;

        private bool IsCurrentUserSeller
        {
            get
            {
                var currentUser = _orchardServices.WorkContext.CurrentUser;
                if (currentUser == null)
                {
                    return false;
                }

                var currentTeeyootUser = currentUser.As<TeeyootUserPart>();
                return currentTeeyootUser != null;
            }
        }

        public HomeController(
            IOrderService orderService,
            ICampaignService campaignService,
            INotifier notifier,
            IOrchardServices orchardServices,
            IPromotionService promotionService,
            IimageHelper imageHelper,
            IPaymentSettingsService paymentSettingsService,
            IShapeFactory shapeFactory,
            ITeeyootMessagingService teeyootMessagingService,
            IWorkContextAccessor workContextAccessor,
            IRepository<UserRolesPartRecord> userRolesPartRepository,
            IRepository<TeeyootUserPartRecord> userRepository,
            IPayoutService payoutService,
            IRepository<CommonSettingsRecord> commonSettingsRepository,
            IRepository<CheckoutCampaignRequest> checkoutRequestRepository,
            ICookieCultureService cookieCultureService,
            IRepository<OrderStatusRecord> orderStatusRepository,
            ICountryService countryService,
            IRepository<CountryRecord> countryRepository,
            IRepository<DeliverySettingRecord> deliverySettingRepository,
            IRepository<DeliveryInternationalSettingRecord> deliveryInternationalSettingRepository,
            IRepository<CurrencyExchangeRecord> currencyExchangeRepository,
            IRepository<CultureRecord> cultureRepository,
            IPriceConversionService ipriceconversionservice,
            ICacheManager cacheManager,
            IPaypal payPal,
            IRepository<CurrencyRecord> currencies,
            IMultiCountryService multiCountryService,
            IBlueSnap blueSnapService,
            IPriceConversionService priceconversationService
            )
        {
            _orderService = orderService;
            _promotionService = promotionService;
            _campaignService = campaignService;
            _imageHelper = imageHelper;
            _userRolesPartRepository = userRolesPartRepository;
            _payoutService = payoutService;
            _teeyootMessagingService = teeyootMessagingService;
            _paymentSettingsService = paymentSettingsService;
            _commonSettingsRepository = commonSettingsRepository;
            _checkoutRequestRepository = checkoutRequestRepository;
            _userRepository = userRepository;
            _orderStatusRepository = orderStatusRepository;
            _countryRepository = countryRepository;
            _deliverySettingRepository = deliverySettingRepository;
            _deliveryInternationalSettingRepository = deliveryInternationalSettingRepository;
            _currencyExchangeRepository = currencyExchangeRepository;
            _cultureRepository = cultureRepository;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
            _notifier = notifier;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;

            //var culture = _workContextAccessor.GetContext().CurrentCulture.Trim();
            _cultureUsed = IsCurrentUserSeller ? _orchardServices.WorkContext.CurrentUser.Get<TeeyootUserPart>().As<TeeyootUserPart>().CountryRecord.Name : "MY";
            //culture == "en-SG" ? "en-SG" : (culture == "id-ID" ? "id-ID" : "en-MY");
            _cookieCultureService = cookieCultureService;
            _countryService = countryService;
            _ipriceconversionservice = ipriceconversionservice;
            cacheManager = _cacheManager;
            _payPal = payPal;

            _currencies = currencies;
            _multiCountryService = multiCountryService;

            _blueSnapService = blueSnapService;
            _priceconversationService = priceconversationService;

        }

        private ILogger Logger { get; set; }
        private Localizer T { get; set; }

        private dynamic Shape { get; set; }

        //public static BraintreeGateway Gateway = new BraintreeGateway
        //{
        //    Environment = Braintree.Environment.SANDBOX,
        //    PublicKey = "ny4y8s7fkcvnfw9t",
        //    PrivateKey = "1532863effa7197329266f7de4837bae",
        //    MerchantId = "7qw5pmrj3hqd2hr4"
        //};
        //
        // GET: /Home/
        public string Index()
        {
            return T("Welcome to Teeyoot!").ToString();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpStatusCodeResult CreateOrder(IEnumerable<OrderProductViewModel> products, string currency)
        {
            if (!products.Any())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest,
                    T("Please, select at least one product to place your order").ToString());
            }

            foreach (var prod in products)
            {
                if (prod.Count == 0)
                {
                    prod.Count = 1;
                }
            }

            try
            {
                var orderRecord = _orderService.CreateOrder(products);//.OrderPublicId;
                var cc = _currencies.Table.FirstOrDefault(aa => aa.Code == currency);
                if (cc != null) orderRecord.CurrencyRecord = cc;
                _orderService.UpdateOrder(orderRecord);
                var id = orderRecord.OrderPublicId;
                Response.Write(id);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Logger.Error("Error occured when trying to create new order ---------------> " + e.ToString());
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
                    T("Error occured when trying to create new order").ToString());
            }
        }

        [Themed, OutputCache(NoStore = true, Duration = 0)]
        public ActionResult Payment(string orderId, string promo, string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                ViewBag.Error = error;
            }

            var order = _orderService.GetOrderByPublicId(orderId);

            if (order.OrderStatusRecord.Name != OrderStatus.Pending.ToString())
            {
                var url = "~/" + order.Campaign.Alias + (!string.IsNullOrWhiteSpace(promo) ? "?promo=" + promo : "");
                return Redirect(url);
            }
            //CurrencyRecord rec = null;
            var fromCountry = _countryService.GetAllCountry().FirstOrDefault(aa => aa.Code == "MY");


            if (order == null)
            {
                return View("NotFound", Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : "");
            }

            CountryRecord buyerCountry;
            CurrencyRecord buyerCurrency = null;

            buyerCountry = _multiCountryService.GetCountry();
            buyerCurrency = _multiCountryService.GetCurrency(); 

            var viewModel = new PaymentViewModel { SellerCountryId = fromCountry.Id };
            var setting = _paymentSettingsService.GetAllSettigns()
                .FirstOrDefault(s => s.CountryRecord.Id == buyerCountry.Id);

            if (setting == null) setting = _paymentSettingsService.GetAllSettigns()
                 .FirstOrDefault(s => s.CountryRecord.Code == "USA");

            
            var deliverableCountries = _deliveryInternationalSettingRepository.Table
                .Where(s => s.CountryFrom == fromCountry && s.IsActive)
                .Fetch(s => s.CountryTo)
                .ThenFetchMany(c => c.CountryCurrencies)
                .ThenFetch(c => c.CurrencyRecord)
                .Select(s => s.CountryTo)
                .ToList();

            deliverableCountries.Add(fromCountry);

            var deliverableCountryItems = deliverableCountries
                .Select(c =>
                {
                    //var currency = (rec == null) ? c.CountryCurrencies.First().CurrencyRecord : rec;
                    var countryItemViewModel = new CountryItemViewModel
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CurrencyCode = (c.CountryCurrencies.FirstOrDefault() == null) ? "USD" : c.CountryCurrencies.FirstOrDefault().CurrencyRecord.Code
                    };

                    if (c == fromCountry)
                    {
                        countryItemViewModel.ExchangeRate = 1;
                    }
                    else
                    {
                        var exchangeRate = _currencyExchangeRepository
                                                    .Table
                                                    .FirstOrDefault(aa => aa.CurrencyFrom == fromCountry.CountryCurrencies.FirstOrDefault().CurrencyRecord &&
                                                                          aa.CurrencyTo == c.CountryCurrencies.FirstOrDefault().CurrencyRecord);   //.FirstOrDefault(r => r.CurrencyTo.Id == currency.Id);
                        if (exchangeRate != null)
                            countryItemViewModel.ExchangeRate = IsCurrentUserSeller
                                ? exchangeRate.RateForSeller
                                : exchangeRate.RateForBuyer;
                        else
                        {
                            countryItemViewModel.ExchangeRate = 1;
                        }
                    }

                    return countryItemViewModel;
                })
                .OrderBy(c => c.Name)
                .ToList();



            var buyerCountryId = deliverableCountryItems.Any(c => c.Id == buyerCountry.Id)
                ? buyerCountry.Id
                : deliverableCountryItems.First().Id;

            var buyerCountryExchangeRate = deliverableCountryItems
                .First(c => c.Id == buyerCountryId)
                .ExchangeRate;

            var buyerCountryCurrencyCode = deliverableCountryItems
                .First(c => c.Id == buyerCountryId)
                .CurrencyCode;

            viewModel.BuyerCountryId = _multiCountryService.GetCountry().Id;
            viewModel.DeliverableCountries = deliverableCountryItems;
            viewModel.ExchangeRate = buyerCountryExchangeRate;
            viewModel.CurrencyCode = buyerCurrency.Code;//(buyerCurrency == null) ? buyerCountryCurrencyCode : buyerCurrency.Code;

            viewModel.Order = order;
            //model.ClientToken = "eyJ2ZXJzaW9uIjoyLCJhdXRob3JpemF0aW9uRmluZ2VycHJpbnQiOiI1NGU1NmE0MmMwZTIzMGFiYjkyZjk2Njc4N2I3NDY4OTEzZDc5YmU5Zjg2NzE5NjI2N2FjMDMwYzEyZjk2ZTEyfGNyZWF0ZWRfYXQ9MjAxNS0wNy0wN1QwOToxNDoyOS41NTc5MDE5NDcrMDAwMFx1MDAyNm1lcmNoYW50X2lkPWRjcHNweTJicndkanIzcW5cdTAwMjZwdWJsaWNfa2V5PTl3d3J6cWszdnIzdDRuYzgiLCJjb25maWdVcmwiOiJodHRwczovL2FwaS5zYW5kYm94LmJyYWludHJlZWdhdGV3YXkuY29tOjQ0My9tZXJjaGFudHMvZGNwc3B5MmJyd2RqcjNxbi9jbGllbnRfYXBpL3YxL2NvbmZpZ3VyYXRpb24iLCJjaGFsbGVuZ2VzIjpbXSwiZW52aXJvbm1lbnQiOiJzYW5kYm94IiwiY2xpZW50QXBpVXJsIjoiaHR0cHM6Ly9hcGkuc2FuZGJveC5icmFpbnRyZWVnYXRld2F5LmNvbTo0NDMvbWVyY2hhbnRzL2RjcHNweTJicndkanIzcW4vY2xpZW50X2FwaSIsImFzc2V0c1VybCI6Imh0dHBzOi8vYXNzZXRzLmJyYWludHJlZWdhdGV3YXkuY29tIiwiYXV0aFVybCI6Imh0dHBzOi8vYXV0aC52ZW5tby5zYW5kYm94LmJyYWludHJlZWdhdGV3YXkuY29tIiwiYW5hbHl0aWNzIjp7InVybCI6Imh0dHBzOi8vY2xpZW50LWFuYWx5dGljcy5zYW5kYm94LmJyYWludHJlZWdhdGV3YXkuY29tIn0sInRocmVlRFNlY3VyZUVuYWJsZWQiOnRydWUsInRocmVlRFNlY3VyZSI6eyJsb29rdXBVcmwiOiJodHRwczovL2FwaS5zYW5kYm94LmJyYWludHJlZWdhdGV3YXkuY29tOjQ0My9tZXJjaGFudHMvZGNwc3B5MmJyd2RqcjNxbi90aHJlZV9kX3NlY3VyZS9sb29rdXAifSwicGF5cGFsRW5hYmxlZCI6dHJ1ZSwicGF5cGFsIjp7ImRpc3BsYXlOYW1lIjoiQWNtZSBXaWRnZXRzLCBMdGQuIChTYW5kYm94KSIsImNsaWVudElkIjpudWxsLCJwcml2YWN5VXJsIjoiaHR0cDovL2V4YW1wbGUuY29tL3BwIiwidXNlckFncmVlbWVudFVybCI6Imh0dHA6Ly9leGFtcGxlLmNvbS90b3MiLCJiYXNlVXJsIjoiaHR0cHM6Ly9hc3NldHMuYnJhaW50cmVlZ2F0ZXdheS5jb20iLCJhc3NldHNVcmwiOiJodHRwczovL2NoZWNrb3V0LnBheXBhbC5jb20iLCJkaXJlY3RCYXNlVXJsIjpudWxsLCJhbGxvd0h0dHAiOnRydWUsImVudmlyb25tZW50Tm9OZXR3b3JrIjp0cnVlLCJlbnZpcm9ubWVudCI6Im9mZmxpbmUiLCJ1bnZldHRlZE1lcmNoYW50IjpmYWxzZSwiYnJhaW50cmVlQ2xpZW50SWQiOiJtYXN0ZXJjbGllbnQzIiwibWVyY2hhbnRBY2NvdW50SWQiOiJzdGNoMm5mZGZ3c3p5dHc1IiwiY3VycmVuY3lJc29Db2RlIjoiVVNEIn0sImNvaW5iYXNlRW5hYmxlZCI6dHJ1ZSwiY29pbmJhc2UiOnsiY2xpZW50SWQiOiIxMWQyNzIyOWJhNThiNTZkN2UzYzAxYTA1MjdmNGQ1YjQ0NmQ0ZjY4NDgxN2NiNjIzZDI1NWI1NzNhZGRjNTliIiwibWVyY2hhbnRBY2NvdW50IjoiY29pbmJhc2UtZGV2ZWxvcG1lbnQtbWVyY2hhbnRAZ2V0YnJhaW50cmVlLmNvbSIsInNjb3BlcyI6ImF1dGhvcml6YXRpb25zOmJyYWludHJlZSB1c2VyIiwicmVkaXJlY3RVcmwiOiJodHRwczovL2Fzc2V0cy5icmFpbnRyZWVnYXRld2F5LmNvbS9jb2luYmFzZS9vYXV0aC9yZWRpcmVjdC1sYW5kaW5nLmh0bWwiLCJlbnZpcm9ubWVudCI6Im1vY2sifSwibWVyY2hhbnRJZCI6ImRjcHNweTJicndkanIzcW4iLCJ2ZW5tbyI6Im9mZmxpbmUiLCJhcHBsZVBheSI6eyJzdGF0dXMiOiJtb2NrIiwiY291bnRyeUNvZGUiOiJVUyIsImN1cnJlbmN5Q29kZSI6IlVTRCIsIm1lcmNoYW50SWRlbnRpZmllciI6Im1lcmNoYW50LmNvbS5icmFpbnRyZWVwYXltZW50cy5zYW5kYm94LkJyYWludHJlZS1EZW1vIiwic3VwcG9ydGVkTmV0d29ya3MiOlsidmlzYSIsIm1hc3RlcmNhcmQiLCJhbWV4Il19fQ==";
            viewModel.ClientToken = setting.ClientToken;
            //var setting = _paymentSettingsService.GetAllSettigns().FirstOrDefault(s => s.Culture == DEFAULT_LANGUAGE_CODE);

            viewModel.CashDeliv = setting.CashDeliv;
            viewModel.CreditCard = setting.CreditCard;
            viewModel.Mol = setting.Mol;
            viewModel.PayPal = setting.PayPal;

            //
            //
            //
            // Tab names for payment methods
            viewModel.CashDelivTabName = setting.CashDelivTabName;
            viewModel.PayPalTabName = setting.PayPalTabName;
            viewModel.MolTabName = setting.MolTabName;
            viewModel.CreditCardTabName = setting.CreditCardTabName;
            // Notes for payment methods
            viewModel.CashDelivNote = setting.CashDelivNote;
            viewModel.PayPalNote = setting.PayPalNote;
            viewModel.MolNote = setting.MolNote;
            viewModel.CreditCardNote = setting.CreditCardNote;
            //
            var commonSettings = _commonSettingsRepository.Table.First();
            viewModel.CashOnDeliveryAvailabilityMessage = commonSettings.CashOnDeliveryAvailabilityMessage;
            viewModel.CheckoutPageRightSideContent = commonSettings.CheckoutPageRightSideContent;
            //
            //
            viewModel.IPay88 = setting.Ipay88;
            viewModel.Ipay88MerchantCode = setting.Ipay88MerchantCode;
            viewModel.Ipay88PaymentId = setting.Ipay88PaymentId;
            viewModel.Ipay88TabName = setting.Ipay88TabName;
            viewModel.Ipay88Note = setting.Ipay88Note;

            //


            ///Paypal
            ///
            viewModel.Paypal_ = setting.Paypal_;
            viewModel.PaypalTabName_ = setting.PaypalTabName_;
            viewModel.PayPalNote_ = setting.PayPalNote_;

            ///BlueSbnap
            ///

            viewModel.BlueSnap = setting.BlueSnap;
            viewModel.BlueSnapDesc = setting.BlueSnapDesc;
            viewModel.BlueSnapKey = setting.BlueSnapKey;
            viewModel.BlueSnapPass = setting.BlueSnapPass;
            viewModel.BlueSnapTabName = setting.BlueSnapTabName;



            if (promo != null)
            {
                var promotion = _promotionService.GetPromotionByPromoId(promo);
                viewModel.Promotion = promotion;

                if (promotion.AmountType == "%")
                {
                    viewModel.Order.Promotion = (viewModel.Order.TotalPrice / 100) * promotion.AmountSize;
                    viewModel.Order.TotalPriceWithPromo = viewModel.Order.TotalPrice - viewModel.Order.Promotion;
                }
                else
                {
                    var currency = _multiCountryService.GetCurrency();

                    if (order.CurrencyRecord == currency)
                    {
                        viewModel.Order.Promotion = promotion.AmountSize;
                        viewModel.Order.TotalPriceWithPromo = viewModel.Order.TotalPrice - viewModel.Order.Promotion;
                    }

                    /*
                        if (promotion.AmountType == order.CurrencyRecord.Code)
                        {
                            model.Order.Promotion = promotion.AmountSize;
                            model.Order.TotalPriceWithPromo = model.Order.TotalPrice - model.Order.Promotion;
                        }
                         */
                }
            }
            viewModel.exchangeRate = Newtonsoft.Json.JsonConvert.SerializeObject(_currencyExchangeRepository.Table.Select(aa => new { From = aa.CurrencyFrom.Code, To = aa.CurrencyTo.Code, RateForSeller = aa.RateForSeller, RateForBuyer = aa.RateForBuyer }));


            var selleruser = _userRepository.Get(order.Campaign.Seller.Id);
            if (selleruser != null)
            {

                viewModel.SellerFbPixel = !string.IsNullOrWhiteSpace(order.Campaign.FBPixelId) ? order.Campaign.FBPixelId : selleruser.DefaultFBPixelId;
                viewModel.FacebookCustomAudiencePixel = !string.IsNullOrWhiteSpace(order.Campaign.FBPixelId) ? order.Campaign.FBPixelId : selleruser.DefaultFacebookCustomAudiencePixel;
            }
            else
            {
                viewModel.SellerFbPixel = order.Campaign.FBPixelId;
            }

            var gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                PublicKey = setting.PublicKey,
                PrivateKey = setting.PrivateKey,
                MerchantId = setting.MerchantId
            };
            try
            {
                viewModel.BTClientToken = gateway.ClientToken.generate();
            }
            catch (Exception ex)
            {
                viewModel.BTClientToken = "";

            }

            return View(viewModel);
        }

        private string GetVCode(string input)
        {
            var result = "";
            var md5 = MD5.Create();
            var md5Bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var stringBuilder = new StringBuilder();
            foreach (byte data in md5Bytes)
            {
                stringBuilder.Append(data.ToString("x2"));
            }
            result = stringBuilder.ToString();

            return result;
        }

        public string Molpay(OrderRecord order,
            CountryRecord deliveryCountry,
            string country,
            string firstName,
            string lastName,
            string email,
            string state,
            string phone,
            double deliveryCost)
        {
            var setting = _paymentSettingsService.GetAllSettigns()
                .First(s => s.CountryRecord == deliveryCountry);

            /*
            var setting = _paymentSettingsService.GetAllSettigns()
                .FirstOrDefault(s => s.CountryRecord.Id == _countryService.GetCountryByCulture(_cultureUsed).Id);
             */

            //var merchantId = "teeyoot1_Dev";
            //var verifyKey = "856287426298f7e8508eae9896c09c03";
            var merchantId = setting.MerchantIdMol;
            var verifyKey = setting.VerifyKey;

            //var Total = order.TotalPrice;
            var total = _priceconversationService.ConvertPrice(((order.TotalPriceWithPromo > 0 ? order.TotalPriceWithPromo : order.TotalPrice) + deliveryCost),
                                                               order.Campaign.CurrencyRecord, "MYR").Value.ToString("F2", CultureInfo.InvariantCulture);
            var orderNumber = order.Id;

            var campaign =
                _campaignService.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id);
            var title = campaign.Title;
            var campId = campaign.Id;
            var name = firstName + " " + lastName;
            var Email = email;
            var Phone = phone;
            var description = campId + ", " + title + "," + "\nOrdered from Teeyoot " + country + " " + state;

            var vCode = GetVCode(total + merchantId + orderNumber + verifyKey);
            //var paymentUrl = "";
            //if (method == "credit")
            //    {
            //        paymentUrl = "https://www.onlinepayment.com.my/MOLPay/pay/" + merchantId + "?amount=" +
            //                  Total + "&orderid=" + OrderNumber +
            //                  "&bill_name=" + Name + "&channel=credit&bill_email=" + Email + "&bill_mobile=" + Phone +
            //                  "&bill_desc=" + Description + "&vcode=" + vCode;

            //    } else {
            var paymentUrl = "https://www.onlinepayment.com.my/MOLPay/pay/" + merchantId + "?amount=" +
                             total + "&orderid=" + orderNumber +
                             "&bill_name=" + name + "&bill_email=" + Email + "&bill_mobile=" + Phone +
                             "&bill_desc=" + description + "&vcode=" + vCode;

            //}

            return paymentUrl;
        }

        public ActionResult Molpas()
        {
            const string merchantId = "7qw5pmrj3hqd2hr4";
            const string total = "51.99";
            const int orderNumber = 352;
            const string verifyKey = "856287426298f7e8508eae9896c09c03";
            var vCode = GetVCode(total + merchantId + orderNumber + verifyKey);
            var model = new MolpasViewModel { vcode = vCode };

            return View(model);
        }

        //[HttpGet]
        public ActionResult CallbackMolpay(string amount, string orderid, string appcode, string tranID, string domain,
            string status, string error_code,
            string error_desc, string currency, string paydate, string channel, string skey)
        {

            string destFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/molPayLog");
            var dir = new DirectoryInfo(destFolder);

            if (!dir.Exists)
            {
                Directory.CreateDirectory(destFolder);
            }
            var request = System.Web.HttpContext.Current.Request;
            System.IO.File.AppendAllText(destFolder + "/mol.txt",
                DateTime.Now + "  -------------  " + "Return Url status:" + status + "; amount: " + amount +
                "; orderid: " + orderid + "; error_desc: " + error_desc + "          " + request.Url + "\r\n" +
                "tranId: " + tranID +
                " domain: " + domain + " error_code: " + error_code + "; skey: " + skey + "; channel: " + channel +
                "\r\n");

            if (orderid == null)
                return View();


            var order = _orderService.GetOrderById(Convert.ToInt32(orderid));
            var campaign =
                _campaignService.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id);

            if (status == "00")
            {
                if (order.OrderStatusRecord !=
                    _orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()) || true)
                {
                    amount = amount.Replace(".", ",");
                    order.OrderStatusRecord = (_orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()));
                    order.Paid = DateTime.Now.ToUniversalTime();
                    order.ProfitPaid = true;
                    _orderService.UpdateOrder(order);

                    var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                    var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                      Request.ApplicationPath.TrimEnd('/');

                    _teeyootMessagingService.SendNewOrderMessageToAdmin(order.Id, pathToMedia, pathToTemplates);
                    _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, order.Id,
                        OrderStatus.Approved.ToString());

                    //if (campaign.ProductCountSold >= campaign.ProductCountGoal)
                    //{
                    //    campaign.ProductCountSold += order.TotalSold; //order.Products.Sum(p => (int?) p.Count) ?? 0;
                    //    _campaignService.UpdateCampaign(campaign);
                    //}
                    //else
                    //{
                    //    campaign.ProductCountSold += order.TotalSold; //order.Products.Sum(p => (int?) p.Count) ?? 0;
                    //    _campaignService.UpdateCampaign(campaign);

                    //    //TYT-78--all code commented

                    //    //if (campaign.ProductCountSold >= campaign.ProductMinimumGoal)
                    //    //{
                    //    //    _teeyootMessagingService.SendCampaignMetMinimumMessageToBuyers(campaign.Id);
                    //    //    _teeyootMessagingService.SendCampaignMetMinimumMessageToSeller(campaign.Id);
                    //    //}
                    //    ///end TYT-78
                    //}




                    var commonSettings =
                        _commonSettingsRepository.Table.Where(
                            s => s.CountryRecord.Id == _countryService.GetCountryByCulture(_cultureUsed).Id)
                            .FirstOrDefault();
                    if (commonSettings == null)
                    {
                        _commonSettingsRepository.Create(new CommonSettingsRecord()
                        {
                            DoNotAcceptAnyNewCampaigns = false,
                            CountryRecord = _countryService.GetCountryByCulture(_cultureUsed)
                        });
                        commonSettings =
                            _commonSettingsRepository.Table.Where(
                                s => s.CountryRecord.Id == _countryService.GetCountryByCulture(_cultureUsed).Id).First();
                    }
                }
            }
            else if (status == "11")
            {
                if (order.OrderStatusRecord !=
                    _orderStatusRepository.Table.First(s => s.Name == OrderStatus.Cancelled.ToString()) && order.OrderStatusRecord != _orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()))
                {
                    order.OrderStatusRecord = (_orderStatusRepository.Table.First(s => s.Name == OrderStatus.Cancelled.ToString()));
                    order.Paid = null;
                    order.ProfitPaid = false;
                    _orderService.UpdateOrder(order);
                    _payoutService.DeletePayoutByOrderPublicId(order.OrderPublicId);
                    var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                    var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                      Request.ApplicationPath.TrimEnd('/');
                    _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, order.Id,
                        OrderStatus.Cancelled.ToString());
                }
                return RedirectToAction("ReservationComplete",
                    new { campaignId = campaign.Id, sellerId = campaign.TeeyootUserId, Id = order.Id, oops = true });
            }
            return RedirectToAction("ReservationComplete",
                new { campaignId = campaign.Id, sellerId = campaign.TeeyootUserId, Id = order.Id });


        }

        public JsonResult GetSettings(int countryFromId, int countryToId, int? orderId, bool cashOnDelivery = false)
        {


            //var order = _orderService.GetActiveOrderById(aa=>aa.Id == orderId);

            countryFromId = _countryService.GetAllCountry().FirstOrDefault(aa => aa.Code == "MY").Id;

            var currency = (IsCurrentUserSeller) ? _orchardServices.WorkContext.CurrentUser.Get<TeeyootUserPart>().As<TeeyootUserPart>().CurrencyRecord : null;

            var countryFrom = _countryRepository.Get(countryFromId);
            var countryTo = _countryRepository.Get(countryToId);

            IEnumerable<DeliverySettingItem> settings;

            if (countryFrom == countryTo)
            {
                settings = _deliverySettingRepository.Table
                    .Where(s => s.Country == countryTo)
                    .Select(s => new DeliverySettingItem
                    {
                        DeliveryTime = (s.DeliveryTime == 0) ? 5 : s.DeliveryTime,
                        State = s.State,
                        Enabled = (cashOnDelivery ? s.Enabled : true),
                        DeliveryCost = (currency == null) ? (cashOnDelivery ? s.CodCost : s.PostageCost) : (cashOnDelivery ? s.CodCost : s.PostageCost)
                    })
                    .ToList();
            }
            else
            {
                var states = _deliverySettingRepository.Table
                    .Where(s => s.Country == countryTo)
                    .Select(s => s.State)
                    .ToList();

                var setting = _deliveryInternationalSettingRepository.Table
                    .FirstOrDefault(s => s.CountryFrom == countryFrom && s.CountryTo == countryTo);
                if (setting == null)
                {
                    settings = states.Select(s => new DeliverySettingItem
                    {
                        DeliveryTime = 5,
                        State = s,
                        Enabled = true,
                        DeliveryCost = (currency == null) ? setting.DeliveryPrice : setting.DeliveryPrice,
                    });
                }
                else
                {
                    settings = states.Select(s => new DeliverySettingItem
                    {
                        DeliveryTime = setting.DeliveryTime,
                        State = s,
                        Enabled = true,
                        DeliveryCost = (currency == null) ? setting.DeliveryPrice : setting.DeliveryPrice,
                    });
                }

            }
            return Json(new { settings }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTransaction(FormCollection collection, string selectedCurrency)
        {
            var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
            var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

            var setting = _paymentSettingsService.GetAllSettigns()
                .FirstOrDefault(s => s.CountryRecord.Id == _countryService.GetCountryByCulture(_cultureUsed).Id);

            var gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.PRODUCTION,
                PublicKey = setting.PublicKey,
                PrivateKey = setting.PrivateKey,
                MerchantId = setting.MerchantId
            };

            var countryId = Convert.ToInt32(collection["Country"]);

            var deliveryCountry = _countryRepository.Get(countryId);
            var deliveryCountryCurrency = deliveryCountry.CountryCurrencies.First().CurrencyRecord;

            var id1 = int.Parse(collection["OrderId"]);
            var orderRec = _orderService.GetOrderById(id1);

            orderRec.selectedCurrency = (selectedCurrency != null) ? selectedCurrency : "";
            
            //updateOrdercount(orderRec);


            switch (collection["paumentMeth"])
            {
                case "2":
                    {
                        var requestPayPal = new TransactionRequest
                        {
                            Amount = _orderService.GetOrderTotalAmount(orderRec.Id),
                            PaymentMethodNonce = "BrainTree-CreditCard",
                            Options = new TransactionOptionsRequest
                            {
                                SubmitForSettlement = false,
                                StoreInVault = true
                            },
                        };
                        orderRec.PaymentMethod = "BrainTree-PayPal";
                        _orderService.UpdateOrder(orderRec);
                        gateway.Transaction.Sale(requestPayPal);
                        break;
                    }
                case "1":
                    {
                        var requestCard = new TransactionRequest
                        {
                            Amount = _orderService.GetOrderTotalAmount(orderRec.Id),
                            CreditCard = new TransactionCreditCardRequest
                            {
                                Number = collection["number"],
                                CVV = collection["cvv"],
                                ExpirationMonth = collection["month"],
                                ExpirationYear = collection["year"]
                            },
                            Options = new TransactionOptionsRequest
                            {
                                StoreInVault = true,
                                SubmitForSettlement = false
                            }
                        };
                        orderRec.PaymentMethod = "BrainTree-CreditCard";
                        _orderService.UpdateOrder(orderRec);
                        gateway.Transaction.Sale(requestCard);
                        break;
                    }
                case "4":
                    orderRec.PaymentMethod = "COD";
                    _orderService.UpdateOrder(orderRec);
                    break;
                case "3":
                    {
                        var method = collection["Bank"];
                        var meth1 = collection["paumentMeth"];
                        var id2 = int.Parse(collection["OrderId"]);
                        var orderMol = orderRec;
                        var campId = orderMol.Products.First().CampaignProductRecord.CampaignRecord_Id;
                        orderMol.Email = collection["Email"];
                        orderMol.FirstName = collection["FirstName"];
                        orderMol.LastName = collection["LastName"];
                        orderMol.StreetAddress = collection["StreetAddress"] + " " + collection["StreetAddress2"];
                        orderMol.City = collection["City"];
                        orderMol.State = collection["State"];
                        orderMol.PostalCode = collection["PostalCode"];
                        orderMol.Country = deliveryCountry.Name;
                        orderMol.PhoneNumber = collection["PhoneNumber"];
                        orderMol.Reserved = DateTime.UtcNow;
                        orderMol.OrderStatusRecord = (_orderStatusRepository
                            .Get(int.Parse(OrderStatus.Pending.ToString("d"))));
                        orderMol.PaymentMethod = "MOL";
                        
                        SetOrderData(orderMol,
                            deliveryCountry,
                            deliveryCountryCurrency,
                            collection["paumentMeth"],
                            collection["State"]);

                        orderMol.IsActive = true;

                        _orderService.UpdateOrder(orderMol);

                        if (collection["PromoId"] != null)
                        {
                            var promotion = _promotionService.GetPromotionByPromoId(collection["PromoId"]);
                            promotion.Redeemed = promotion.Redeemed + 1;
                        }
                        
                        var url = Molpay(
                            _orderService.GetOrderById(int.Parse(collection["OrderId"])),
                            deliveryCountry,
                            deliveryCountry.Name,
                            collection["FirstName"],
                            collection["LastName"],
                            collection["Email"],
                            collection["State"],
                            collection["PhoneNumber"],
                            orderMol.Delivery);

                        return Redirect(url);
                    }
                case "5":
                    {
                        var id2 = int.Parse(collection["OrderId"]);
                        var campId = orderRec.Products.First().CampaignProductRecord.CampaignRecord_Id;
                        orderRec.Email = collection["Email"];
                        orderRec.FirstName = collection["FirstName"];
                        orderRec.LastName = collection["LastName"];
                        orderRec.StreetAddress = collection["StreetAddress"] + " " + collection["StreetAddress2"];
                        orderRec.City = collection["City"];
                        orderRec.State = collection["State"];
                        orderRec.PostalCode = collection["PostalCode"];
                        orderRec.Country = deliveryCountry.Name;
                        orderRec.PhoneNumber = collection["PhoneNumber"];
                        orderRec.Reserved = DateTime.UtcNow;
                        orderRec.OrderStatusRecord = (_orderStatusRepository.Get(int.Parse(OrderStatus.Pending.ToString("d"))));
                        orderRec.PaymentMethod = "IPay88";
                        orderRec.IsActive = true;


                        SetOrderData(orderRec,
                            deliveryCountry,
                            deliveryCountryCurrency,
                            collection["paumentMeth"],
                            collection["State"]);
                        _orderService.UpdateOrder(orderRec);
                        
                        return RedirectToAction("IPay88RedirectPage", "Home", new { OrderID = orderRec.Id, settingID = setting.Id });
                    }
                case "6":
                    {
                        var method = collection["Bank"];
                        var meth1 = collection["paumentMeth"];
                        //var id1 = int.Parse(collection["OrderId"]);
                        var campId = orderRec.Products.First().CampaignProductRecord.CampaignRecord_Id;
                        orderRec.Email = collection["Email"];
                        orderRec.FirstName = collection["FirstName"];
                        orderRec.LastName = collection["LastName"];
                        orderRec.StreetAddress = collection["StreetAddress"] + " " + collection["StreetAddress2"];
                        orderRec.City = collection["City"];
                        orderRec.State = collection["State"];
                        orderRec.PostalCode = collection["PostalCode"];
                        orderRec.Country = deliveryCountry.Name;
                        orderRec.PhoneNumber = collection["PhoneNumber"];
                        orderRec.Reserved = DateTime.UtcNow;

                        orderRec.OrderStatusRecord = (_orderStatusRepository.Get(int.Parse(OrderStatus.Pending.ToString("d"))));

                        SetOrderData(orderRec,
                            deliveryCountry,
                            deliveryCountryCurrency,
                            collection["paumentMeth"],
                            collection["State"]);
                        orderRec.IsActive = true;
                        if (collection["PromoId"] != null)
                        {
                            var promotion = _promotionService.GetPromotionByPromoId(collection["PromoId"]);
                            promotion.Redeemed = promotion.Redeemed + 1;
                        }
                        orderRec.PaymentMethod = "Paypal";

                        SetOrderData(orderRec,
                          deliveryCountry,
                          deliveryCountryCurrency,
                          collection["paumentMeth"],
                          collection["State"]);



                        _orderService.UpdateOrder(orderRec);

                        return PaypalPayment(orderRec, collection);
                    }
                case "7":
                    orderRec.Email = collection["Email"];
                    orderRec.FirstName = collection["FirstName"];
                    orderRec.LastName = collection["LastName"];
                    orderRec.StreetAddress = collection["StreetAddress"] + " " + collection["StreetAddress2"];
                    orderRec.City = collection["City"];
                    orderRec.State = collection["State"];
                    orderRec.PostalCode = collection["PostalCode"];
                    orderRec.Country = deliveryCountry.Name;
                    orderRec.PhoneNumber = collection["PhoneNumber"];
                    orderRec.Reserved = DateTime.UtcNow;

                    orderRec.OrderStatusRecord = (_orderStatusRepository.Get(int.Parse(OrderStatus.Pending.ToString("d"))));
                    orderRec.FirstName = collection["FirstName"];
                    orderRec.LastName = collection["LastName"];
                    SetOrderData(orderRec,
                                               deliveryCountry,
                                               deliveryCountryCurrency,
                                               collection["paumentMeth"],
                                               collection["State"]);
                    var x = _blueSnapService.createPayment(orderRec, new CreditCardInfo()
                    {
                        ExpirationMonth = collection["exp-month"],
                        CardNumber = collection["encryptedCreditCard"],
                        ExpirationYear = collection["exp-year"],
                        SecurityCode = collection["encryptedCvv"],
                        CardLastFour = collection["ccLast4Digits"]
                    });

                    if (x != null && x.processingInfoProcessingStatus == "success")
                    {
                        orderRec.PaymentMethod = "BlueSnap";
                        orderRec.BlueSnapTransationId = x.transactionId;
                        _orderService.UpdateOrder(orderRec);
                        
                        orderRec.Paid = DateTime.Now.ToUniversalTime();
                        orderRec.ProfitPaid = true;
                        _orderService.UpdateOrder(orderRec);
                    }
                    else
                    {
                        return Redirect(Request.UrlReferrer.ToString() + "?error=" + "Your credit card info is not correct!");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var meth = collection["paumentMeth"];
            var id = int.Parse(collection["OrderId"]);
            var order = _orderService.GetOrderById(id);
            var campaignId = order.Products.First().CampaignProductRecord.CampaignRecord_Id;
            order.Email = collection["Email"];
            order.BuyerCultureRecord = _cultureRepository.Table.Where(c => c.Culture == _cultureUsed).FirstOrDefault();
            order.FirstName = collection["FirstName"];
            order.LastName = collection["LastName"];
            order.StreetAddress = collection["StreetAddress"] + " " + collection["StreetAddress2"];
            order.City = collection["City"];
            order.State = collection["State"];
            order.PostalCode = collection["PostalCode"];
            order.Country = deliveryCountry.Name;
            order.PhoneNumber = collection["PhoneNumber"];
            order.Reserved = DateTime.UtcNow;

            SetOrderData(order,
                deliveryCountry,
                deliveryCountryCurrency,
                collection["paumentMeth"],
                collection["State"]);
            order.OrderStatusRecord = (_orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()));
           
            order.IsActive = true;
            _teeyootMessagingService.SendNewOrderMessageToAdmin(order.Id, pathToMedia, pathToTemplates);
            _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates,
                pathToMedia,
                order.Id,
                OrderStatus.Approved.ToString());

            var campaign = _campaignService.GetCampaignById(campaignId);
            var sendMessage = campaign.ProductCountSold <= campaign.ProductMinimumGoal - 1;

            if (collection["PromoId"] != null)
            {
                var promotion = _promotionService.GetPromotionByPromoId(collection["PromoId"]);
                promotion.Redeemed = promotion.Redeemed + 1;
            }

            var commonSettings = _commonSettingsRepository.Table
                .FirstOrDefault(s => s.CountryRecord.Id == _countryService.GetCountryByCulture(_cultureUsed).Id);

            if (commonSettings == null)
            {
                _commonSettingsRepository.Create(new CommonSettingsRecord()
                {
                    DoNotAcceptAnyNewCampaigns = false,
                    CountryRecord = _countryService.GetCountryByCulture(_cultureUsed)
                });
                commonSettings = _commonSettingsRepository.Table
                    .First(s => s.CountryRecord.Id == _countryService.GetCountryByCulture(_cultureUsed).Id);
            }

            if (commonSettings.DoNotAcceptAnyNewCampaigns)
            {
                var request = new CheckoutCampaignRequest
                {
                    RequestUtcDate = DateTime.UtcNow,
                    Email = order.Email,
                    BuyerCultureRecord = _cultureRepository.Table.Where(c => c.Culture == _cultureUsed).First()
                };
                _checkoutRequestRepository.Create(request);
            }

            return RedirectToAction("ReservationComplete",
                new { campaignId = campaign.Id, sellerId = campaign.TeeyootUserId, Id = order.Id });
        }

        private void updateOrdercount(OrderRecord orderRec)
        {
            //throw new NotImplementedException();
        }

        private ActionResult PaypalPayment(OrderRecord orderRec, FormCollection form)
        {
            var baseUrl = string.Concat(this.Request.Url.Scheme, "://", this.Request.Url.Authority);

            //try
            //{
            var order = orderRec; // Retrieve/create your order object somehow

            var cancelUrl = "/purchaseunsucessful";
            var returnUrl = "/purchasecomplete?orderNumber=" + order.OrderPublicId.ToLower();


            var payment = this._payPal.CreatePayment(order, string.Concat(baseUrl, cancelUrl), string.Concat(baseUrl, returnUrl));

            //if (!payment.WasCreated())
            //{
            //    return this.RedirectToAction("purchaseunsucessful");
            //}

            // Save order with payment.id somewhere on it

            return this.Redirect(payment.GetApprovalUrl(true));
            //}
            //catch (PayPalException ex)
            //{
            //    this.ViewBag.Error = ((ConnectionException)ex.InnerException).Response;
            //}
            //catch (Exception ex)
            //{
            //    this.ViewBag.Error = ex.Message;
            //}

            return this.RedirectToAction("purchaseunsucessful");
        }

        [Themed]
        public ActionResult ReservationComplete(int campaignId, int sellerId, int? Id, bool oops = false)
        {
            var order_id = Id;
            var campaigns = _campaignService.GetAllCampaigns()
                .Where(c => c.TeeyootUserId == sellerId && c.IsApproved && c.Id != campaignId)
                .Select(c => new
                {
                    Id = c.Id,
                    Alias = c.Alias,
                    Title = c.Title,
                    Goal = c.ProductMinimumGoal,
                    Sold = c.ProductCountSold,
                    ShowBack = c.BackSideByDefault,
                    EndDate = c.EndDate,
                    FlagFileName = c.CurrencyRecord.FlagFileName
                }).OrderByDescending(aa => aa.EndDate).ToArray();


            if (!order_id.HasValue && Id.HasValue) order_id = Id;
            //get Order
            var order = _orderService.GetOrderById(order_id.Value);
            
            //updateCamoaignTotalSold

            _campaignService.UpdateCampaginSoldCount(order.Campaign.Id);


            var entriesProjection = campaigns.Select(e =>
            {
                return Shape.campaign(
                    Id: e.Id,
                    Title: e.Title,
                    Sold: e.Sold,
                    Goal: e.Goal,
                    ShowBack: e.ShowBack,
                    Alias: e.Alias,
                    EndDate: e.EndDate,
                    FlagFileName: e.FlagFileName,
                    FirstProductId:
                        _campaignService.GetAllCampaignProducts()
                            .First(p => p.CampaignRecord_Id == e.Id && p.WhenDeleted == null)
                            .Id
                    );
            });

            var model = new ReservationCompleteViewModel();
            if (!oops)
            {
                model.Oops = false;
                model.Message =
                    T(
                        "Your reservation is confirmed. We will notify you once the T-shirt is ready. Meanwhile check out other designs or campaigns from the same seller")
                        .ToString();
            }
            else
            {
                model.Message =
                    T("Oops! we couldn't process this order, you can try to change your payment method and try again")
                        .ToString();
                model.Oops = true;
            }



            model.Campaigns = entriesProjection.ToArray();
            model.Order = order;


            var ____user = _userRepository.Get(order.Seller.Id);
            if (____user != null)
            {

                model.SellerFbPixel = !string.IsNullOrWhiteSpace(model.Order.Campaign.FBPixelId) ? model.Order.Campaign.FBPixelId : ____user.DefaultFBPixelId;
                model.FacebookCustomAudiencePixel = !string.IsNullOrWhiteSpace(order.Campaign.FBPixelId) ? order.Campaign.FBPixelId : ____user.DefaultFacebookCustomAudiencePixel;
                
            }
            else
            {
                model.SellerFbPixel = model.Order.Campaign.FBPixelId;
            }


            return View(model);
        }

        [Themed]
        public ActionResult TrackOrder()
        {
            var message = TempData["OrderNotFoundMessage"];
            if (message != null && !string.IsNullOrWhiteSpace(message.ToString()))
                _notifier.Error(T(message.ToString()));
            return View();
        }

        [HttpPost]
        public ActionResult SearchForOrder(string orderId)
        {
            return RedirectToAction("OrderTracking", new { orderId = orderId.Trim() });
        }

        [Themed]
        [HttpGet]
        public ActionResult OrderTracking(string orderId)
        {
            var order = _orderService.GetActiveOrderByPublicId(orderId);

            if (order == null)
            {
                _notifier.Error(T("We cannot find the order number you entered"));
                return View("TrackOrder");
            }

            if (order.OrderStatusRecord.Name == OrderStatus.Pending.ToString())
            {
                _notifier.Error(T("We have not received your payment or you did not complete the payment process Please wait for 24 hours, or contact us at support@teeyoot.com"));
                return View("TrackOrder");
            }

            if (order.OrderStatusRecord.Name == OrderStatus.Cancelled.ToString())
            {
                _notifier.Error(T(@"The order number you entered has been canceled for one or more of these reasons: <br />
                                    The contact details you provided was not accurate <br />
                                    There was a problem with your payment <br />
                                    If you believe that none of the above applies to you or your order was wrongly canceled, please contact us immediately at support@teeyoot.com"));
                return View("TrackOrder");
            }

            var model = new OrderTrackingViewModel();
            model.OrderId = order.Id;
            model.OrderPublicId = orderId;
            model.Status = order.OrderStatusRecord;
            model.Products = order.Products.ToArray();
            model.ShippingTo = new string[]
            {
                order.FirstName + " " + order.LastName,
                order.StreetAddress,
                order.City + ", " + order.State + ", " + order.Country + " " + order.PostalCode
            };
            model.Events = order.Events.ToArray();
            model.CultureInfo = CultureInfo.GetCultureInfo("en-MY");
            model.CreateDate = order.Created.ToLocalTime().ToString("dd MMM HH:mm", model.CultureInfo);
            var campaign = _campaignService.GetCampaignById(order.Products[0].CampaignProductRecord.CampaignRecord_Id);
            model.CampaignName = campaign.Title;
            model.CampaignAlias = campaign.Alias;
            model.TotalPrice = (order.TotalPrice + order.Delivery - order.Promotion).ToString();
            model.Delivery = order.Delivery.ToString();
            model.Promotion = order.Promotion == 0 ? string.Empty : order.Promotion.ToString();

            return View(model);
        }

        [Themed]
        public ActionResult RecoverOrder(string email)
        {
            if (email == null)
            {
                return View("RecoverOrder");
            }
            var orders = _orderService.GetActiveOrdersByEmailForLastTwoMoth(email);

            if (orders.Count() == 0)
            {
                var infoMessage = T("No orders found during last 60 days");
                _notifier.Add(NotifyType.Information, infoMessage);
            }
            else
            {
                var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                _teeyootMessagingService.SendRecoverOrderMessage(pathToTemplates, orders.ToList(), email);
                var infoMessage =
                    T("We sent you an email containing your order number");
                _notifier.Add(NotifyType.Information, infoMessage);
            }
            return View("RecoverOrder");
        }

        public ActionResult CancelOrder(int orderId, string publicId)
        {
            try
            {
                var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                  Request.ApplicationPath.TrimEnd('/');
                _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, orderId, "Cancelled");
                
                var order = _orderService.GetOrderById(orderId);

                _orderService.DeleteOrder(orderId);

                _campaignService.UpdateCampaginSoldCount(order.Campaign.Id);

                return Redirect("/");
            }
            catch (Exception ex)
            {
                Logger.Error("Error occured when trying to delete an order ---------------> " + ex.ToString());
                return RedirectToAction("OrderTracking", new { orderId = publicId });
            }
        }


        [Themed]
        [HttpPost]
        public HttpStatusCodeResult ShareCampaign(bool isBack, int campaignId)
        {
            CampaignRecord campaign = _campaignService.GetCampaignById(campaignId);
            int product =
                _campaignService.GetProductsOfCampaign(campaignId).Where(pr => pr.WhenDeleted == null).First().Id;

            string destFolder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                product.ToString(), "social");
            var dir = new DirectoryInfo(destFolder);

            if (dir.Exists == false || ((dir.Exists == true) && (dir.GetFiles().Count() == 0)))
            {
                try
                {
                    Directory.CreateDirectory(destFolder);

                    var serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = int.MaxValue;
                    DesignInfo data = serializer.Deserialize<DesignInfo>(campaign.CampaignDesign.Data);

                    var p = campaign.Products.Where(pr => pr.WhenDeleted == null).First();

                    var imageFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/images/");
                    var rgba = ColorTranslator.FromHtml(p.ProductColorRecord.Value);

                    if (!isBack)
                    {
                        var frontPath = Path.Combine(imageFolder, "product_type_" + p.ProductRecord.Id + "_front.png");
                        var imgPath = new Bitmap(frontPath);

                        _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, data.Front);
                    }
                    else
                    {
                        var backPath = Path.Combine(imageFolder, "product_type_" + p.ProductRecord.Id + "_back.png");
                        var imgPath = new Bitmap(backPath);

                        _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, data.Back);
                    }
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpStatusCodeResult ReservCampaign(string email, int id)
        {
            var requests = _campaignService.GetReservedRequestsOfCampaign(id);

            foreach (var request in requests)
            {
                if (request.Email == email)
                {
                    Response.Write("Already");
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
            }

            _campaignService.ReservCampaign(id, email);

            if (requests.Count() == 10)
            {
                _teeyootMessagingService.SendReLaunchCampaignMessageToAdmin(id);
                _teeyootMessagingService.SendReLaunchCampaignMessageToSeller(id);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult ChangeCountryAndCulture(int countrId)
        {
            var cultures = _countryService.GetCultureByCountry(countrId);
            if (cultures == null || cultures.Count() == 0)
            {
                cultures = _countryService.GetCultureByCountry(_countryService.GetAllCountry().First().Id);
            }

            _cookieCultureService.SetCulture(cultures.First().Culture);

            return Redirect(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/");
        }

        private void SetOrderData(OrderRecord order,
            CountryRecord deliveryCountry,
            CurrencyRecord deliveryCountryCurrency,
            string paymentMethod,
            string state)
        {
            //order.CurrencyRecord = deliveryCountryCurrency;

            if (order.SellerCurrency != deliveryCountryCurrency)
            {
                //var currencyExchange = _currencyExchangeRepository.Table
                //    .First(e => e.CurrencyFrom == order.SellerCurrency &&
                //                e.CurrencyTo == deliveryCountryCurrency);

                //var exchangeRate = IsCurrentUserSeller
                //    ? currencyExchange.RateForSeller
                //    : currencyExchange.RateForBuyer;

                //order.TotalPrice *= exchangeRate;
                //order.ExchangeRate = exchangeRate;
            }

            if (order.SellerCountry == deliveryCountry)
            {
                var deliverySetting = _deliverySettingRepository.Table
                    .First(s => s.State == state);

                order.Delivery = (paymentMethod == "4")
                    ? deliverySetting.CodCost + ((order.TotalSold - 1) * deliverySetting.CodCost / 2)
                    : deliverySetting.PostageCost + ((order.TotalSold - 1) * deliverySetting.PostageCost / 2);

                order.Delivery = _priceconversationService.ConvertPrice(order.Delivery, "MYR", order.Campaign.CurrencyRecord).Value;
            }
            else
            {
                var deliverySetting = _deliveryInternationalSettingRepository.Table
                    .First(s => s.CountryTo == deliveryCountry);

                order.Delivery = deliverySetting.DeliveryPrice + ((order.TotalSold - 1) * deliverySetting.DeliveryPrice / 2);

                order.Delivery = _priceconversationService.ConvertPrice(order.Delivery, "MYR", order.Campaign.CurrencyRecord).Value;


            }
        }

        [Themed, OutputCache(NoStore = true, Duration = 0)]
        public ActionResult SetCurrency(string code)
        {


            var localizationStorageContainer = LocalizationInfoStorageContainerFactory.GetStorageContainer();
            localizationStorageContainer.StorCurrency(code);

            return Redirect(Request.UrlReferrer.ToString());

        }

        [ValidateAntiForgeryTokenOrchard(false)]
        [Themed]
        public ActionResult IPay88ResponseUrl(FormCollection frm)
        {
            var campaignId = 0;
            var sellerId = 0;
            var Id = 0;

            ViewBag.Error = frm["ErrDesc"];
            string orderNumber = frm["RefNo"];
            ViewBag.Error = frm["RefNo"];

            var status = frm["status"];

            if (status.Trim() == "1")
            {
                try
                {
                    var order = _orderService.GetOrderById(Convert.ToInt32(orderNumber)); // Retrieve the order using the order number we saved into the returnUrl
                    order.Paid = DateTime.Now;
                    var request = System.Web.HttpContext.Current.Request;
                    var amount = order.TotalPrice.ToString("0.00");
                    order.OrderStatusRecord = (_orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()));


                    order.Paid = DateTime.Now.ToUniversalTime();
                    order.ProfitPaid = true;
                    _orderService.UpdateOrder(order);

                    Id = order.Id;
                    campaignId = order.Campaign.Id;
                    sellerId = order.Seller.Id;


                    var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                    var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                      Request.ApplicationPath.TrimEnd('/');
                    _teeyootMessagingService.SendNewOrderMessageToAdmin(order.Id, pathToMedia, pathToTemplates);
                    _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, order.Id,
                        OrderStatus.Approved.ToString());

                    //try
                    //{
                    //    if (order.Campaign.ProductCountSold >= order.Campaign.ProductCountGoal)
                    //    {
                    //        order.Campaign.ProductCountSold += order.TotalSold; //order.Products.Sum(p => (int?)p.Count) ?? 0;
                    //        _campaignService.UpdateCampaign(order.Campaign);
                    //    }
                    //    else
                    //    {
                    //        order.Campaign.ProductCountSold += order.TotalSold;//order.Products.Sum(p => (int?)p.Count) ?? 0;
                    //        _campaignService.UpdateCampaign(order.Campaign);
                    //    }
                    //}
                    //catch (Exception ex2)
                    //{
                    //    ViewBag.Error = ex2.ToString();
                    //}
                    return RedirectToAction("ReservationComplete",
                           new { campaignId = order.Campaign.Id, sellerId = order.Campaign.TeeyootUserId, Id = order.Id });
                }
                catch (Exception ex)
                {
                    ViewBag.Error = ex.ToString();
                }
            }
            else
            {
                ViewBag.Error = frm["ErrDesc"] != null ? frm["ErrDesc"] : "Payment fail.";
                return View();
            }
            return View();
        }


        public ActionResult PaypalResponse(FormCollection frm)
        {
            return View();
        }

        [NonAction]
        private string IPay88Signature(string merchantKey, string MerchantCode, string RefNo, string Amount, string Currency)
        {
            var input = merchantKey + MerchantCode + RefNo + Amount.Replace(".", "").Replace(",", "") + Currency;
            SHA1CryptoServiceProvider objSHA1 = new SHA1CryptoServiceProvider();
            objSHA1.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input.ToCharArray()));
            var buffer = objSHA1.Hash;
            var HashValue = System.Convert.ToBase64String(buffer);
            return HashValue;
        }

        public ActionResult PurchaseUnsucessful()
        {
            return Redirect("/");
        }

        public ActionResult PurchaseComplete(string orderNumber, string payerId, string paymentId)
        {

            //paymentId=PAY-35142466VA430402CKZJISNQ&token=EC-4YB35801XR592810D&PayerID=3CQHG83SEA82E
            try
            {
                var order = _orderService.GetOrderByPublicId(orderNumber); // Retrieve the order using the order number we saved into the returnUrl
                order.Paid = DateTime.Now;
                var request = System.Web.HttpContext.Current.Request;
                this._payPal.ConfirmPayment(paymentId, payerId, order);

                var campaign =
                    _campaignService.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id);

                if (order.OrderStatusRecord != _orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()))
                {
                    var amount = order.TotalPrice.ToString("0.00");
                    order.OrderStatusRecord = (_orderStatusRepository.Table.First(s => s.Name == OrderStatus.Approved.ToString()));
                    order.Paid = DateTime.Now.ToUniversalTime();
                    order.ProfitPaid = true;
                    //ayPal.ConfirmPayment(paymentId, payerId, order);
                    _orderService.UpdateOrder(order);


                    var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                    var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                      Request.ApplicationPath.TrimEnd('/');

                    _teeyootMessagingService.SendNewOrderMessageToAdmin(order.Id, pathToMedia, pathToTemplates);
                    _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, order.Id,
                        OrderStatus.Approved.ToString());

                    //if (campaign.ProductCountSold >= campaign.ProductCountGoal)
                    //{
                    //    campaign.ProductCountSold += order.TotalSold; //order.Products.Sum(p => (int?)p.Count) ?? 0;
                    //    _campaignService.UpdateCampaign(campaign);
                    //}
                    //else
                    //{
                    //    campaign.ProductCountSold += order.TotalSold;//order.Products.Sum(p => (int?)p.Count) ?? 0;
                    //    _campaignService.UpdateCampaign(campaign);
                    //    //TYT-78 Code Commented
                    //    //if (campaign.ProductCountSold >= campaign.ProductMinimumGoal)
                    //    //{
                    //    //    _teeyootMessagingService.SendCampaignMetMinimumMessageToBuyers(campaign.Id);
                    //    //    _teeyootMessagingService.SendCampaignMetMinimumMessageToSeller(campaign.Id);
                    //    //}
                    //    //End TYT-78
                    //}
                    return RedirectToAction("ReservationComplete",
                           new { campaignId = campaign.Id, sellerId = campaign.TeeyootUserId, Id = order.Id });
                }
            }
            catch (PayPalException ex)
            {
                this.ViewBag.Error = ex.Message;
            }
            catch (Exception ex)
            {
                this.ViewBag.Error = ex.Message;
            }

            return this.RedirectToAction("Index");

        }


        public ActionResult IPay88RedirectPage(int OrderID, int settingID)
        {

            var baseUrl = string.Concat(this.Request.Url.Scheme, "://", this.Request.Url.Authority);

            // var payment = this._payPal.CreatePayment(order, string.Concat(baseUrl, cancelUrl), string.Concat(baseUrl, returnUrl));


            var rec = _orderService.GetOrderById(OrderID);
            var setting = _paymentSettingsService.GetAllSettigns().FirstOrDefault(aa => aa.Id == settingID);

            var totalPrice = (rec.TotalPriceWithPromo != 0) ? rec.TotalPriceWithPromo : rec.TotalPrice;
            totalPrice += rec.Delivery;

            var currency = string.IsNullOrWhiteSpace(rec.selectedCurrency) ? rec.CurrencyRecord.Code : rec.selectedCurrency;



            totalPrice =
                _priceconversationService.ConvertPrice(((rec.TotalPriceWithPromo > 0 ? rec.TotalPriceWithPromo : rec.TotalPrice) + rec.Delivery),
                rec.Campaign.CurrencyRecord, currency).Value;

            return View("IPay88RedirectPage", new Ipay88ViewModel()
            {
                MerchantCode = setting.Ipay88MerchantCode,
                PaymentId = 16,
                RefNo = rec.Id.ToString(),
                Amount = totalPrice.ToString("0.00").Replace(",", "."), //rec.TotalPrice,
                Currency = currency, //string.IsNullOrWhiteSpace(rec.selectedCurrency) ? rec.CurrencyRecord.Code : rec.selectedCurrency,
                ProdDesc = rec.Campaign.Alias + "(" + rec.Campaign.Id + ")",
                UserName = rec.FirstName + " " + rec.LastName,
                UserEmail = rec.Email,
                UserContact = rec.StreetAddress,
                Remark = "Buyer:" + rec.FirstName + " " + rec.LastName + " (" + rec.Email + ")" + ", Seller: " + rec.Seller.Email + "(" + rec.Seller.Id + "), Phone Number: (" + rec.PhoneNumber + ")",
                Lang = "UTF-8",
                Signature = IPay88Signature(setting.Ipay88MerchantKey, setting.Ipay88MerchantCode, rec.Id.ToString(), totalPrice.ToString("0.00").Replace(",", "."), currency),
                ResponseURL = string.Concat(baseUrl, this.Url.Action("IPay88ResponseUrl", "Home")),
                BackendURL = string.Concat(baseUrl, this.Url.Action("IPay88ResponseUrl", "Home"))
            });
        }

        public JsonResult ShowMyCountry()
        {
            return Json(new
            {
                country_code = _multiCountryService.GetCountry().Name,
                IP = System.Web.HttpContext.Current.Request.UserHostAddress,
                Code = _multiCountryService.GetCountryCode()
            }, JsonRequestBehavior.AllowGet);
        }

    }

    public class TwoDecimalFormatProvider : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }
            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg == null || arg.GetType() != typeof(double))
            {
                try
                {
                    return HandleOtherFormats(format, arg);
                }
                catch (FormatException e)
                {
                    throw new FormatException(string.Format("The format of '{0}' is invalid.", format));
                }
            }

            if (format.StartsWith("T"))
            {
                int dp = 2;
                int idx = 1;
                if (format.Length > 1)
                {
                    if (format[1] == '(')
                    {
                        int closeIdx = format.IndexOf(')');
                        if (closeIdx > 0)
                        {
                            if (int.TryParse(format.Substring(2, closeIdx - 2), out dp))
                            {
                                idx = closeIdx + 1;
                            }
                        }
                        else
                        {
                            throw new FormatException(string.Format("The format of '{0}' is invalid.", format));
                        }
                    }
                }
                double mult = Math.Pow(10, dp);
                arg = Math.Truncate((double)arg * mult) / mult;
                format = format.Substring(idx);
            }

            try
            {
                return HandleOtherFormats(format, arg);
            }
            catch (FormatException e)
            {
                throw new FormatException(string.Format("The format of '{0}' is invalid.", format));
            }
        }

        private string HandleOtherFormats(string format, object arg)
        {
            if (arg is IFormattable)
            {
                return ((IFormattable)arg).ToString(format, CultureInfo.CurrentCulture);
            }
            return arg != null ? arg.ToString() : String.Empty;
        }

    }
}
