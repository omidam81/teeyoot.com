using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Themes;
using RM.QuickLogOn.OAuth.Models;
using Teeyoot.Module.Common.Utils;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Module.Controllers
{
    [Themed]
    public class WizardController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly ICampaignService _campaignService;
        private readonly IimageHelper _imageHelper;
        private readonly IFontService _fontService;
        private readonly IProductService _productService;
        private readonly ISwatchService _swatchService;
        private readonly ITShirtCostService _costService;
        private readonly ITeeyootMessagingService _teeyootMessagingService;
        private readonly IRepository<CommonSettingsRecord> _commonSettingsRepository;
        private readonly IRepository<ArtRecord> _artRepository;
        private readonly IRepository<CheckoutCampaignRequest> _checkoutCampaignRequestRepository;
        private readonly ShellSettings _shellSettings;
        private readonly string _cultureUsed;
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        private readonly IPriceConversionService _priceConversionService;

        private const int ArtsPageSize = 30;
        private const string SendEmailRequestAcceptedKey = "SendEmailAcceptedRequest";
        private const string InvalidEmailKey = "InvalidEmail";

        public WizardController(
            IOrchardServices orchardServices,
            ICampaignService campaignService,
            IimageHelper imageHelper,
            IFontService fontService,
            IProductService productService,
            ISwatchService swatchService,
            ITShirtCostService costService,
            ITeeyootMessagingService teeyootMessagingService,
            IRepository<CommonSettingsRecord> commonSettingsRepository,
            IRepository<ArtRecord> artRepository,
            IRepository<CheckoutCampaignRequest> checkoutCampaignRequestRepository,
            ShellSettings shellSettings,
            IWorkContextAccessor workContextAccessor,
            IRepository<CurrencyRecord> currencyRepository,
            IPriceConversionService priceConversionService)
        {
            _orchardServices = orchardServices;
            _campaignService = campaignService;
            _imageHelper = imageHelper;
            _fontService = fontService;
            _productService = productService;
            _swatchService = swatchService;
            Logger = NullLogger.Instance;
            _costService = costService;
            _teeyootMessagingService = teeyootMessagingService;
            _commonSettingsRepository = commonSettingsRepository;
            _checkoutCampaignRequestRepository = checkoutCampaignRequestRepository;
            _shellSettings = shellSettings;
            T = NullLocalizer.Instance;
            _artRepository = artRepository;

            var culture = workContextAccessor.GetContext().CurrentCulture.Trim();
            _cultureUsed = culture == "en-SG" ? "en-SG" : (culture == "id-ID" ? "id-ID" : "en-MY");
            _currencyRepository = currencyRepository;
            _priceConversionService = priceConversionService;
        }

        public ILogger Logger { get; set; }

        private Localizer T { get; set; }

        // GET: Wizard
        public ActionResult Index(int? id)
        {
            
            ViewBag.Currencies = _currencyRepository.Table.ToArray();

            var commonSettings = _commonSettingsRepository.Table
                .FirstOrDefault(s => s.CommonCulture == _cultureUsed);

            if (commonSettings == null)
            {
                _commonSettingsRepository.Create(new CommonSettingsRecord()
                {
                    DoNotAcceptAnyNewCampaigns = false,
                    CommonCulture = _cultureUsed
                });

                commonSettings = _commonSettingsRepository.Table
                    .First(s => s.CommonCulture == _cultureUsed);
            }

            if (commonSettings.DoNotAcceptAnyNewCampaigns)
            {
                return RedirectToAction("Oops");
            }

            var product = _productService.GetAllProducts()
                .Where(pr => pr.ProductHeadlineRecord.ProdHeadCulture == _cultureUsed);

            var group = _productService.GetAllProductGroups()
                .Where(gr => gr.ProdGroupCulture == _cultureUsed);

            var color = _productService.GetAllColorsAvailable()
                .Where(col => col.ProdColorCulture == _cultureUsed);

            //var art = _artRepository.Table.Where(a => a.ArtCulture == cultureUsed);
            var font = _fontService.GetAllfonts()
                .Where(f => f.FontCulture == _cultureUsed);

            var swatch = _swatchService.GetAllSwatches()
                .Where(s => s.SwatchCulture == _cultureUsed);

            var currencies = _currencyRepository.Table
                .Where(c => c.CurrencyCulture == _cultureUsed);

            var sizes = _productService.GetAllProducts()
                .Where(pr => pr.ProductImageRecord.ProdImgCulture == _cultureUsed);
            //var images = _productService.GetAllProducts().Where(pr => pr.);

            if (!product.Any() ||
                !group.Any() ||
                !color.Any() ||
                !font.Any() ||
                !swatch.Any() ||
                !currencies.Any() ||
                !sizes.Any())
            {
                return RedirectToAction("Oops");
            }

            var cost = _costService.GetCost(_cultureUsed);

            var currencyFrom = _currencyRepository.Table
                .First(c => c.CurrencyCulture == _cultureUsed);

            var costViewModel = new AdminCostViewModel();
            if (cost != null)
            {
                costViewModel.AdditionalScreenCosts = cost.AdditionalScreenCosts.ToString();

                var dtgPrintPrice = cost.DTGPrintPrice; //_priceConversionService
                    //.ConvertPrice(cost.DTGPrintPrice, currencyFrom, ExchangeRateFor.Seller);
                costViewModel.DTGPrintPrice = dtgPrintPrice.ToString();

                var firstScreenCost = cost.FirstScreenCost; //_priceConversionService
                    //.ConvertPrice(cost.FirstScreenCost, currencyFrom, ExchangeRateFor.Seller);
                costViewModel.FirstScreenCost = firstScreenCost.ToString();

                var inkCost = cost.InkCost; // _priceConversionService
                     //.ConvertPrice(cost.InkCost, currencyFrom, ExchangeRateFor.Seller);
                costViewModel.InkCost = inkCost.ToString();

                var laborCost = cost.LabourCost; // _priceConversionService
                     //.ConvertPrice(cost.LabourCost, currencyFrom, ExchangeRateFor.Seller);
                costViewModel.LabourCost = laborCost.ToString();

                costViewModel.LabourTimePerColourPerPrint = cost.LabourTimePerColourPerPrint;
                costViewModel.LabourTimePerSidePrintedPerPrint = cost.LabourTimePerSidePrintedPerPrint;
                costViewModel.PercentageMarkUpRequired = cost.PercentageMarkUpRequired.ToString();
                costViewModel.PrintsPerLitre = cost.PrintsPerLitre;
                costViewModel.SalesGoal = cost.SalesGoal;
                costViewModel.MinimumTarget = cost.MinimumTarget;
                costViewModel.MaxColors = cost.MaxColors;
                costViewModel = ReplaceAllCost(costViewModel);
            }

            if (id != null && id > 0)
            {
                var campaignId = (int)id;
                var campaign = _campaignService.GetCampaignById(campaignId);
                var products = _campaignService.GetProductsOfCampaign(campaignId).ToList();
                costViewModel.Campaign = campaign;
                costViewModel.Products = products;
            }

            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                var currentUserRoles = _orchardServices.WorkContext.CurrentUser.ContentItem.As<UserRolesPart>().Roles;
                costViewModel.IsCurrentUserAdministrator = currentUserRoles.Any(r => r == "Administrator");
            }

            var facebookSettingsPart = _orchardServices.WorkContext.CurrentSite.As<FacebookSettingsPart>();
            costViewModel.FacebookClientId = facebookSettingsPart.ClientId;

            var googleSettingsPart = _orchardServices.WorkContext.CurrentSite.As<GoogleSettingsPart>();
            costViewModel.GoogleClientId = googleSettingsPart.ClientId;

            costViewModel.GoogleApiKey = "AIzaSyBijPOV5bUKPNRKTE8areEVNi81ji7sS1I";

            costViewModel.CampaignCurrencyId = _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD") != null ? _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD").Id : _priceConversionService.CurrentUserCurrency.Id;
            costViewModel.CurrencyCulture = _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD") != null ? "USD" : _priceConversionService.CurrentUserCurrency.Code;

            costViewModel.IsAdmin = _orchardServices.WorkContext.CurrentUser != null;

            
            return View(costViewModel);
        }

        [HttpPost]
        // [ValidateAntiForgeryToken] Temporary turned off because of not issuing __RequestVerificationToken cookie
        [ValidateInput(false)]
        public HttpStatusCodeResult LaunchCampaign(LaunchCampaignData data)
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


            if (string.IsNullOrWhiteSpace(data.CampaignTitle) && string.IsNullOrWhiteSpace(data.Description) &&
                string.IsNullOrWhiteSpace(data.Alias))
            {
                var error = "name|" + T("Campaign Title can't be empty") + "|campaign_description_text|" +
                            T("Campaign Description can't be empty") + "|url|" +
                            T("Campaign URL can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.CampaignTitle) && string.IsNullOrWhiteSpace(data.Description))
            {
                var error = "name|" + T("Campaign Title can't be empty") + "|campaign_description_text|" +
                            T("Campaign Description can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.CampaignTitle) && string.IsNullOrWhiteSpace(data.Alias))
            {
                var error = "name|" + T("Campaign Title can't be empty") + "|url|" +
                            T("Campaign URL can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.Description) && string.IsNullOrWhiteSpace(data.Alias))
            {
                var error = "campaign_description_text|" + T("Campaign Description can't be empty") +
                            "|url|" + T("Campaign URL can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.CampaignTitle))
            {
                var error = "name|" + T("Campaign Title can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.Description))
            {
                var error = "campaign_description_text|" + T("Campaign Description can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.Alias))
            {
                var error = "url|" + T("Campaign URL can't be empty");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            data.Alias = data.Alias.Trim();

            if (data.Alias.Any(ch => char.IsWhiteSpace(ch)))
            {
                var error = "url|" + T("Campaign URL can't contain whitespaces");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (data.Alias.Contains('&') || data.Alias.Contains('?') || data.Alias.Contains('/') ||
                data.Alias.Contains('\\'))
            {
                var error = "url|" + T("Campaign URL has wrong format");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (_campaignService.GetCampaignByAlias(data.Alias) != null)
            {
                var error = "url|" + T("Campaign with this URL already exists");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }
            if (data.Alias.Length < 4)
            {
                var error = "url|" + T("Campaign URL must be at least 4 characters long");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (string.IsNullOrWhiteSpace(data.Design))
            {
                var error = "Design|" + T("No design found for your campaign");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            }

            if (_orchardServices.WorkContext.CurrentUser == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            var campaignCurrency = _currencyRepository.Get(data.CampaignCurrencyId);

            //if (campaignCurrency != _priceConversionService.CurrentUserCurrency)
            //{
            //    var error = T("Campaign currency is not the same as user currency").ToString();
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, error);
            //}

            try
            {
                foreach (var prod in data.Products)
                {
                    double price;
                    if (!double.TryParse(prod.Price, out price))
                    {
                        double.TryParse(prod.Price.Replace('.', ','), out price);
                    }
                    double cost;
                    if (!double.TryParse(prod.BaseCost, out cost))
                    {
                        double.TryParse(prod.BaseCost.Replace('.', ','), out cost);
                    }

                    if (price < cost)
                    {
                        prod.Price = prod.BaseCost;
                    }
                }

                data.CampaignCulture = _cultureUsed;
                ////TODO: (auth:keinlekan) После удаления поля в таблице/моделе - удалить данный код

                var campaign = _campaignService.CreateNewCampiagn(data);

                CreateImagesForCampaignProducts(campaign);
                var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                  Request.ApplicationPath.TrimEnd('/');
                _teeyootMessagingService.SendNewCampaignAdminMessage(pathToTemplates, pathToMedia, campaign.Id);


                ////TODO: add conditional here to allow admin to set setting for it, is campaign is auto-approved or not. 
                Approve(campaign.Id);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e, "", null);
                Logger.Error("Error occured when trying to create new campaign ---------------> " + e);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError,
                    T("Error occured when trying to create new campaign").ToString());
            }
        }


        //Approve campaign by default. 
        [NonAction]
        private void Approve(int id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            campaign.IsApproved = true;
            campaign.Rejected = false;
            campaign.WhenApproved = DateTime.UtcNow;

            var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
            var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/');

            if (!campaign.IsArchived && campaign.BaseCampaignId != null)
            {
                _teeyootMessagingService.SendReLaunchApprovedCampaignMessageToSeller(pathToTemplates, pathToMedia, campaign.Id);
                _teeyootMessagingService.SendReLaunchApprovedCampaignMessageToBuyers(pathToTemplates, pathToMedia, campaign.Id);
            }
            else
            {
                _teeyootMessagingService.SendLaunchCampaignMessage(pathToTemplates, pathToMedia, campaign.Id);
            }
        }


        [HttpPost]
        public JsonResult UpoadArtFile(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }

            var fileExt = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid() + fileExt;
            var path = Path.Combine(Server.MapPath("/Modules/Teeyoot.Module/Content/uploads/"), fileName);
            file.SaveAs(path);

            return Json(new ArtUpload {Name = fileName});
        }

        public JsonResult GetDetailTags(string filter)
        {
            var entries = _campaignService.GetAllCategories()
                .Where(c => c.CategoriesCulture == _cultureUsed)
                .Select(n => new {name = n.Name})
                .ToList();

            return Json(entries, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFonts()
        {
            var fonts = _fontService.GetAllfonts().Where(f => f.FontCulture == _cultureUsed);
            return Json(fonts.Select(f => new
            {
                id = f.Id,
                family = f.Family,
                filename = f.FileName,
                tags = f.Tags,
                priority = f.Priority
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetSwatches()
        {
            var swatches = _swatchService.GetAllSwatches().Where(s => s.SwatchCulture == _cultureUsed);
            return Json(swatches.ToList().Select(s => new
            {
                id = s.Id,
                name = s.Name,
                inStock = s.InStock,
                rgb = new[] {s.Red, s.Green, s.Blue}
            }), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetArts(string query, int page)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new List<ArtItemDto>(), JsonRequestBehavior.AllowGet);
            }

            var arts = _artRepository.Table
                .Where(a => a.Name.Contains(query.ToLowerInvariant()) && a.ArtCulture == _cultureUsed);

            if (page > 0)
            {
                arts = arts.Skip((page - 1)*ArtsPageSize);
            }

            arts = arts.Take(ArtsPageSize);

            var artDtos = arts.ToList()
                .Select(a => new ArtItemDto
                {
                    id = a.Id,
                    name = a.Name,
                    filename = a.FileName
                });

            return Json(artDtos, JsonRequestBehavior.AllowGet);
        }


       



        public JsonResult GetRandomArts()
        {
            var arts = new List<ArtItemDto>();

            using (var connection = new SqlConnection(_shellSettings.DataConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText =
                            " SELECT TOP (@artsPageSize) * FROM Teeyoot_Module_ArtRecord WHERE ArtCulture = @artCulture ORDER BY NEWID()";

                        var artsPageSizeParameter = new SqlParameter("@artsPageSize", SqlDbType.Int)
                        {
                            Value = ArtsPageSize
                        };
                        var artCulture = new SqlParameter("@artCulture", SqlDbType.VarChar)
                        {
                            Value = _cultureUsed
                        };
                        command.Parameters.Add(artsPageSizeParameter);
                        command.Parameters.Add(artCulture);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var artItem = new ArtItemDto
                                {
                                    id = (int) reader["Id"],
                                    name = (string) reader["Name"],
                                    filename = (string) reader["FileName"]
                                };

                                arts.Add(artItem);
                            }
                        }
                    }
                    transaction.Commit();
                }
            }

            return Json(arts, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProducts()
        {
            var model = new WizardProductsViewModel();

            var currencyFrom = _currencyRepository.Table
                .First(c => c.CurrencyCulture == _cultureUsed);

            var products = _productService.GetAllProducts()
                .Where(pr => pr.ProductHeadlineRecord.ProdHeadCulture == _cultureUsed)
                .ToList();

            var groups = _productService.GetAllProductGroups().OrderBy(aa=>aa.xOrder)
                .Where(gr => gr.ProdGroupCulture == _cultureUsed)
                .ToList();

            var colors = _productService.GetAllColorsAvailable()
                .Where(col => col.ProdColorCulture == _cultureUsed);

            model.product_colors = colors.Select(c => new ColorViewModel
            {
                id = c.Id,
                name = c.Name,
                value = c.Value,
                importance = c.Importance
            }).ToArray();

            model.product_images = products.Select(p => new ProductImageViewModel
            {
                id = p.ProductImageRecord.Id,
                product_id = p.Id,
                width = p.ProductImageRecord.Width,
                height = p.ProductImageRecord.Height,
                ppi = p.ProductImageRecord.Ppi,
                printable_back_height = p.ProductImageRecord.PrintableBackHeight,
                printable_back_left = p.ProductImageRecord.PrintableBackLeft,
                printable_back_top = p.ProductImageRecord.PrintableBackTop,
                printable_back_width = p.ProductImageRecord.PrintableBackWidth,
                printable_front_height = p.ProductImageRecord.PrintableFrontHeight,
                printable_front_left = p.ProductImageRecord.PrintableFrontLeft,
                printable_front_top = p.ProductImageRecord.PrintableFrontTop,
                printable_front_width = p.ProductImageRecord.PrintableFrontWidth,
                chest_line_back = p.ProductImageRecord.ChestLineBack,
                chest_line_front = p.ProductImageRecord.ChestLineFront,
                gender = p.ProductImageRecord.Gender
            }).ToArray();

            model.product_groups = groups.Select(g => new ProductGroupViewModel
            {
                id = g.Id,
                name = g.Name,
                singular = g.Name.ToLower(),
                products = g.Products.Where(c => c.ProductRecord.WhenDeleted == null).OrderBy(aa=>aa.ProductRecord.xOrder)
                    .Select(pr => pr.ProductRecord.Id)
                    .ToArray()
            }).ToArray();

            model.products = products.Select(p => new ProductViewModel
            {
                id = p.Id,
                name = p.Name,
                headline = p.ProductHeadlineRecord.Name,
                colors_available = p.ColorsAvailable.Select(c => c.ProductColorRecord.Id).ToArray(),
                list_of_sizes = p.SizesAvailable.Count > 0
                    ? p.SizesAvailable.OrderBy(s => s.ProductSizeRecord.SizeCodeRecord.Id)
                        .First()
                        .ProductSizeRecord
                        .SizeCodeRecord.Name + " - " +
                      p.SizesAvailable.OrderBy(s => s.ProductSizeRecord.SizeCodeRecord.Id)
                          .Last()
                          .ProductSizeRecord.SizeCodeRecord.Name
                    : "",
                prices = p.ColorsAvailable.Select(c => new ProductPriceViewModel
                {
                    color_id = c.ProductColorRecord.Id,
                    //change when currency from back-end changed to USD
                    price = p.BaseCost //_priceConversionService.ConvertPrice(p.BaseCost, "MYR", _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD")).Value
                }).ToArray()
            }).ToArray();

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetProductsAsync()
        {
            var model = new WizardProductsViewModel();

            var colorTask = GetColorViewModelsAsync();
            var productTask = GetProductViewModelsAsync();
            var groupTask = GetProductGroupViewModelsAsync();
            var imageTask = GetProductImageViewModelsAsync();

            await Task.WhenAll(colorTask, productTask, groupTask, imageTask);

            model.product_colors = colorTask.Result;
            model.products = productTask.Result;
            model.product_groups = groupTask.Result;
            model.product_images = imageTask.Result;

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #region Helper methods

        private object _locker = new object();

        private Task<ColorViewModel[]> GetColorViewModelsAsync()
        {
            var tcs = new TaskCompletionSource<ColorViewModel[]>();
            Task.Run(() =>
            {
                try
                {
                    lock (_locker)
                    {
                        var result =
                            _productService.GetAllColorsAvailable()
                                .Where(c => c.ProdColorCulture == _cultureUsed)
                                .Select(c => new ColorViewModel
                                {
                                    id = c.Id,
                                    name = c.Name,
                                    value = c.Value,
                                    importance = c.Importance
                                }).ToArray();
                        tcs.SetResult(result);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private Task<ProductViewModel[]> GetProductViewModelsAsync()
        {
            var tcs = new TaskCompletionSource<ProductViewModel[]>();

            Task.Run(() =>
            {
                try
                {
                    lock (_locker)
                    {
                        var result =
                            _productService.GetAllProducts()
                                .Where(pr => pr.ProductHeadlineRecord.ProdHeadCulture == _cultureUsed)
                                .ToList()
                                .Select(p => new ProductViewModel
                                {
                                    id = p.Id,
                                    name = p.Name,
                                    headline = p.ProductHeadlineRecord.Name,
                                    colors_available = p.ColorsAvailable.Select(c => c.ProductColorRecord.Id).ToArray(),
                                    list_of_sizes = p.SizesAvailable.Count > 0
                                        ? p.SizesAvailable.First().ProductSizeRecord.SizeCodeRecord.Name + " - " +
                                          p.SizesAvailable.Last().ProductSizeRecord.SizeCodeRecord.Name
                                        : ""
                                }).ToArray();

                        tcs.SetResult(result);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private Task<ProductGroupViewModel[]> GetProductGroupViewModelsAsync()
        {
            var tcs = new TaskCompletionSource<ProductGroupViewModel[]>();

            Task.Run(() =>
            {
                try
                {
                    lock (_locker)
                    {
                        var groups =
                            _productService.GetAllProductGroups()
                                .Where(gr => gr.ProdGroupCulture == _cultureUsed)
                                .OrderBy(aa=>aa.xOrder)
                                .ToList()
                                .Select(g => new ProductGroupViewModel
                                {
                                    id = g.Id,
                                    name = g.Name,
                                    singular = g.Name.ToLower(),
                                    products = g.Products.Select(pr => pr.ProductRecord.Id).ToArray()
                                }).ToArray();

                        tcs.SetResult(groups);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private Task<ProductImageViewModel[]> GetProductImageViewModelsAsync()
        {
            var tcs = new TaskCompletionSource<ProductImageViewModel[]>();

            Task.Run(() =>
            {
                try
                {
                    lock (_locker)
                    {
                        var images =
                            _productService.GetAllProducts()
                                .Where(pr => pr.ProductHeadlineRecord.ProdHeadCulture == _cultureUsed)
                                .Select(p => new ProductImageViewModel
                                {
                                    id = p.ProductImageRecord.Id,
                                    product_id = p.Id,
                                    width = p.ProductImageRecord.Width,
                                    height = p.ProductImageRecord.Height,
                                    ppi = p.ProductImageRecord.Ppi,
                                    printable_back_height = p.ProductImageRecord.PrintableBackHeight,
                                    printable_back_left = p.ProductImageRecord.PrintableBackLeft,
                                    printable_back_top = p.ProductImageRecord.PrintableBackTop,
                                    printable_back_width = p.ProductImageRecord.PrintableBackWidth,
                                    printable_front_height = p.ProductImageRecord.PrintableFrontHeight,
                                    printable_front_left = p.ProductImageRecord.PrintableFrontLeft,
                                    printable_front_top = p.ProductImageRecord.PrintableFrontTop,
                                    printable_front_width = p.ProductImageRecord.PrintableFrontWidth,
                                    chest_line_back = p.ProductImageRecord.ChestLineBack,
                                    chest_line_front = p.ProductImageRecord.ChestLineFront,
                                    gender = p.ProductImageRecord.Gender
                                }).ToArray();

                        tcs.SetResult(images);
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }

        private void CreateImagesForCampaignProducts(CampaignRecord campaign)
        {
            var serializer = new JavaScriptSerializer {MaxJsonLength = int.MaxValue};
            var data = serializer.Deserialize<DesignInfo>(campaign.CampaignDesign.Data);

            foreach (var p in campaign.Products)
            {
                var imageFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/images/");
                var frontPath = Path.Combine(imageFolder, "product_type_" + p.ProductRecord.Id + "_front.png");
                var backPath = Path.Combine(imageFolder, "product_type_" + p.ProductRecord.Id + "_back.png");

                CreateImagesForOtherColor(campaign.Id, p.Id.ToString(), p, data, frontPath, backPath,
                    p.ProductColorRecord.Value);

                if (p.SecondProductColorRecord != null)
                {
                    CreateImagesForOtherColor(campaign.Id,
                        p.Id + "_" + p.SecondProductColorRecord.Id, p, data, frontPath, backPath,
                        p.SecondProductColorRecord.Value);
                }

                if (p.ThirdProductColorRecord != null)
                {
                    CreateImagesForOtherColor(campaign.Id,
                        p.Id + "_" + p.ThirdProductColorRecord.Id, p, data, frontPath, backPath,
                        p.ThirdProductColorRecord.Value);
                }

                if (p.FourthProductColorRecord != null)
                {

                    CreateImagesForOtherColor(campaign.Id,
                        p.Id + "_" + p.FourthProductColorRecord.Id, p, data, frontPath, backPath,
                        p.FourthProductColorRecord.Value);
                }

                if (p.FifthProductColorRecord != null)
                {
                    CreateImagesForOtherColor(campaign.Id,
                        p.Id + "_" + p.FifthProductColorRecord.Id, p, data, frontPath, backPath,
                        p.FifthProductColorRecord.Value);
                }

            }
            //var destForder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(), p.Id.ToString());

            //if (!Directory.Exists(destForder))
            //{
            //    Directory.CreateDirectory(destForder + "/normal");
            //    Directory.CreateDirectory(destForder + "/big");
            //}

            //var frontTemplate = new Bitmap(frontPath);
            //var backTemplate = new Bitmap(backPath);

            //var rgba = ColorTranslator.FromHtml(p.ProductColorRecord.Value);

            //var front = BuildProductImage(frontTemplate, _imageHelper.Base64ToBitmap(data.Front), rgba, p.ProductRecord.ProductImageRecord.Width, p.ProductRecord.ProductImageRecord.Height,
            //    p.ProductRecord.ProductImageRecord.PrintableFrontTop, p.ProductRecord.ProductImageRecord.PrintableFrontLeft,
            //    p.ProductRecord.ProductImageRecord.PrintableFrontWidth, p.ProductRecord.ProductImageRecord.PrintableFrontHeight);
            //front.Save(Path.Combine(destForder, "normal", "front.png"));

            //var back = BuildProductImage(backTemplate, _imageHelper.Base64ToBitmap(data.Back), rgba, p.ProductRecord.ProductImageRecord.Width, p.ProductRecord.ProductImageRecord.Height,
            //    p.ProductRecord.ProductImageRecord.PrintableBackTop, p.ProductRecord.ProductImageRecord.PrintableBackLeft,
            //    p.ProductRecord.ProductImageRecord.PrintableBackWidth, p.ProductRecord.ProductImageRecord.PrintableBackHeight);
            //back.Save(Path.Combine(destForder, "normal", "back.png"));

            //_imageHelper.ResizeImage(front, 1070, 1274).Save(Path.Combine(destForder, "big", "front.png"));
            //_imageHelper.ResizeImage(back, 1070, 1274).Save(Path.Combine(destForder, "big", "back.png"));

            //frontTemplate.Dispose();
            //backTemplate.Dispose();
            //front.Dispose();
            //back.Dispose();

            var product = _campaignService.GetProductsOfCampaign(campaign.Id).First().Id;
            var destFolder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                product.ToString(), "social");
            Directory.CreateDirectory(destFolder);

            var imageSocialFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/images/");
            if (!campaign.BackSideByDefault)
            {
                var frontSocialPath = Path.Combine(imageSocialFolder,
                    "product_type_" + campaign.Products.First(pr => pr.WhenDeleted == null).ProductRecord.Id +
                    "_front.png");
                var imgPath = new Bitmap(frontSocialPath);

                _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, data.Front);
            }
            else
            {
                var backSocialPath = Path.Combine(imageSocialFolder,
                    "product_type_" + campaign.Products.First(pr => pr.WhenDeleted == null).ProductRecord.Id +
                    "_back.png");
                var imgPath = new Bitmap(backSocialPath);

                _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, data.Back);
            }
        }

        private Bitmap BuildProductImage(Bitmap image,
            Bitmap design,
            Color color,
            int width,
            int height,
            int printableAreaTop,
            int printableAreaLeft,
            int printableAreaWidth,
            int printableAreaHeight)
        {
            var background = _imageHelper.CreateBackground(width, height, color);
            image = _imageHelper.ApplyBackground(image, background, color,width, height);
            return _imageHelper.ApplyDesign(image, design, printableAreaTop, printableAreaLeft, printableAreaWidth,
                printableAreaHeight, color, width, height);
        }

        #endregion

        public AdminCostViewModel ReplaceAllCost(AdminCostViewModel cost)
        {
            cost.AdditionalScreenCosts = cost.AdditionalScreenCosts.Replace(",", ".");
            cost.DTGPrintPrice = cost.DTGPrintPrice.Replace(",", ".");
            cost.FirstScreenCost = cost.FirstScreenCost.Replace(",", ".");
            cost.InkCost = cost.InkCost.Replace(",", ".");
            cost.LabourCost = cost.LabourCost.Replace(",", ".");
            cost.PercentageMarkUpRequired = cost.PercentageMarkUpRequired.Replace(",", ".");

            return cost;
        }

        public ActionResult Oops()
        {
            var viewModel = new OopsViewModel();

            if (TempData[InvalidEmailKey] != null)
            {
                viewModel.InvalidEmail = (bool) TempData[InvalidEmailKey];
            }

            if (TempData[SendEmailRequestAcceptedKey] != null)
            {
                viewModel.RequestAccepted = (bool) TempData[SendEmailRequestAcceptedKey];
            }

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Oops(OopsViewModel viewModel)
        {
            var commonSettings = _commonSettingsRepository.Table
                .FirstOrDefault(s => s.CommonCulture == _cultureUsed);

            if (commonSettings == null)
            {
                _commonSettingsRepository.Create(new CommonSettingsRecord()
                {
                    DoNotAcceptAnyNewCampaigns = false,
                    CommonCulture = _cultureUsed
                });

                commonSettings = _commonSettingsRepository.Table
                    .First(s => s.CommonCulture == _cultureUsed);

            }
            if (!commonSettings.DoNotAcceptAnyNewCampaigns)
            {
                return RedirectToAction("Oops");
            }

            if (!ModelState.IsValidField("Email"))
            {
                TempData[InvalidEmailKey] = true;
                return RedirectToAction("Oops");
            }

            var request = new CheckoutCampaignRequest {RequestUtcDate = DateTime.UtcNow, Email = viewModel.Email};
            _checkoutCampaignRequestRepository.Create(request);

            TempData[SendEmailRequestAcceptedKey] = true;
            return RedirectToAction("Oops");
        }




        public JsonResult checkDP(byte[] dataImage)
        {
            return Json("");
        }






        public void CreateImagesForOtherColor(int campaignId,
            string prodIdAndColor,
            CampaignProductRecord p,
            DesignInfo data,
            string frontPath,
            string backPath,
            string color)
        {
            var destForder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaignId.ToString(), prodIdAndColor);

            if (!Directory.Exists(destForder))
            {
                Directory.CreateDirectory(destForder + "/normal");
                Directory.CreateDirectory(destForder + "/big");
            }

            var frontTemplate = new Bitmap(frontPath);
            var backTemplate = new Bitmap(backPath);

            var rgba = ColorTranslator.FromHtml(color);

            var front = BuildProductImage(frontTemplate, _imageHelper.Base64ToBitmap(data.Front), rgba,
                p.ProductRecord.ProductImageRecord.Width, p.ProductRecord.ProductImageRecord.Height,
                p.ProductRecord.ProductImageRecord.PrintableFrontTop,
                p.ProductRecord.ProductImageRecord.PrintableFrontLeft,
                p.ProductRecord.ProductImageRecord.PrintableFrontWidth,
                p.ProductRecord.ProductImageRecord.PrintableFrontHeight);
            front.Save(Path.Combine(destForder, "normal", "front.png"));

            var back = BuildProductImage(backTemplate, _imageHelper.Base64ToBitmap(data.Back), rgba,
                p.ProductRecord.ProductImageRecord.Width, p.ProductRecord.ProductImageRecord.Height,
                p.ProductRecord.ProductImageRecord.PrintableBackTop,
                p.ProductRecord.ProductImageRecord.PrintableBackLeft,
                p.ProductRecord.ProductImageRecord.PrintableBackWidth,
                p.ProductRecord.ProductImageRecord.PrintableBackHeight);
            back.Save(Path.Combine(destForder, "normal", "back.png"));

            var frontZoom = BuildProductImage(frontTemplate, _imageHelper.Base64ToBitmap(data.Front), rgba,
                p.ProductRecord.ProductImageRecord.Width*2, p.ProductRecord.ProductImageRecord.Height*2,
                p.ProductRecord.ProductImageRecord.PrintableFrontTop*2,
                p.ProductRecord.ProductImageRecord.PrintableFrontLeft*2,
                p.ProductRecord.ProductImageRecord.PrintableFrontWidth*2,
                p.ProductRecord.ProductImageRecord.PrintableFrontHeight*2);

            var backZoom = BuildProductImage(backTemplate, _imageHelper.Base64ToBitmap(data.Back), rgba,
                p.ProductRecord.ProductImageRecord.Width*2, p.ProductRecord.ProductImageRecord.Height*2,
                p.ProductRecord.ProductImageRecord.PrintableBackTop*2,
                p.ProductRecord.ProductImageRecord.PrintableBackLeft*2,
                p.ProductRecord.ProductImageRecord.PrintableBackWidth*2,
                p.ProductRecord.ProductImageRecord.PrintableBackHeight*2);

            var rect = new Rectangle(0, 0, frontZoom.Width - 10, frontZoom.Height - 10);
            var croppedFront = frontZoom.Clone(rect, frontZoom.PixelFormat);

            croppedFront.Save(Path.Combine(destForder, "big", "front.png"));

            var rect2 = new Rectangle(0, 0, backZoom.Width - 10, backZoom.Height - 10);
            var croppedBck = backZoom.Clone(rect2, backZoom.PixelFormat);

            croppedBck.Save(Path.Combine(destForder, "big", "back.png"));

            frontTemplate.Dispose();
            backTemplate.Dispose();
            croppedFront.Dispose();
            croppedBck.Dispose();
            front.Dispose();
            back.Dispose();
        }
    }
}
