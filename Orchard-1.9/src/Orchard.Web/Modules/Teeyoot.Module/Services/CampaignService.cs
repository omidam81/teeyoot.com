using Braintree;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Server;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Messaging.CampaignService;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Localization;
using Orchard.Users.Models;

namespace Teeyoot.Module.Services
{
    public class CampaignService : ICampaignService
    {
        private readonly IRepository<CampaignRecord> _campaignRepository;
        private readonly IRepository<CampaignProductRecord> _campProdRepository;
        private readonly IRepository<ProductColorRecord> _colorRepository;
        private readonly IRepository<ProductRecord> _productRepository;
        private readonly IRepository<CampaignStatusRecord> _statusRepository;
        private readonly IRepository<CampaignCategoriesRecord> _campaignCategories;
        private readonly IRepository<LinkCampaignAndCategoriesRecord> _linkCampaignAndCategories;
        private readonly IRepository<LinkOrderCampaignProductRecord> _ocpRepository;
        private readonly IRepository<OrderRecord> _orderRepository;
        private readonly IRepository<OrderHistoryRecord> _orderHistoryRepository;
        private readonly ITeeyootMessagingService _teeyootMessagingService;
        private readonly IRepository<BringBackCampaignRecord> _backCampaignRepository;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly ICountryService _countryService;
        private readonly ShellSettings _shellSettings;
        private readonly IRepository<CampaignDesign> _campaignDesign;
        private readonly ITShirtCostService _costService;
        private readonly IRepository<PayoutRecord> _payoutRespository;
        private readonly IRepository<UserPartRecord> _userPartRepository;
        private readonly ITShirtCostService _tshirtCostService;
        private readonly IRepository<OrderStatusRecord> _orderStatusRepository;
        private readonly IRepository<CurrencyRecord> _currencyRepository;


        //private readonly ICampaignService _campaignService;



        public CampaignService(
            IRepository<CampaignRecord> campaignRepository,
            IRepository<CampaignProductRecord> campProdRepository,
            IRepository<ProductColorRecord> colorRepository,
            IRepository<ProductRecord> productRepository,
            IRepository<CampaignStatusRecord> statusRepository,
            IRepository<CampaignCategoriesRecord> campaignCategories,
            IOrchardServices services,
            IRepository<LinkCampaignAndCategoriesRecord> linkCampaignAndCategories,
            IRepository<LinkOrderCampaignProductRecord> ocpRepository,
            IRepository<OrderRecord> orderRepository,
            IRepository<OrderHistoryRecord> orderHistoryRepository,
            ITeeyootMessagingService teeyootMessagingService,
            IRepository<BringBackCampaignRecord> backCampaignRepository,
            IWorkContextAccessor workContextAccessor,
            ICountryService countryService,
            ShellSettings shellSettings,
            IRepository<CampaignDesign> campaignDesign,
            ITShirtCostService costService,
            IRepository<PayoutRecord> payoutRespository,
            IRepository<UserPartRecord> userPartRepository,
            ITShirtCostService tshirtCostService,
            IRepository<OrderStatusRecord> orderStatusRepository,
            IRepository<CurrencyRecord> currencyRepository
            )
        {
            _campaignRepository = campaignRepository;
            _campProdRepository = campProdRepository;
            _colorRepository = colorRepository;
            _productRepository = productRepository;
            _statusRepository = statusRepository;
            _campaignCategories = campaignCategories;
            Services = services;
            _linkCampaignAndCategories = linkCampaignAndCategories;
            _ocpRepository = ocpRepository;
            _orderRepository = orderRepository;
            _orderHistoryRepository = orderHistoryRepository;
            _teeyootMessagingService = teeyootMessagingService;
            _backCampaignRepository = backCampaignRepository;
            _campaignDesign = campaignDesign;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _workContextAccessor = workContextAccessor;
            _countryService = countryService;
            _shellSettings = shellSettings;
            _costService = costService;
            _payoutRespository = payoutRespository;
            _userPartRepository = userPartRepository;
            _tshirtCostService = tshirtCostService;
            _orderStatusRepository = orderStatusRepository;
            _currencyRepository = currencyRepository;
            //campaignService = _campaignService;

        }

        private IOrchardServices Services { get; set; }

        public Localizer T { get; set; }

        public ILogger Logger { get; set; }


        public BraintreeGateway Gateway = new BraintreeGateway
        {
            Environment = Braintree.Environment.SANDBOX,
            PublicKey = "ny4y8s7fkcvnfw9t",
            PrivateKey = "1532863effa7197329266f7de4837bae",
            MerchantId = "7qw5pmrj3hqd2hr4"
        };

        public IQueryable<CampaignCategoriesRecord> GetAllCategories()
        {
            return _campaignCategories.Table;
        }

        public IQueryable<CampaignRecord> GetAllCampaigns()
        {
            return _campaignRepository.Table.Where(c => c.WhenDeleted == null);
        }

        public CampaignRecord GetCampaignByAlias(string alias)
        {
            var x = _campaignRepository.Fetch(c => c.WhenDeleted == null && c.Alias == alias);
            return x.FirstOrDefault();
        }

        public CampaignRecord GetCampaignById(int id)
        {
            return _campaignRepository.Get(id);
        }

        public int GetArchivedCampaignsCnt(int id)
        {
            return _campaignRepository.Table.Count(c => c.BaseCampaignId == id);
        }

