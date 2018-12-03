using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using DataTables.Mvc;
using Orchard.Data;
using Orchard.DisplayManagement;
using Orchard.Environment.Configuration;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Teeyoot.FeaturedCampaigns.Common;
using Teeyoot.FeaturedCampaigns.Models;
using Teeyoot.FeaturedCampaigns.ViewModels;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Common.Utils;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;
using Orchard.Users.Models;

namespace Teeyoot.FeaturedCampaigns.Controllers
{
    [Admin]
    public class AdminCampaignsSettingsController : Controller
    {
        private readonly IRepository<CampaignStatusRecord> _campaignStatusRepository;
        private readonly ICampaignService _campaignService;
        private readonly IimageHelper _imageHelper;
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        private readonly ITeeyootMessagingService _teeyootMessagingService;
        private readonly INotifier _notifier;
        private readonly IRepository<ProductColorRecord> _repositoryColor;
        private readonly ShellSettings _shellSettings;
        private readonly IRepository<UserPartRecord> _users;

        public AdminCampaignsSettingsController(
            IRepository<CampaignStatusRecord> campaignStatusRepository,
            ICampaignService campaignService,
            IShapeFactory shapeFactory,
            IimageHelper imageHelper,
            ITeeyootMessagingService teeyootMessagingService,
            INotifier notifier,
            IRepository<ProductColorRecord> repositoryColor,
            ShellSettings shellSettings,
            IRepository<CurrencyRecord> currencyRepository,
            IRepository<UserPartRecord> users)
        {
            _campaignStatusRepository = campaignStatusRepository;
            _campaignService = campaignService;
            _imageHelper = imageHelper;
            _teeyootMessagingService = teeyootMessagingService;
            _currencyRepository = currencyRepository;

            Shape = shapeFactory;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            _notifier = notifier;
            _repositoryColor = repositoryColor;
            _users = users;
            _shellSettings = shellSettings;
        }

        private dynamic Shape { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ActionResult Index()
        {
            var currencies = _currencyRepository.Table
                .Select(c => new CurrencyItemViewModel
                {
                    Id = c.Id,
                    Name = c.ShortName
                })
                .ToList();

            var campaignStatuses = _campaignStatusRepository.Table
                .Select(s => s.Name)
                .ToList();

            var viewModel = new ExportPrintsViewModel
            {
                Currencies = currencies,
                CampaignStatuses = campaignStatuses
            };

            return View(viewModel);
        }

        public JsonResult GetCampaigns(
            [ModelBinder(typeof (GetCampaignsDataTablesBinder))] GetCampaignsDataTablesRequest request)
        {
            IEnumerable<CampaignItemViewModel> campaignItemViewModels;
            int campaignsTotal;
            int campaignsFilteredTotal;

            using (var connection = new SqlConnection(_shellSettings.DataConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "GetCampaigns";

                        var currentDateParameter = new SqlParameter("@CurrentDate", SqlDbType.DateTime)
                        {
                            Value = DateTime.UtcNow
                        };
                        command.Parameters.Add(currentDateParameter);

                        if (request.FilterCurrencyId.HasValue)
                        {
                            var currencyIdParameter = new SqlParameter("@CurrencyId", SqlDbType.Int)
                            {
                                Value = request.FilterCurrencyId.Value
                            };
                            command.Parameters.Add(currencyIdParameter);
                        }

                        if (!string.IsNullOrWhiteSpace(request.Search.Value))
                        {
                            var filterParameter = new SqlParameter("@Filter", SqlDbType.NVarChar, 4000)
                            {
                                Value = request.Search.Value
                            };
                            command.Parameters.Add(filterParameter);
                        }

                        if (request.Columns.GetSortedColumns().Any())
                        {
                            var sortColumn = request.Columns.GetSortedColumns().First();
                            var sortColumnParameter = new SqlParameter("@SortColumn", SqlDbType.NVarChar, 100)
                            {
                                Value = sortColumn.Data
                            };
                            command.Parameters.Add(sortColumnParameter);

                            var sortDirection = sortColumn.SortDirection == Column.OrderDirection.Ascendant
                                ? "ASC"
                                : "DESC";
                            var sortDirectionParameter = new SqlParameter("@SortDirection", SqlDbType.NVarChar, 50)
                            {
                                Value = sortDirection
                            };
                            command.Parameters.Add(sortDirectionParameter);
                        }

                        var skipParameter = new SqlParameter("@Skip", SqlDbType.Int)
                        {
                            Value = request.Start
                        };
                        command.Parameters.Add(skipParameter);
                        var takeParameter = new SqlParameter("@Take", SqlDbType.Int)
                        {
                            Value = request.Length
                        };
                        command.Parameters.Add(takeParameter);

                        IEnumerable<CampaignItem> campaignItems;

                        using (var reader = command.ExecuteReader())
                        {
                            campaignItems = GetCampaignItemsFrom(reader);
                        }

                        campaignItemViewModels = ConvertToCampaignItemViewModels(campaignItems);
                    }

                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "GetCampaignsCount";

                        if (request.FilterCurrencyId.HasValue)
                        {
                            var currencyIdParameter = new SqlParameter("@CurrencyId", SqlDbType.Int)
                            {
                                Value = request.FilterCurrencyId.Value
                            };
                            command.Parameters.Add(currencyIdParameter);
                        }

                        campaignsTotal = (int) command.ExecuteScalar();
                    }

                    if (!string.IsNullOrWhiteSpace(request.Search.Value))
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "GetCampaignsCount";

                            var filterParameter = new SqlParameter("@Filter", SqlDbType.NVarChar, 4000)
                            {
                                Value = request.Search.Value
                            };
                            command.Parameters.Add(filterParameter);

                            if (request.FilterCurrencyId.HasValue)
                            {
                                var currencyIdParameter = new SqlParameter("@CurrencyId", SqlDbType.Int)
                                {
                                    Value = request.FilterCurrencyId.Value
                                };
                                command.Parameters.Add(currencyIdParameter);
                            }

                            campaignsFilteredTotal = (int) command.ExecuteScalar();
                        }
                    }
                    else
                    {
                        campaignsFilteredTotal = campaignsTotal;
                    }

