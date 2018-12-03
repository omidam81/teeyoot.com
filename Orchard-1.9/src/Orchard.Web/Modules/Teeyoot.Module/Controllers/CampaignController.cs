using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Notify;
using RM.Localization.Services;
using Teeyoot.Localization;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;
using Newtonsoft.Json.Serialization;

namespace Teeyoot.Module.Controllers
{
    [Themed]
    public class CampaignController : Controller
    {
        public IOrchardServices Services { get; set; }
        private readonly ICampaignService _campaignService;
        private readonly IPromotionService _promotionService;
        private readonly IWorkContextAccessor _wca;
        private readonly IProductService _productService;
        private readonly ITShirtCostService _tshirtService;
        private readonly INotifier _notifier;
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        public Localizer T { get; set; }
        private readonly ICookieCultureService _cookieCultureService;
        private readonly string _cultureUsed;
        private readonly ICountryService _countryService;
        private readonly IPriceConversionService _priceConversionService;
        private readonly IOrderService _orderService;
        private readonly IRepository<TeeyootUserPartRecord> _userRepository;
        private readonly IMultiCountryService _imulticountryservice;
        private readonly IRepository<CurrencyExchangeRecord> _currencyExchangeRepository;

        private readonly IMultiCountryService _multiCountryService;


    
        public CampaignController(
            ICampaignService campaignService,
            ITShirtCostService tshirtService,
            IProductService productService,
            IPromotionService promotionService,
            IRepository<CurrencyRecord> currencyRepository,
            IWorkContextAccessor wca,
            INotifier notifier,
            IOrchardServices services,
            ICookieCultureService cookieCultureService,
            ICountryService countryService,
            IPriceConversionService priceConversionService,
            IOrderService orderService,
            IRepository<TeeyootUserPartRecord> userRepository,
            IMultiCountryService imulticountryservice,
            IRepository<CurrencyExchangeRecord> currencyExchangeRepository,
            IMultiCountryService multiCountryService
            )
        {
            _currencyRepository = currencyRepository;
            Services = services;
            _tshirtService = tshirtService;
            _productService = productService;
            _campaignService = campaignService;
            _promotionService = promotionService;
            _wca = wca;
            _notifier = notifier;
            Logger = NullLogger.Instance;

            _cookieCultureService = cookieCultureService;
            //var culture = _wca.GetContext().CurrentCulture.Trim();
            _cultureUsed = _wca.GetContext().CurrentCulture.Trim();
            //cultureUsed = culture == "en-SG" ? "en-SG" : (culture == "id-ID" ? "id-ID" : "en-MY");
            _countryService = countryService;
            _priceConversionService = priceConversionService;
            _orderService = orderService;
            _userRepository = userRepository;
            _imulticountryservice = imulticountryservice;
            _currencyExchangeRepository = currencyExchangeRepository;
            _multiCountryService = multiCountryService;
        }

        public ILogger Logger { get; set; }