        public List<CampaignRecord> GetCampaignsForTheFilter(string filter, int skip = 0, int take = 16,
            bool tag = false)
        {
            if (tag)
            {
                //var camp = _campaignCategories.Table.Where(c => c.Name.ToLower() == filter).SelectMany(c => c.Campaigns.Select(x => x.CampaignRecord)).OrderByDescending(c => c.ProductCountSold).OrderBy(c => c.Title).Distinct();
                var categCamp = _campaignCategories.Table.Where(c => c.Name.ToLower() == filter).Select(c => c.Id);
                var campForTags =
                    _linkCampaignAndCategories.Table.Where(c => categCamp.Contains(c.CampaignCategoriesPartRecord.Id))
                        .Select(c => c.CampaignRecord)
                        .Where(c => c.WhenDeleted == null && !c.IsPrivate && c.IsActive && c.IsApproved)
                        .OrderByDescending(c => c.ProductCountSold)
                        .OrderBy(c => c.Title)
                        .Distinct();
                return campForTags.Skip(skip).Take(take).ToList();
            }
            else
            {
                var categCamp = _campaignCategories.Table.Where(c => c.Name.ToLower().Contains(filter))
                    .Select(c => c.Id);
                var campForTags =
                    _linkCampaignAndCategories.Table.Where(c => categCamp.Contains(c.CampaignCategoriesPartRecord.Id))
                        .Select(c => c.CampaignRecord)
                        .Where(c => c.WhenDeleted == null && !c.IsPrivate && c.IsActive && c.IsApproved);
                //List<CampaignRecord> campForTags = _campaignCategories.Table.Where(c => c.Name.ToLower().Contains(filter)).SelectMany(c => c.Campaigns.Select(x => x.CampaignRecord)).ToList();
                IEnumerable<CampaignRecord> camps =
                    GetAllCampaigns()
                        .Where(c => !c.IsPrivate && c.IsActive && c.IsApproved)
                        .Where(c => c.Title.Contains(filter) || c.Description.Contains(filter));
                camps =
                    camps.Concat(campForTags)
                        .OrderByDescending(c => c.ProductCountSold)
                        .OrderBy(c => c.Title)
                        .Distinct();
                //return camps.Skip(skip).Take(take);
                return camps.Skip(skip).Take(take).ToList();
            }
        }

        public CampaignRecord CreateNewCampiagn(LaunchCampaignData data)
        {
            CurrencyRecord currencyRecod = null;

            if (data.Currency.HasValue)
            {
                currencyRecod = _currencyRepository.Table.FirstOrDefault(aa => aa.Id == data.Currency.Value);
            }
            else
            {
                currencyRecod = _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD");
            }
            if (currencyRecod == null) _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD");


            var user = Services.WorkContext.CurrentUser;
            var teeyootUser = user.ContentItem.Get(typeof (TeeyootUserPart));
            int? userId = null;

            if (teeyootUser != null)
            {
                userId = teeyootUser.ContentItem.Record.Id;
            }

            try
            {
                var culture = _workContextAccessor.GetContext().CurrentCulture.Trim();
                var cultureUsed = culture == null ? "en-MY" : culture;
                CampaignDesign D = new CampaignDesign()
                {
                    Data = data.Design
                };

                _campaignDesign.Create(D);


                var newCampaign = new CampaignRecord
                {
                    Alias = data.Alias,
                    BackSideByDefault = data.BackSideByDefault,
                    Description = data.Description,
                    CampaignDesign = D,
                    EndDate = DateTime.UtcNow.Add(new TimeSpan(data.CampaignLength, 0, 0, 0, 0)), //.AddDays(data.CampaignLength),
                    IsForCharity = data.IsForCharity,
                    StartDate = DateTime.UtcNow,
                    ProductCountGoal = data.ProductCountGoal,
                    Tareget = data.target,
                    TargetSet = data.targetSet,
                    ProductCountSold = 0,
                    TeeyootUserId = userId,
                    Title = data.CampaignTitle,
                    IsActive = true,
                    IsApproved = false,
                    CampaignStatusRecord = _statusRepository.Table
                        .First(s => s.Name == CampaignStatus.Unpaid.ToString()),
                    CampaignProfit = data.CampaignProfit ?? string.Empty,
                    ProductMinimumGoal = data.ProductMinimumGoal == 0 ? 1 : data.ProductMinimumGoal,
                    CampaignCulture = string.IsNullOrEmpty(data.CampaignCulture) ? "en-MY" : data.CampaignCulture.Trim(),
                    //TODO: (auth:keinlekan) Удалить код после удаления поля из таблицы/модели
                    CntBackColor = data.CntBackColor,
                    CntFrontColor = data.CntFrontColor,
                    //CountryRecord = _countryService.GetCountryByCulture(_workContextAccessor.GetContext().CurrentCulture.Trim()),
                    CountryRecord = user.ContentItem.As<TeeyootUserPart>().CountryRecord,
                    CurrencyRecord = currencyRecod, //user.ContentItem.As<TeeyootUserPart>().CurrencyRecord,
                    FBPixelId = user.ContentItem.As<TeeyootUserPart>().DefaultFBPixelId,
                    GooglePixelId = user.ContentItem.As<TeeyootUserPart>().DefaultGooglePixelId,
                    PinterestPixelId = user.ContentItem.As<TeeyootUserPart>().DefaultPinterestPixelId,
                    Seller = _userPartRepository.Get(userId.Value),
                    TShirtCostRecord = _tshirtCostService.GetCost(cultureUsed)
                };

                _campaignRepository.Create(newCampaign);

                //TODO: (auth:keinlekan) Удалить данный код после локализации
                var currencyId =
                    _countryService.GetCurrencyByCulture(_workContextAccessor.GetContext().CurrentCulture.Trim(), LocalizationInfoFactory.GetCurrency());
                //_currencyRepository.Table.Where(c => c.CurrencyCulture == cultureUsed).First();

                if (data.Tags != null)
                {
                    foreach (var tag in data.Tags)
                    {
                        if (_campaignCategories.Table.FirstOrDefault(c => c.Name.ToLower() == tag) != null)
                        {
                            var cat = _campaignCategories.Table.FirstOrDefault(c => c.Name.ToLower() == tag);
                            var link = new LinkCampaignAndCategoriesRecord
                            {
                                CampaignRecord = newCampaign,
                                CampaignCategoriesPartRecord = cat
                            };
                            _linkCampaignAndCategories.Create(link);
                        }
                        else
                        {
                            var cat = new CampaignCategoriesRecord
                            {
                                Name = tag,
                                IsVisible = false,
                                CategoriesCulture = cultureUsed,
                                CountryRecord =
                                    _countryService.GetCountryByCulture(
                                        _workContextAccessor.GetContext().CurrentCulture.Trim())
                            };
                            _campaignCategories.Create(cat);
                            var link = new LinkCampaignAndCategoriesRecord
                            {
                                CampaignRecord = newCampaign,
                                CampaignCategoriesPartRecord = cat
                            };
                            _linkCampaignAndCategories.Create(link);
                        }
                    }
                }

                foreach (var prod in data.Products)
                {
                    double baseCost;
                    if (!double.TryParse(prod.BaseCost, out baseCost))
                    {
                        double.TryParse(prod.BaseCost.Replace('.', ','), out baseCost);
                    }

                    double price;
                    if (!double.TryParse(prod.Price, out price))
                    {
                        double.TryParse(prod.Price.Replace('.', ','), out price);
                    }

                    var campProduct = new CampaignProductRecord
                    {
                        CampaignRecord_Id = newCampaign.Id,
                        BaseCost = baseCost,
                        CurrencyRecord = currencyId,
                        Price = price,
                        CostOfMaterial = _productRepository.Get(prod.ProductId).BaseCost,
                        ProductColorRecord = _colorRepository.Get(prod.ColorId),
                        ProductRecord = _productRepository.Get(prod.ProductId),
                        SecondProductColorRecord =
                            prod.SecondColorId == 0 ? null : _colorRepository.Get(prod.SecondColorId),
                        ThirdProductColorRecord =
                            prod.ThirdColorId == 0 ? null : _colorRepository.Get(prod.ThirdColorId),
                        FourthProductColorRecord =
                            prod.FourthColorId == 0 ? null : _colorRepository.Get(prod.FourthColorId),
                        FifthProductColorRecord =
                            prod.FifthColorId == 0 ? null : _colorRepository.Get(prod.FifthColorId)
                    };

                    _campProdRepository.Create(campProduct);

                    newCampaign.Products.Add(campProduct);
                }

                return newCampaign;
            }
            catch
            {
                throw;
            }
        }