                    transaction.Commit();
                }
            }

            var dataTableResponse = new DataTablesResponse(
                request.Draw,
                campaignItemViewModels,
                campaignsFilteredTotal,
                campaignsTotal);

            return Json(dataTableResponse);
        }

        private static IEnumerable<CampaignItem> GetCampaignItemsFrom(IDataReader reader)
        {
            var campaignItems = new List<CampaignItem>();

            while (reader.Read())
            {
                var campaignItem = new CampaignItem
                {
                    Profit = (double) reader["Profit"],
                    Last24HoursSold = (int) reader["Last24HoursSold"],
                    Id = (int) reader["Id"],
                    Title = (string) reader["Title"],
                    Goal = (int) reader["Goal"],
                    Sold = (int) reader["Sold"],
                    IsApproved = (bool) reader["IsApproved"],
                    EndDate = (DateTime) reader["EndDate"],
                    Alias = (string) reader["Alias"],
                    IsActive = (bool) reader["IsActive"],
                    Minimum = (int) reader["Minimum"],
                    CreatedDate = (DateTime) reader["CreatedDate"],
                    Status = (string) reader["Status"],
                    Email = (string) reader["Email"]
                };

                if (reader["PhoneNumber"] != DBNull.Value)
                    campaignItem.PhoneNumber = (string) reader["PhoneNumber"];

                if (reader["Currency"] != DBNull.Value)
                    campaignItem.Currency = (string) reader["Currency"];

                campaignItems.Add(campaignItem);
            }

            return campaignItems;
        }

        private IEnumerable<CampaignItemViewModel> ConvertToCampaignItemViewModels(
            IEnumerable<CampaignItem> campaignItems)
        {
            return campaignItems.Select(campaignItem => new CampaignItemViewModel
            {
                Id = campaignItem.Id,
                Title = campaignItem.Title,
                Alias = "/" + campaignItem.Alias,
                CreatedDate = campaignItem.CreatedDate.ToLocalTime().ToString("dd/MM/yyyy"),
                IsActive = campaignItem.IsActive ? T("Yes").ToString() : T("No").ToString(),
                Last24HoursSold = campaignItem.Last24HoursSold > 0 ? campaignItem.Last24HoursSold.ToString() : "-",
                Sold = campaignItem.Sold,
                Minimum = campaignItem.Minimum,
                Goal = campaignItem.Goal,
                Profit = campaignItem.Profit.ToString("F", System.Globalization.CultureInfo.InvariantCulture),
                Status = campaignItem.Status,
                EndDate = campaignItem.EndDate.ToLocalTime().ToString("dd/MM/yyyy"),
                Email = campaignItem.Email,
                PhoneNumber = campaignItem.PhoneNumber,
                Currency = campaignItem.Currency
            }).ToList();
        }

        public ActionResult ChangeStatus(PagerParameters pagerParameters, int id, CampaignStatus status)
        {
            _campaignService.SetCampaignStatus(id, status);
            _teeyootMessagingService.SendChangedCampaignStatusMessage(id, status.ToString());
            return RedirectToAction("Index", new {PagerParameters = pagerParameters});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpStatusCodeResult ChangeEndDate(int campaignId, int day, int month, int year)
        {
            DateTime date = new DateTime(year, month, day);
            var campaign = _campaignService.GetCampaignById(campaignId);
            campaign.EndDate = date.ToUniversalTime();

            Response.Write(campaign.EndDate.ToLocalTime().ToString("dd/MM/yyyy"));
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult ChangeInformation(int Id)
        {
            var campaign = _campaignService.GetCampaignById(Id);
            _campaignService.CalculateCampaignProfit(Id, false);
            float[] baseCost = new float[campaign.Products.Count];
            int i = 0;
            if (campaign.Products != null)
                foreach (var product in campaign.Products)
                {
                    if (campaign.ProductCountSold == 0)
                    {
                        baseCost[i++] = _campaignService.CalculateBaseCost(Id, product.Id, 1);

                    }
                    else
                    {
                        baseCost[i++] = _campaignService.CalculateBaseCost(Id, product.Id, campaign.ProductCountSold);
                    }

                }
            
            var day = campaign.EndDate.Day;
            var mounth = campaign.EndDate.Month;
            var year = campaign.EndDate.Year;
            
            var model = new CampaignInfViewModel()
            {
                CampaignId = campaign.Id,
                Title = campaign.Title,
                Alias = campaign.Alias,
                Target = campaign.ProductCountGoal,

                Day = Convert.ToInt32(day),
                Mounth = Convert.ToInt32(mounth),
                Year = Convert.ToInt32(year),
                Description = campaign.Description,
                Currency = campaign.CurrencyRecord,
                Currencies = _currencyRepository,
                BackSideByDefault = campaign.BackSideByDefault,
                Products = campaign.Products.Where(c => c.WhenDeleted == null)
            };
            ViewBag.BaseCosts = baseCost; 
            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public HttpStatusCodeResult SaveInfo(int campaignId, string Title, string URL, int Day, int Mounth, int Year, int Target, string Description, string[] Prices, int currencyId, string[] Colors, bool BackSideByDefault)
        {
            var campaign = _campaignService.GetCampaignById(campaignId);
            var campaigns = _campaignService.GetAllCampaigns();

            campaign.CurrencyRecord = _currencyRepository.Get(currencyId);

            bool resultError = false;

            if (!campaigns.Select(c => c.Alias).ToList().Contains(URL) || campaign.Alias == URL)
            {
                DateTime date = new DateTime(Year, Mounth, Day);
                if (date < DateTime.Now)
                {
                    campaign.IsActive = false;
                    campaign.IsFeatured = false;
                    var isSuccesfull = campaign.ProductCountGoal <= campaign.ProductCountSold;
                    _teeyootMessagingService.SendExpiredCampaignMessageToSeller(campaign.Id, isSuccesfull);
                    _teeyootMessagingService.SendExpiredCampaignMessageToBuyers(campaign.Id, isSuccesfull);
                    _teeyootMessagingService.SendExpiredCampaignMessageToAdmin(campaign.Id, isSuccesfull);

                }
                else if (date > DateTime.Now)
                    campaign.IsActive = true;

                campaign.Title = Title;
                campaign.Alias = URL;
                campaign.ProductCountGoal = Target;
                campaign.Description = Description;
                campaign.EndDate = date.ToUniversalTime();
                campaign.BackSideByDefault = BackSideByDefault;
                //_campaignService.UpdateCampaign(campaign);

                var address = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString());
                if (Directory.Exists(Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString()))) DeleteDirectory(address);

                Directory.CreateDirectory(address);


                var prods = campaign.Products.Where(c => c.WhenDeleted == null).ToList();


                for (int i = 0; i < prods.Count; i++)
                {
                    var price = Prices[i];//.Replace(".", ",");
                    prods[i].Price = Convert.ToDouble(price);

                    
                }
                for (int k = 0; k < Colors.Length; k++)
                {
                    var colors = Colors[k].Split('/').ToList();
                    int prodId = Int32.Parse(colors[0]);
                    colors.RemoveAt(0);

                    var prod = campaign.Products.Where(c => c.Id == prodId).First();

                    string productPath1 = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(), prod.Id.ToString());
                    string productPath2 = 
                        prod.SecondProductColorRecord != null ? 
                        
                        Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(), 
                        string.Format("{0}_{1}", prod.Id.ToString(), prod.SecondProductColorRecord.Id.ToString()))
                        : string.Empty;
                    string productPath3 = prod.ThirdProductColorRecord != null
                        ? Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                            string.Format("{0}_{1}", prod.Id.ToString(), prod.ThirdProductColorRecord.Id.ToString()))
                        : string.Empty;
                    string productPath4 = prod.FourthProductColorRecord != null
                        ? Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                            string.Format("{0}_{1}", prod.Id.ToString(), prod.FourthProductColorRecord.Id.ToString()))
                        : string.Empty;
                    string productPath5 = prod.FifthProductColorRecord != null
                        ? Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                            string.Format("{0}_{1}", prod.Id.ToString(), prod.FifthProductColorRecord.Id.ToString()))
                        : string.Empty;

                    try
                    {



                        DirectoryInfo dir = new DirectoryInfo(productPath1);
                        if (!dir.Exists) dir.Create();
                        
                        if (!string.IsNullOrEmpty(productPath2))
                        {
                            DirectoryInfo dir2 = new DirectoryInfo(productPath2);
                            if (!dir2.Exists) dir2.Create();
                        }
                        if (!string.IsNullOrEmpty(productPath3))
                        {
                            DirectoryInfo dir3 = new DirectoryInfo(productPath3);
                            if (!dir3.Exists) dir3.Create();
                        }
                        if (!string.IsNullOrEmpty(productPath4))
                        {
                            DirectoryInfo dir4 = new DirectoryInfo(productPath4);
                            if (!dir4.Exists) dir4.Create();
                        }
                        if (!string.IsNullOrEmpty(productPath5))
                        {
                            DirectoryInfo dir5 = new DirectoryInfo(productPath5);
                            if (!dir5.Exists) dir5.Create();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(
                            T(
                                "Error when trign delete directory for products --------------------------------------->" +
                                e.Message).ToString());
                        resultError = true;
                    }
                    finally
                    {
                        var serializer = new JavaScriptSerializer();
                        serializer.MaxJsonLength = int.MaxValue;
                        if (campaign.CampaignDesign != null)
                        {
                            var data = serializer.Deserialize<DesignInfo>(campaign.CampaignDesign.Data);
                            var color = _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[0])).First();


                            var imageFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/images/");
                            var frontPath = Path.Combine(imageFolder, "product_type_" + prod.ProductRecord.Id + "_front.png");
                            var backPath = Path.Combine(imageFolder, "product_type_" + prod.ProductRecord.Id + "_back.png");

                            try
                            {
                                CreateImagesForOtherColor(campaign.Id, prod.Id.ToString(), prod, data, frontPath, backPath,
                                    color.Value);
                                prod.ProductColorRecord = color;

                                if (!string.IsNullOrEmpty(colors[1]))
                                {

                                    color = _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[1])).First();

                                    CreateImagesForOtherColor(campaign.Id,
                                        string.Format("{0}_{1}", prod.Id.ToString(), color.Id.ToString()), prod, data,
                                        frontPath, backPath, color.Value);

                                    prod.SecondProductColorRecord = color;
                                }
                                else
                                {
                                    prod.SecondProductColorRecord = null;
                                }

                                if (!string.IsNullOrEmpty(colors[2]))
                                {
                                    color = _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[2])).First();
                                    _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[2])).First();

                                    CreateImagesForOtherColor(campaign.Id,
                                        string.Format("{0}_{1}", prod.Id.ToString(), color.Id.ToString()), prod, data,
                                        frontPath, backPath, color.Value);

                                    prod.ThirdProductColorRecord = color;
                                }
                                else
                                {
                                    prod.ThirdProductColorRecord = null;
                                }

                                if (!string.IsNullOrEmpty(colors[3]))
                                {
                                    color = _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[3])).First();
                                    _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[3])).First();
                                    CreateImagesForOtherColor(campaign.Id,
                                        string.Format("{0}_{1}", prod.Id.ToString(), color.Id.ToString()), prod, data,
                                        frontPath, backPath, color.Value);
                                    prod.FourthProductColorRecord = color;
                                }
                                else
                                {
                                    prod.FourthProductColorRecord = null;
                                }

                                if (!string.IsNullOrEmpty(colors[4]))
                                {
                                    color = _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[4])).First();
                                    _repositoryColor.Table.Where(c => c.Id == Int32.Parse(colors[4])).First();

                                    CreateImagesForOtherColor(campaign.Id,
                                        string.Format("{0}_{1}", prod.Id.ToString(), color.Id.ToString()), prod, data,
                                        frontPath, backPath, color.Value);

                                    prod.FifthProductColorRecord = color;
                                }
                                else
                                {
                                    prod.FifthProductColorRecord = null;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(
                                    T(
                                        "Error when trign creating images and directories for products --------------------------------------->" +
                                        ex.Message).ToString());
                                resultError = true;
                            }
                        }
                    }
                }
                if (campaign.CampaignDesign != null)
                {
                    var s = new JavaScriptSerializer();
                    s.MaxJsonLength = int.MaxValue;
                    var d = s.Deserialize<DesignInfo>(campaign.CampaignDesign.Data);

                    string destFolder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                        campaign.Products.First(pr => pr.WhenDeleted == null).Id.ToString(), "social");
                    var dirict = new DirectoryInfo(destFolder);
                    var imageSocialFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/images/");

                    if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

                    if (campaign.BackSideByDefault == false)
                    {
                        try
                        {
                            var frontSocialPath = Path.Combine(imageSocialFolder,
                                                    "product_type_" + campaign.Products.First(pr => pr.WhenDeleted == null).ProductRecord.Id +
                                                    "_front.png");
                            var imgPath = new Bitmap(frontSocialPath);

                            _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, d.Front, false);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            var backSocialPath = Path.Combine(imageSocialFolder,
                                                   "product_type_" + campaign.Products.First(pr => pr.WhenDeleted == null).ProductRecord.Id +
                                                   "_back.png");
                            var imgPath = new Bitmap(backSocialPath);

                            _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, d.Back);
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }
                //campaign.BackSideByDefault = BackSideByDefault;
                campaign.Seller = _users.Get(campaign.TeeyootUserId.Value);
                _campaignService.UpdateCampaign(campaign);
                var pathToTemplates = Server.MapPath("/Modules/Teeyoot.Module/Content/message-templates/");
                var pathToMedia = Request.Url.Scheme + "://" + Request.Url.Authority +
                                  Request.ApplicationPath.TrimEnd('/');
                _teeyootMessagingService.SendEditedCampaignMessageToSeller(campaign.Id, pathToMedia, pathToTemplates);
                Response.Write(true);
            }
            else
            {
                Response.Write(false);
            }

            if (resultError && false)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                System.IO.File.SetAttributes(file, FileAttributes.Normal);
                System.IO.File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }


        public void CreateImagesForOtherColor(int campaignId, string prodIdAndColor, CampaignProductRecord p,
            DesignInfo data, string frontPath, string backPath, string color)
        {
            var destForder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaignId.ToString(), prodIdAndColor);

            if (!Directory.Exists(destForder))
                Directory.CreateDirectory(destForder);

            if (!Directory.Exists(destForder + "/normal")) Directory.CreateDirectory(destForder + "/normal");
            if (!Directory.Exists(destForder + "/big")) Directory.CreateDirectory(destForder + "/big");

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

            _imageHelper.ResizeImage(front, p.ProductRecord.ProductImageRecord.Width * 2, p.ProductRecord.ProductImageRecord.Height * 2).Save(Path.Combine(destForder, "big", "front.png"));
            _imageHelper.ResizeImage(back, p.ProductRecord.ProductImageRecord.Width * 2, p.ProductRecord.ProductImageRecord.Height * 2).Save(Path.Combine(destForder, "big", "back.png"));

            frontTemplate.Dispose();
            backTemplate.Dispose();
            front.Dispose();
            back.Dispose();
        }

        private Bitmap BuildProductImage(Bitmap image, Bitmap design, Color color, int width, int height,
            int printableAreaTop, int printableAreaLeft, int printableAreaWidth, int printableAreaHeight)
        {
            var background = _imageHelper.CreateBackground(width, height, color);
            image = _imageHelper.ApplyBackground(image, background, color,width, height);
            return _imageHelper.ApplyDesign(image, design, printableAreaTop, printableAreaLeft, printableAreaWidth,
                printableAreaHeight,color, width, height);
        }

        public ActionResult DeleteProduct(int productId, int campaignId)
        {
            var camp = _campaignService.GetCampaignById(campaignId);

            try
            {
                camp.Products.First(c => c.Id == productId).WhenDeleted = DateTime.UtcNow;
                _campaignService.UpdateCampaign(camp);
                _notifier.Add(NotifyType.Information, T("The product was removed!"));
            }
            catch
            {
                _notifier.Add(NotifyType.Error, T("An error occurred while deleting"));
            }

            return RedirectToAction("ChangeInformation", new {id = campaignId});
        }
    }
}