        //
        // GET: /Campaign/
        public ActionResult Index(string campaignName, string promo)
        {
            if (string.IsNullOrWhiteSpace(campaignName))
            {
                return View("NotFound", Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : "");
            }

            var campaign = _campaignService.GetCampaignByAlias(campaignName);

            if (campaign == null)
            {
                return View("NotFound", Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : "");
            }

            var user = _wca.GetContext().CurrentUser;
            var teeyootUserId = -1;

            if (user != null)
            {
                teeyootUserId = user.ContentItem.Get(typeof (TeeyootUserPart)).Id;
            }

            if (!campaign.IsApproved &&
                !Services.Authorizer.Authorize(Permissions.ApproveCampaigns) &&
                teeyootUserId != campaign.TeeyootUserId)
            {
                return View("NotFound", Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : "");
            }

            //TODO: (auth:keinlekan) Удалить код, если больше не пригодиться. Переход сайта на культуру компании
            //string campaignCulture = campaign.CampaignCulture;
            //if (campaignCulture != cultureUsed)
            //{
            //    _cookieCultureService.SetCulture(campaignCulture);
            //}

            if ((Services.Authorizer.Authorize(Permissions.ApproveCampaigns) || teeyootUserId == campaign.TeeyootUserId) &&
                campaign.Rejected)
            {
                var infoMessage = T("Your campaign have been rejected!");
                _notifier.Add(NotifyType.Information, infoMessage);
            }
            else
            {
                if ((Services.Authorizer.Authorize(Permissions.ApproveCampaigns) ||
                     teeyootUserId == campaign.TeeyootUserId) && campaign.IsApproved == false)
                {
                    var infoMessage =
                        T(
                            "Your campaign is awaiting approval. This should take less than 1 hour during office hours.");
                    _notifier.Add(NotifyType.Information, infoMessage);
                }
            }

            var model = new CampaignIndexViewModel(_priceConversionService)
            {
                Campaign = campaign
            };

            model.FBDescription = model.Campaign.Description;
            model.FBDescription = Regex.Replace(model.FBDescription, @"<br>", " ").Trim();
            model.FBDescription = Regex.Replace(model.FBDescription, @"<[^>]+>", "").Trim();
            model.FBDescription = Regex.Replace(model.FBDescription, @"&nbsp;", " ").Trim();


            //Add User Pixel
            var ____user = _userRepository.Get(campaign.Seller.Id);
            if (____user != null)
            {

                model.SellerFbPixel = !string.IsNullOrWhiteSpace(campaign.FBPixelId) ? campaign.FBPixelId : ____user.DefaultFBPixelId;
                model.FacebookCustomAudiencePixel = !string.IsNullOrWhiteSpace(campaign.FBPixelId) ? campaign.FBPixelId : ____user.DefaultFacebookCustomAudiencePixel;
            }
            else
            {
                model.SellerFbPixel = campaign.FBPixelId;
            }


            model.Prices = new Dictionary<int, Dictionary<string, double>>();
            
            foreach (var p in campaign.Products)
            {
                Dictionary<string, double> productPrices = new Dictionary<string, double>();
                foreach (var c in _currencyRepository.Table)
                {
                    productPrices.Add(c.Code, _priceConversionService.ConvertPrice(p.Price, campaign.CurrencyRecord, c).Value);
                }
                model.Prices.Add(p.Id, productPrices);

            }
            model.currency = _imulticountryservice.GetCurrency().Code;


            model.exchangeRate = Newtonsoft.Json.JsonConvert.SerializeObject(_currencyExchangeRepository.Table.Select(aa => new { From= aa.CurrencyFrom.Code, To=aa.CurrencyTo.Code, RateForSeller=aa.RateForSeller, RateForBuyer = aa.RateForBuyer}));

                                                    


            if (campaign.ProductCountSold >= campaign.ProductMinimumGoal && campaign.IsActive)
            {
                var infoMessage =
                    T(
                        "Yippee! The minimum order for this campaign is {0}, but we have already sold {1}. The item will definitely go to print once the campaign ends.",
                        campaign.ProductMinimumGoal, campaign.ProductCountSold);
                _notifier.Add(NotifyType.Information, infoMessage);
            }
            //if (campaign.IsApproved && campaign.ProductCountSold < campaign.ProductMinimumGoal &&
            //    campaign.IsActive)
            //{
            //    var infoMessage = T(
            //        string.Format(
            //            "{0} orders have been made. We need {1} more for this campaign to proceed.",
            //            campaign.ProductCountSold,
            //            campaign.ProductMinimumGoal - campaign.ProductCountSold));
            //    _notifier.Add(NotifyType.Information, infoMessage);
            //}
            if (!campaign.IsActive && campaign.IsApproved && !campaign.IsArchived)
            {
                var cntRequests = _campaignService.GetCountOfReservedRequestsOfCampaign(campaign.Id);
                model.CntRequests = 10 - (cntRequests >= 10 ? 10 : cntRequests);
                if (cntRequests >= 10)
                {
                    var infoMessage = T("This campaign is likely to be re-activated soon.");
                    _notifier.Add(NotifyType.Information, infoMessage);
                }
                else
                {
                    var infoMessage = T(
                        string.Format("Only {0} more requests for the campaign to be re-activated",
                            10 - (cntRequests >= 10 ? 10 : cntRequests)));
                    _notifier.Add(NotifyType.Information, infoMessage);
                }
            }

            if (promo == null)
            {
                return View("Index2", model);
            }

            try
            {
                var promotion = _promotionService.GetPromotionByPromoId(promo);

                var localizationInfo = _multiCountryService.GetCountry(); //LocalizationInfoFactory.GetCurrentLocalizationInfo();
                var currency = _multiCountryService.GetCurrency(); //_countryService.GetCurrency(localizationInfo, LocalizationInfoFactory.GetCurrency());

                if (promotion.Status &&
                    promotion.Expiration > DateTime.UtcNow &&
                    promotion.UserId == campaign.TeeyootUserId 
                    /*campaign.ProductCountSold >= campaign.ProductMinimumGoal*/)
                {
                    if (promotion.AmountType == "%")
                    {
                        FillViewModelWithPromo(model, promotion);
                    }
                    else
                    {
                        var promotionCurrency = _currencyRepository.Table
                            .First(c => c.Code == promotion.AmountType);

                        if (promotionCurrency == currency)
                        {
                            FillViewModelWithPromo(model, promotion);
                        }
                        else
                        {
                            var infoMessage = T(
                                "Oh no! The requested promotion is currently not available for this campaign. But you can still buy at the normal price!");
                            _notifier.Add(NotifyType.Information, infoMessage);
                        }
                    }
                }
                else
                {
                    var infoMessage = T(
                        "Oh no! The requested promotion is currently not available for this campaign. But you can still buy at the normal price!");
                    _notifier.Add(NotifyType.Information, infoMessage);
                }
                
                //foreach (var product in campaign.Products)
                //{
                //    model.Prices.Add()
                //}
                return View("Index2", model);
            }
            catch (Exception)
            {

                var infoMessage = T(
                    "Oh no! The requested promotion is currently not available for this campaign. But you can still buy at the normal price!");
                _notifier.Add(NotifyType.Information, infoMessage);

                return View("Index2", model);
            }
        }