        public CampaignRecord ReLaunchCampiagn(int productCountGoal, string campaignProfit, int campaignLength,
            int minimum, RelaunchProductInfo[] baseCost, int id)
        {
            var campaign = GetCampaignById(id);
            var alias = campaign.Alias;
            campaign.IsArchived = true;

            int campId = 0;
            int.TryParse(campaign.BaseCampaignId.ToString(), out campId);
            if (campId != 0)
            {
                var numberArchive = GetArchivedCampaignsCnt(campId);
                campaign.Alias = alias + "_archive_" + (numberArchive + 1);
            }
            else
            {
                campaign.Alias = alias + "_archive_1";
            }

            var user = Services.WorkContext.CurrentUser;
            var teeyootUser = user.ContentItem.Get(typeof (TeeyootUserPart));
            int? userId = null;

            if (teeyootUser != null)
            {
                userId = teeyootUser.ContentItem.Record.Id;
            }

            try
            {

                CampaignDesign D = new CampaignDesign()
                {
                    Data = campaign.CampaignDesign.Data
                };

                _campaignDesign.Create(D);

                var newCampaign = new CampaignRecord
                {
                    Alias = alias,
                    BackSideByDefault = campaign.BackSideByDefault,
                    Description = campaign.Description,
                    CampaignDesign = D, //campaign.Design,
                    EndDate = DateTime.UtcNow.AddDays(campaignLength),
                    IsForCharity = campaign.IsForCharity,
                    StartDate = DateTime.UtcNow,
                    ProductCountGoal = productCountGoal,
                    ProductCountSold = 0,
                    TeeyootUserId = userId,
                    Title = campaign.Title,
                    IsActive = true,
                    IsApproved = false,
                    CampaignStatusRecord =
                        _statusRepository.Table.First(s => s.Name == CampaignStatus.Unpaid.ToString()),
                    CampaignProfit = campaignProfit ?? string.Empty,
                    ProductMinimumGoal = minimum == 0 ? 1 : minimum,
                    CntBackColor = campaign.CntBackColor,
                    CntFrontColor = campaign.CntFrontColor,
                    BaseCampaignId = campaign.BaseCampaignId ?? campaign.Id,
                    CampaignCulture = campaign.CampaignCulture
                };
                _campaignRepository.Create(newCampaign);

                var tags = _linkCampaignAndCategories.Table.Where(c => c.CampaignRecord.Id == id);

                if (tags != null)
                {
                    foreach (var tag in tags)
                    {
                        if (
                            _campaignCategories.Table.FirstOrDefault(
                                c => c.Name.ToLower() == tag.CampaignCategoriesPartRecord.Name) != null)
                        {
                            var cat =
                                _campaignCategories.Table.FirstOrDefault(
                                    c => c.Name.ToLower() == tag.CampaignCategoriesPartRecord.Name);
                            var link = new LinkCampaignAndCategoriesRecord
                            {
                                CampaignRecord = newCampaign,
                                CampaignCategoriesPartRecord = cat
                            };
                            _linkCampaignAndCategories.Create(link);
                        }
                        else
                        {
                            var cat = new CampaignCategoriesRecord
                            {
                                Name = tag.CampaignCategoriesPartRecord.Name,
                                IsVisible = false
                            };
                            _campaignCategories.Create(cat);
                            var link = new LinkCampaignAndCategoriesRecord
                            {
                                CampaignRecord = newCampaign,
                                CampaignCategoriesPartRecord = cat
                            };
                            _linkCampaignAndCategories.Create(link);
                        }
                    }
                }

                foreach (var prod in campaign.Products)
                {
                    double newBaseCost = 0;

                    foreach (var newProd in baseCost)
                    {
                        if (newProd.Id == prod.ProductRecord.Id)
                        {
                            double.TryParse(newProd.BaseCost.Replace('.', ','), out newBaseCost);
                        }
                    }

                    var campProduct = new CampaignProductRecord
                    {
                        CampaignRecord_Id = newCampaign.Id,
                        BaseCost = newBaseCost,
                        CurrencyRecord = prod.CurrencyRecord,
                        Price = prod.Price,
                        ProductColorRecord = prod.ProductColorRecord,
                        ProductRecord = prod.ProductRecord,
                        WhenDeleted = prod.WhenDeleted,
                        SecondProductColorRecord = prod.SecondProductColorRecord,
                        ThirdProductColorRecord = prod.ThirdProductColorRecord,
                        FourthProductColorRecord = prod.FourthProductColorRecord,
                        FifthProductColorRecord = prod.FifthProductColorRecord
                    };

                    _campProdRepository.Create(campProduct);

                    newCampaign.Products.Add(campProduct);
                }

                return newCampaign;
            }
            catch
            {
                throw;
            }
        }