        public ActionResult Index2(string campaignName, string promo)
        {

            ViewBag.CampaingName = campaignName;
            ViewBag.Promo = promo;

            return View();
        }
        [HttpGet]
        public JsonResult GET(int id)
        {
            var c = _campaignService.GetCampaignById(id);
            return Json(new
            {
                Title = c.Title, 

            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult GetDataForReLaunch(string campaignName)
        {
            var campaign = _campaignService.GetCampaignByAlias(campaignName);
            var products = _campaignService.GetProductsOfCampaign(campaign.Id);
            var result = new RelaunchViewModel();
            var prodInfo = new List<object>();
            foreach (var product in products)
            {
                var prodRec = _productService.GetProductById(product.ProductRecord.Id);
                prodInfo.Add(new
                {
                    Price = product.Price,
                    BaseCostForProduct = prodRec.BaseCost,
                    ProductId = prodRec.Id,
                    BaseCost = product.BaseCost
                });
            }

            var tShirtCostRecord = _tshirtService.GetCost(_cultureUsed);

            result.Products = prodInfo.ToArray();
            result.CntBackColor = campaign.CntBackColor;
            result.CntFrontColor = campaign.CntFrontColor;
            result.TShirtCostRecord = tShirtCostRecord;
            result.ProductCountGoal = campaign.ProductCountSold;

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private static void FillViewModelWithPromo(CampaignIndexViewModel viewModel, PromotionRecord promotion)
        {
            viewModel.PromoId = promotion.PromoId;
            viewModel.PromoSize = promotion.AmountSize;
            viewModel.PromoType = promotion.AmountType;
        }
    }

    public class NHibernateContractResolver : DefaultContractResolver
    {
        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(NHibernate.Proxy.INHibernateProxy).IsAssignableFrom(objectType))
                return base.CreateContract(objectType.BaseType);
            else
                return base.CreateContract(objectType);
        }
    }


}