        public void UpdateCampaign(CampaignRecord campiagn)
        {
            _campaignRepository.Update(campiagn);
        }

        public CampaignProductRecord GetCampaignProductById(int id)
        {
            return _campProdRepository.Get(id);
        }

        public IQueryable<CampaignProductRecord> GetProductsOfCampaign(int campaignId)
        {
            return _campProdRepository.Table.Where(p => p.CampaignRecord_Id == campaignId).OrderBy(p => p.Id);
        }

        public IQueryable<CampaignRecord> GetCampaignsOfUser(int userId)
        {
            return userId > 0
                ? GetAllCampaigns().Where(c => c.TeeyootUserId == userId && c.WhenDeleted == null)
                : GetAllCampaigns().Where(c => !c.TeeyootUserId.HasValue);
        }

        public bool DeleteCampaignFromCategoryById(int campId, int categId)
        {
            var camp = GetCampaignById(campId);
            try
            {
                foreach (var link in camp.Categories)
                {
                    if (link.CampaignCategoriesPartRecord.Id == categId)
                    {
                        _linkCampaignAndCategories.Delete(link);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        public void ApplyStatus(OrderRecord order, string orderStatus, bool changePayment = true)
        {
            var campId = order.Campaign.Id; //order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id;
            OrderStatus newStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), orderStatus);
            order.OrderStatusRecord = (_orderStatusRepository.Get(int.Parse(newStatus.ToString("d"))));
            _orderRepository.Update(order);
            
            _orderRepository.Flush();


            UpdateCampaginSoldCount(order.Campaign.Id);


            // ;
            var pathToTemplates = System.Web.HttpContext.Current.Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
            var pathToMedia =System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + System.Web.HttpContext.Current.Request.ApplicationPath.TrimEnd('/');
            //ToDo: delete order if new status is "Cancelled"

            var ordersForCampaign = _orderRepository.Table.Where(aa => aa.Campaign.Id == order.Campaign.Id);
            var i = 0;

            foreach (var item in ordersForCampaign)
            {

                if (item.OrderStatusRecord != null && item.OrderStatusRecord.Name != "Delivered")
                {
                    i++;
                }
            }
            if (i == 0)
            {
                if (!order.Campaign.IsActive && order.Campaign.ProductCountGoal <= order.Campaign.ProductCountSold)
                {
                    _teeyootMessagingService.SendAllOrderDeliveredMessageToSeller(
                                        (order.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id));
                }

            }
            _teeyootMessagingService.SendOrderStatusMessage(pathToTemplates, pathToMedia, order.Id, orderStatus);
        }


        public void CheckExpiredCampaigns()
        {
            //return;
            var campaigns = _campaignRepository
                .Table
                .Where(c => c.EndDate < DateTime.UtcNow.AddMinutes(2) && c.IsActive && c.IsApproved)
                .ToList();
            var CampaignEndButNotDeliverdCampaigns = _campaignRepository.Table.Where(c => c.EndDate < DateTime.Now.AddMinutes(2) && c.IsApproved && c.CampaignEndButNotDeliverd);
            Logger.Information("Check expired campaign --------------- > {0} expired campaigns found", campaigns.Count);

            foreach (var c in campaigns)
            {
                //c.CampaignStatusRecord = _statusRepository
                //                            .Table
                //                            .First(s => s.Name == CampaignStatus.Ended.ToString());

                c.IsActive = false;
                c.IsFeatured = false;
                _campaignRepository.Update(c);
                _campaignRepository.Flush();

                if (!c.WhenDeleted.HasValue)
                {
                    var orders =
                        _ocpRepository.Table.Where(
                            p => p.CampaignProductRecord.CampaignRecord_Id == c.Id && p.OrderRecord.IsActive)
                            .Select(pr => pr.OrderRecord)
                            .Distinct()
                            .ToList();

                    var isSuccesfull = c.ProductMinimumGoal <= c.ProductCountSold;
                    _teeyootMessagingService.SendExpiredCampaignMessageToSeller(c.Id, isSuccesfull);
                    _teeyootMessagingService.SendExpiredCampaignMessageToBuyers(c.Id, isSuccesfull);
                    _teeyootMessagingService.SendExpiredCampaignMessageToAdmin(c.Id, isSuccesfull);

                    if (isSuccesfull) this.CalculateCampaignProfit(c.Id);
                    if (isSuccesfull) this.CreatePayoutData(c.Id);

                    if (!isSuccesfull)
                    {
                        c.ProductCountGoal = 0;
                        c.CampaignProfit = 0.ToString();
                        c.ClaimableProfit = 0;
                        c.UnclaimableProfit = 0;
                        _campaignRepository.Update(c);


                        //TYY-11
                        foreach (var o in orders)
                        {

                            if (o.OrderStatusRecord.Name == OrderStatus.Pending.ToString() || o.OrderStatusRecord.Name == OrderStatus.Approved.ToString())
                                ApplyStatus(o, OrderStatus.Cancelled.ToString(), false);
                        }
                        //end TYT-11
                    }
                    foreach (var o in orders)
                    {
                        if (o.OrderStatusRecord.Name == OrderStatus.Approved.ToString())
                        {
                            //o.OrderStatusRecord = isSuccesfull ?
                            //    _orderStatusRepository.Table.First(s => s.Name == OrderStatus.Printing.ToString()) :
                            //    _orderStatusRepository.Table.First(s => s.Name == OrderStatus.Cancelled.ToString());


                            if (isSuccesfull && o.TranzactionId != null)
                            {
                                try
                                {
                                    Gateway.Transaction.SubmitForSettlement(o.TranzactionId);
                                    o.Paid = DateTime.UtcNow;
                                }
                                catch (Exception e)
                                {
                                    Logger.Error("Error when trying to make transaction ---------------------- > {0}",
                                        e.ToString());
                                }
                            }

                            _orderRepository.Update(o);
                            _orderRepository.Flush();

                            string eventStr = isSuccesfull
                                ? T("The campaign successfully reached its goal!").ToString()
                                : T(
                                    "The campaign failed to reach its goal by the deadline. You will not be charged and the shirts will not be printed.")
                                    .ToString();

                            _orderHistoryRepository.Create(new OrderHistoryRecord
                            {
                                EventDate = DateTime.UtcNow,
                                OrderRecord_Id = o.Id,
                                Event = eventStr
                            });

                            eventStr = isSuccesfull
                                ? T("The campaign has ended and your order is now being printed!").ToString()
                                : T("Your order was cancelled.").ToString();

                            _orderHistoryRepository.Create(new OrderHistoryRecord
                            {
                                EventDate = DateTime.UtcNow,
                                OrderRecord_Id = o.Id,
                                Event = eventStr
                            });
                            _orderHistoryRepository.Flush();
                        }
                    }
                }
            }

            foreach (var c in CampaignEndButNotDeliverdCampaigns)
            {
                this.CalculateCampaignProfit(c.Id);
                this.CreatePayoutData(c.Id);
            }
        }

        public void CreatePayoutData(int campaignId)
        {
            //throw new NotImplementedException();
            var campaign = this.GetCampaignById(campaignId);
            if (campaign.ClaimableProfit == 0) return;
            var _payout = _payoutRespository.Fetch(a => a.CampaignId == campaignId).OrderByDescending(aa => aa.Date);

            if (_payout == null || _payout.Count() == 0 && campaign.ClaimableProfit > 0)
            {
                var payout = new PayoutRecord()
                {
                    Amount = campaign.ClaimableProfit,
                    CampaignId = campaign.Id,
                    Currency_Id = campaign.CurrencyRecord.Id,
                    Date = DateTime.Now,
                    Event = "",
                    IsCampiaign = true,
                    IsOrder = false,
                    IsPlus = true,
                    IsProfitPaid = false,
                    Status = "Completed",
                    UserId = (campaign.Seller == null) ? (campaign.TeeyootUserId.HasValue ? campaign.TeeyootUserId.Value : 0) : campaign.Seller.Id,
                    Description = "Campaign " + campaign.Alias + "(" + campaign.Id + ")" + " just made some profit"
                    //"For Campaign " + campaign.Alias + "(" + campaign.Id + ")",
                };
                _payoutRespository.Create(payout);
            }
            else
            {
                var amount = _payout.Sum(aa => aa.Amount);
                var new_amount = campaign.ClaimableProfit - amount;
                if (new_amount * 100 > 1)
                {
                    var payout = new PayoutRecord()
                    {
                        Amount = new_amount,
                        CampaignId = campaign.Id,
                        Currency_Id = campaign.CurrencyRecord.Id,
                        Date = DateTime.Now,
                        Event = "",
                        IsCampiaign = true,
                        IsOrder = false,
                        IsPlus = true,
                        IsProfitPaid = false,
                        Status = "Completed",
                        UserId = (campaign.Seller == null) ? (campaign.TeeyootUserId.HasValue ? campaign.TeeyootUserId.Value : 0) : campaign.Seller.Id,
                        Description = "Campaign " + campaign.Alias + "(" + campaign.Id + ")" + " just made some profit"
                    };
                    _payoutRespository.Create(payout);
                }
            }
        }

        public bool DeleteCampaign(int id)
        {
            try
            {
                var delCamp = _campaignRepository.Table.First(c => c.Id == id);
                delCamp.WhenDeleted = DateTime.UtcNow;
                delCamp.IsActive = false;
                _campaignRepository.Update(delCamp);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public IQueryable<CampaignProductRecord> GetAllCampaignProducts()
        {
            return _campProdRepository.Table;
        }

        public bool PrivateCampaign(int id, bool change)
        {
            try
            {
                var camp = GetAllCampaigns().First(c => c.Id == id);
                camp.IsPrivate = change;
                _campaignRepository.Update(camp);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetCampaignStatus(int id, CampaignStatus status)
        {
            var campaign = GetCampaignById(id);
            campaign.CampaignStatusRecord = _statusRepository.Table.First(s => s.Name == status.ToString());
            UpdateCampaign(campaign);
        }

        public void ReservCampaign(int id, string email)
        {
            var backCampaignRecord = new BringBackCampaignRecord();
            backCampaignRecord.Email = email;
            try
            {
                var campaign = GetCampaignById(id);
                backCampaignRecord.CampaignRecord = campaign;

                _backCampaignRepository.Create(backCampaignRecord);
            }
            catch (Exception e)
            {
                Logger.Error("Error when trying to make reservation of campaign ---------------------- > {0}",
                    e.ToString());
            }
        }

        public IQueryable<BringBackCampaignRecord> GetReservedRequestsOfCampaign(int id)
        {
            return _backCampaignRepository.Table.Where(c => c.CampaignRecord.Id == id);
        }

        public int GetCountOfReservedRequestsOfCampaign(int id)
        {
            return _backCampaignRepository.Table.Count(c => c.CampaignRecord.Id == id);
        }

        public IQueryable<string> GetBuyersEmailOfReservedCampaign(int id)
        {
            return _backCampaignRepository.Table.Where(c => c.CampaignRecord.Id == id).Select(c => c.Email);
        }

        public SearchCampaignsResponse SearchCampaigns(SearchCampaignsRequest request)
        {
            var response = new SearchCampaignsResponse();

            using (var connection = new SqlConnection(_shellSettings.DataConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "SearchCampaigns";

                        var currentDateParameter = new SqlParameter("@CurrentDate", SqlDbType.DateTime)
                        {
                            Value = DateTime.UtcNow
                        };
                        var skipParameter = new SqlParameter("@Skip", SqlDbType.Int)
                        {
                            Value = request.Skip
                        };
                        var takeParameter = new SqlParameter("@Take", SqlDbType.Int)
                        {
                            Value = request.Take
                        };

                        var Culter = new SqlParameter("@Culture", SqlDbType.VarChar)
                        {
                            Value = "en-MY" 
                        };

                        command.Parameters.Add(currentDateParameter);
                        command.Parameters.Add(skipParameter);
                        command.Parameters.Add(takeParameter);
                        command.Parameters.Add(Culter);

                        using (var reader = command.ExecuteReader())
                        {
                            response.Campaigns = GetSearchCampaignItemsFrom(reader);
                        }
                    }

                    FillSearchCampaignItemsWithData(response.Campaigns, transaction);

                    transaction.Commit();
                }
            }

            return response;
        }

        public SearchCampaignsResponse SearchCampaignsForTag(SearchCampaignsRequest request)
        {
            var response = new SearchCampaignsResponse();

            using (var connection = new SqlConnection(_shellSettings.DataConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "SearchCampaignsForTag";

                        var currentDateParameter = new SqlParameter("@CurrentDate", SqlDbType.DateTime)
                        {
                            Value = DateTime.UtcNow
                        };
                        var tagParameter = new SqlParameter("@Tag", SqlDbType.NVarChar, 100)
                        {
                            Value = request.Tag
                        };
                        var skipParameter = new SqlParameter("@Skip", SqlDbType.Int)
                        {
                            Value = request.Skip
                        };
                        var takeParameter = new SqlParameter("@Take", SqlDbType.Int)
                        {
                            Value = request.Take
                        };

                        var cultureParameter = new SqlParameter("@Culture", SqlDbType.NVarChar)
                        {
                           Value = _workContextAccessor.GetContext().CurrentCulture
                        };
                        
                        command.Parameters.Add(currentDateParameter);
                        command.Parameters.Add(tagParameter);
                        command.Parameters.Add(skipParameter);
                        command.Parameters.Add(takeParameter);
                        command.Parameters.Add(cultureParameter);
                        using (var reader = command.ExecuteReader())
                        {
                            response.Campaigns = GetSearchCampaignItemsFrom(reader);
                        }
                    }

                    FillSearchCampaignItemsWithData(response.Campaigns, transaction);

                    transaction.Commit();
                }
            }
            return response;
        }

        public SearchCampaignsResponse SearchCampaignsForFilter(SearchCampaignsRequest request)
        {
            var response = new SearchCampaignsResponse();

            using (var connection = new SqlConnection(_shellSettings.DataConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "SearchCampaignsForFilter";

                        var currentDateParameter = new SqlParameter("@CurrentDate", SqlDbType.DateTime)
                        {
                            Value = DateTime.UtcNow
                        };
                        var filterParameter = new SqlParameter("@Filter", SqlDbType.NVarChar, 4000)
                        {
                            Value = request.Filter
                        };
                        var skipParameter = new SqlParameter("@Skip", SqlDbType.Int)
                        {
                            Value = request.Skip
                        };
                        var takeParameter = new SqlParameter("@Take", SqlDbType.Int)
                        {
                            Value = request.Take
                        };
                        var cultureParameter = new SqlParameter("@Culture", SqlDbType.NVarChar)
                        {
                            Value = _workContextAccessor.GetContext().CurrentCulture
                        };
                        command.Parameters.Add(currentDateParameter);
                        command.Parameters.Add(filterParameter);
                        command.Parameters.Add(skipParameter);
                        command.Parameters.Add(takeParameter);
                        command.Parameters.Add(cultureParameter);

                        using (var reader = command.ExecuteReader())
                        {
                            response.Campaigns = GetSearchCampaignItemsFrom(reader);
                        }
                    }

                    FillSearchCampaignItemsWithData(response.Campaigns, transaction);

                    transaction.Commit();
                }
            }
            return response;
        }

        private static List<SearchCampaignItem> GetSearchCampaignItemsFrom(IDataReader reader)
        {
            var searchCampaigns = new List<SearchCampaignItem>();

            while (reader.Read())
            {
                var searchCampaignItem = new SearchCampaignItem
                {
                    Id = (int) reader["Id"],
                    Title = (string) reader["Title"],
                    Alias = (string) reader["Alias"],
                    EndDate = (DateTime) reader["EndDate"],
                    ProductCountSold = (int) reader["ProductCountSold"],
                    ProductMinimumGoal = (int) reader["ProductMinimumGoal"],
                    BackSideByDefault = (bool) reader["BackSideByDefault"]
                };

                if (reader["URL"] != DBNull.Value)
                    searchCampaignItem.Url = (string) reader["URL"];

                searchCampaigns.Add(searchCampaignItem);
            }

            return searchCampaigns;
        }

        private static void FillSearchCampaignItemsWithData(
            IList<SearchCampaignItem> searchCampaignItems,
            IDbTransaction transaction)
        {
            if (!searchCampaignItems.Any())
                return;

            using (var command = transaction.Connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "GetCampaignsFirstProductData";

                // http://www.sommarskog.se/arrays-in-sql-2008.html#TVP_in_TSQL
                var campaignIdsValue = new List<SqlDataRecord>();
                foreach (var searchCampaignItem in searchCampaignItems)
                {
                    var campaignIdValue = new SqlDataRecord(new SqlMetaData("N", SqlDbType.BigInt));
                    campaignIdValue.SetInt64(0, Convert.ToInt64(searchCampaignItem.Id));

                    campaignIdsValue.Add(campaignIdValue);
                }

                var campaignIdsParameter = new SqlParameter("@CampaignIds", SqlDbType.Structured)
                {
                    TypeName = "INTEGER_LIST_TABLE_TYPE",
                    Value = campaignIdsValue
                };

                command.Parameters.Add(campaignIdsParameter);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var campaignId = (int) reader["CampaignRecordId"];
                        var campaign = searchCampaignItems.First(c => c.Id == campaignId);

                        campaign.CampaignCurrencyId = (int) reader["CampaignCurrencyId"];

                        campaign.CampaignFirstProductId = (int) reader["CampaignFirstProductId"];
                        campaign.CampaignFirstProductPrice = (reader["CampaignFirstProductPrice"] is DBNull) ? 0 : (double)reader["CampaignFirstProductPrice"];

                        campaign.FlagFileName = (reader["FlagFileName"] is DBNull) ? "" : (string)reader["FlagFileName"];
                    }
                }
            }
        }





        public void CalculateCampaignProfit(int campaignID, bool save = false)
        {

            var campaign = _campaignRepository.Get(campaignID);
            int totalSold = 0;
            var campaignOrders = _orderRepository.Table.Where(aa => aa.Campaign != null).Where(aa => aa.Campaign.Id == campaignID && aa.IsActive);// && aa.Campaign.IsActive && !string.IsNullOrWhiteSpace(aa.Email));
            Dictionary<int, float> Productcost = new Dictionary<int, float>();

            int countOfCODthatnotDeliverd = 0;

            foreach (var product in campaign.Products)
            {
                Productcost.Add(product.ProductRecord.Id, CalculateBaseCost(campaignID, product.Id, campaign.ProductCountSold));
            }


            double totalCampaignProfit = 0;
            double UnclaimableProfit = 0;

            foreach (var order in campaignOrders)
            {
                double orderTotal = (order.TotalPriceWithPromo != 0) ? order.TotalPriceWithPromo : order.TotalPrice;
                double orderBasePrice = 0;

                //bool isCod = order.PaymentMethod == "COD";

                //if (isCod)
                //{
                //    if (order.OrderStatusRecord.Name == OrderStatus.Delivered.ToString())
                //    {
                //        totalSold += order.TotalSold;
                //        orderBasePrice = 0;
                //        foreach (var item in order.Products)
                //        {
                //            orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
                //        }

                //        totalCampaignProfit += (orderTotal - orderBasePrice);
                //    }
                //    else
                //    {
                //        if (order.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
                //            order.OrderStatusRecord.Name != OrderStatus.Refunded.ToString() &&
                //            order.OrderStatusRecord.Name != OrderStatus.Pending.ToString())
                //        {
                //            totalSold += order.TotalSold;
                //            countOfCODthatnotDeliverd++;
                //            orderBasePrice = 0;
                //            foreach (var item in order.Products)
                //            {
                //                orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
                //            }
                //            UnclaimableProfit += (orderTotal - orderBasePrice);
                //        }
                //    }
                //}
                //else
                //{
                if (campaign.IsActive)
                {
                    if (order.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
                        order.OrderStatusRecord.Name != OrderStatus.Refunded.ToString() &&
                        order.OrderStatusRecord.Name != OrderStatus.Pending.ToString())
                    {

                        totalSold += order.TotalSold;
                        orderBasePrice = 0;
                        foreach (var item in order.Products)
                        {
                            orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
                        }
                        UnclaimableProfit += (orderTotal - orderBasePrice);
                    }
                }
                else
                {
                    if (order.OrderStatusRecord.Name != OrderStatus.Cancelled.ToString() &&
                        order.OrderStatusRecord.Name != OrderStatus.Refunded.ToString() &&
                        order.OrderStatusRecord.Name != OrderStatus.Pending.ToString())
                    {

                        totalSold += order.TotalSold;
                        orderBasePrice = 0;
                        foreach (var item in order.Products)
                        {
                            orderBasePrice += item.Count * (Productcost[item.CampaignProductRecord.ProductRecord.Id]);
                        }
                        totalCampaignProfit += (orderTotal - orderBasePrice);
                    }
                }
                
                //}
            }

            campaign.UnclaimableProfit = UnclaimableProfit < 0 ? 0 : UnclaimableProfit;
            campaign.NumOfApprovedButNotDeliverd = countOfCODthatnotDeliverd;
            campaign.ClaimableProfit = totalCampaignProfit < 0 ? 0: totalCampaignProfit;
            campaign.ProductCountSold = totalSold;
            campaign.CampaignProfit = ((totalCampaignProfit + UnclaimableProfit) < 0 ? 0 : (UnclaimableProfit + totalCampaignProfit)).ToString();
            if (!save) campaign.CampaignEndAndDelivered = (countOfCODthatnotDeliverd == 0);
            if (!save) campaign.CampaignEndButNotDeliverd = (countOfCODthatnotDeliverd != 0);

            if (campaign.EndDate < DateTime.Today) campaign.IsActive = false;

            _campaignRepository.Update(campaign);
        }

        public float CalculateBaseCost(int campaignID, int productID, int soldcount)
        {
            var culture = (_workContextAccessor.GetContext() == null) ? "en-MY" : _workContextAccessor.GetContext().CurrentCulture.Trim();
            //TShirtCostRecord cost = _costService.GetCost(culture);
            var campaign = this.GetCampaignById(campaignID);
            var product = campaign.Products.Where(aa => aa.Id == productID).First();
            //if (soldcount >= campaign.ProductCountGoal) return (float)product.BaseCost;
            int CntBackColor = campaign.CntBackColor;
            int CntFrontColor = campaign.CntFrontColor;
            if (CntBackColor == 0 && CntFrontColor == 0) CntFrontColor = 1;
            else
            {
                CntBackColor = campaign.CntBackColor;// == 0 ? 1 : campaign.CntBackColor;
                CntFrontColor = campaign.CntFrontColor;// == 0 ? 1 : campaign.CntFrontColor;
            }
            TShirtCostRecord cost = (campaign.TShirtCostRecord != null) ? campaign.TShirtCostRecord : _costService.GetCost(culture);

            double B3 = cost.FirstScreenCost;	//1st Screen Cost (RM)	
            double B4 = cost.AdditionalScreenCosts;	//Additional Screen Costs (RM)	
            double B5 = cost.InkCost;	//Ink Cost (RM per litre per colour)	
            double B6 = cost.PrintsPerLitre;	//Prints per litre	
            double B7 = cost.LabourCost;	//Labour Cost (RM per hr)	
            double B8 = cost.LabourTimePerColourPerPrint;	//Labour time per colour per print (seconds)	
            double B9 = cost.LabourTimePerSidePrintedPerPrint;	//Labour time per side printed per print (seconds)	
            double B10 = (product.CostOfMaterial == 0) ? product.ProductRecord.BaseCost : product.CostOfMaterial;
            
            //product.ProductRecord.BaseCost;	//Cost of material/ t-shirt (RM each)	
            double B11 = cost.PercentageMarkUpRequired / 100;	//Percentage Mark-Up required	
            double B12 = cost.DTGPrintPrice;	//DTG print price (RM)	

            double B14 = CntFrontColor;	//Number of colours (front)	
            double B15 = CntBackColor;	//Number of colours (back)	
            double B16 = soldcount;	//Quantity	

            var x = Math.Min(
                    B10 + B12, (B3 * Math.Min(B14, 1) +
                    B4 * Math.Max(0, B14 - 1) + B3 * Math.Min(B15, 1) +
                    B4 * Math.Max(0, B15 - 1) +
                    B7 * B8 / 3600 * B16 * (B14 + B15) +
                    B5 / B6 * B16 * (B14 + B15) +
                    B7 * (B9 / 3600) * ((B14 > 0 ? 1 : 0) + (B15 > 0 ? 1 : 0)) * B16 + B10 * B16) / B16) * (1 + B11);
            return (float)x;
        }



        public void UpdateCampaginSoldCount(int campaignId)
        {
            var campaign = _campaignRepository.Table.FirstOrDefault(c => c.Id == campaignId);
            if (campaign == null) return;

            var totalsold = 0;
            var orders = _orderRepository.Table.Where(aa => aa.Campaign.Id == campaign.Id);
            
            foreach (var item in orders)
            {

                if (item.OrderStatusRecord.Name == OrderStatus.Approved.ToString() ||
                    item.OrderStatusRecord.Name == OrderStatus.Delivered.ToString() ||
                    item.OrderStatusRecord.Name == OrderStatus.Printing.ToString() ||
                    item.OrderStatusRecord.Name == OrderStatus.Shipped.ToString())
                {
                    totalsold += item.TotalSold;
                }
            }
            campaign.ProductCountSold = totalsold;

            UpdateCampaign(campaign);
        }
    }
}
