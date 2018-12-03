using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using Orchard.Themes;
using Orchard.UI.Notify;
using Teeyoot.Dashboard.ViewModels;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Common.ExtentionMethods;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.Dashboard.Controllers
{
    public partial class DashboardController
    {

        [Authorize]
        public ActionResult Campaigns(bool? isError, string result)
        {
            var model = new CampaignsViewModel();
            var user = _wca.GetContext().CurrentUser;
            var teeyootUser = (TeeyootUserPart)user.ContentItem.Get(typeof(TeeyootUserPart));

            model.CurrencyCode = teeyootUser.CurrencyRecord == null ? "USD" : teeyootUser.CurrencyRecord.Code;

            var campaignsQuery = _campaignService.GetCampaignsOfUser(user.Id).Where(aa => aa.IsApproved);
            
            var campaignList = new List<CampaignRecord>();
            foreach (var campaign in campaignsQuery)
            {
                _campaignService.CalculateCampaignProfit(campaign.Id, true);

                if (campaign.ProductMinimumGoal <= campaign.ProductCountSold && campaign.Products != null && campaign.Products.Count > 0)
                {
                    campaignList.Add(campaign);
                }
            }

            var productsOrderedQueryWithMinimum = _orderService.GetProductsOrderedOfCampaigns(campaignList.Select(c => c.Id).ToArray());

            FillCampaigns(model, campaignsQuery);
            FillOverviews(model, campaignsQuery);

            return View(model);
        }

        private void FillCampaigns(CampaignsViewModel model, IQueryable<CampaignRecord> campaignsQuery)
        {
            var user = _wca.GetContext().CurrentUser;
            var teeyootUser = (TeeyootUserPart)user.ContentItem.Get(typeof(TeeyootUserPart));

            var campaignProducts = _campaignService.GetAllCampaignProducts();
            var orderedProducts = _orderService.GetAllOrderedProducts();
            double x = 0;
            var campaignSummaries = campaignsQuery
                .Select(c => new CampaignSummary
                {
                    Alias = c.Alias,
                    EndDate = c.EndDate,
                    Goal = c.ProductCountGoal,
                    Id = c.Id,
                    Name = c.Title,
                    Sold = c.ProductCountSold,
                    Minimum = c.ProductMinimumGoal,
                    StartDate = c.StartDate,
                    Status = c.CampaignStatusRecord,
                    IsActive = c.IsActive,
                    IsArchived = c.IsArchived,
                    ShowBack = c.BackSideByDefault,
                    IsPrivate = c.IsPrivate
                })
                .OrderBy(c => c.StartDate)
                .ToArray();

            foreach (var c in campaignSummaries)
            {
                var cam = campaignsQuery.FirstOrDefault(aa => aa.Id == c.Id);
                c.FirstProductId = (campaignsQuery.FirstOrDefault(aa => aa.Id == c.Id).Products[0].Id);
                // c.Profit = double.TryParse(c.CampaignProfit, out x) ? double.Parse(c.CampaignProfit) : 0;
                c.SummaryCurrency = "USD";
                if (c.SummaryCurrency == cam.CurrencyRecord.Code)
                    c.Profit = double.Parse(cam.CampaignProfit);
                else
                    c.Profit = _priceConversionService.ConvertPrice(double.Parse(cam.CampaignProfit), cam.CurrencyRecord, teeyootUser.CurrencyRecord.Code).Value;
            }
            campaignSummaries = campaignSummaries.Where(c => c.FirstProductId > 0).ToArray();

            model.Campaigns = campaignSummaries;
        }

        [NonAction]
        private float CalculateBaseCost(int campaignID, int productID, int soldcount)
        {
            culture = _workContextAccessor.GetContext().CurrentCulture.Trim();
            //TShirtCostRecord cost = _costService.GetCost(culture);
            var campaign = _campaignService.GetCampaignById(campaignID);
            var product = campaign.Products.Where(aa => aa.Id == productID).First();
            if (soldcount >= campaign.ProductCountGoal) return (float)product.BaseCost;
            int CntBackColor = campaign.CntBackColor;
            int CntFrontColor = campaign.CntFrontColor;
            if (CntBackColor == 0 && CntFrontColor == 0) CntFrontColor = 1;
            else
            {
                CntBackColor = campaign.CntBackColor;// == 0 ? 1 : campaign.CntBackColor;
                CntFrontColor = campaign.CntFrontColor;// == 0 ? 1 : campaign.CntFrontColor;
            }
            TShirtCostRecord cost = (campaign.TShirtCostRecord != null) ? campaign.TShirtCostRecord : _costService.GetCost(culture);


            var additionalScreenCosts = cost.AdditionalScreenCosts;
            var costOfMaterial = product.ProductRecord.BaseCost;
            var dTGPrintPrice = cost.DTGPrintPrice;
            var firstScreenCost = cost.FirstScreenCost;
            var inkCost = cost.InkCost;
            var labourCost = cost.LabourCost;
            var labourTimePerColourPerPrint = cost.LabourTimePerColourPerPrint;
            var labourTimePerSidePrintedPerPrint = cost.LabourTimePerSidePrintedPerPrint;
            var percentageMarkUpRequired = cost.PercentageMarkUpRequired / 100;
            var printsPerLitre = cost.PrintsPerLitre;
            var count = soldcount;
            var argument1 = 1 + percentageMarkUpRequired;
            var argument2 = CntBackColor > 0 ? CntBackColor : 0;
            var argument3 = CntFrontColor > 0 ? CntFrontColor : 0;
            var argument4 = (float)labourTimePerSidePrintedPerPrint / 3600;
            var argument5 = CntBackColor + CntFrontColor;
            var argument6 = costOfMaterial + dTGPrintPrice;
            var argument7 = Math.Min(CntFrontColor, 1);
            var argument8 = Math.Max(0, CntFrontColor - 1);
            var argument9 = Math.Min(CntBackColor, 1);
            var argument10 = Math.Max(0, CntBackColor - 1);
            var argument11 = costOfMaterial * count;
            var argument12 = count * argument5;
            var argument13 = labourCost * labourTimePerColourPerPrint / 3600 * argument12;
            var argument14 = inkCost / printsPerLitre * argument12;
            var argument15_1 = argument3 + argument2;
            var argument15 = labourCost * argument4 * argument15_1 * count;
            var function1 = argument7 + argument9;
            var function2 = firstScreenCost * function1;
            var function3 = argument8 + argument10;
            var function4 = additionalScreenCosts * function3;
            var function5 = function2 + function4 + argument13 + argument14 + argument15 + argument11;
            var function6 = function5 / count;
            var function7 = argument6 > function6 ? function6 : argument6;
            var result = function7 * argument1;
            return (float)result;
        }

        public float CalculateVersion2(int campaignID, int productID, int soldcount)
        {
                var culture = _workContextAccessor.GetContext().CurrentCulture.Trim();
                //TShirtCostRecord cost = _costService.GetCost(culture);
                var campaign = _campaignService.GetCampaignById(campaignID);
                var product = campaign.Products.Where(aa => aa.Id == productID).First();
                if (soldcount >= campaign.ProductCountGoal) return (float)product.BaseCost;
                int CntBackColor = campaign.CntBackColor;
                int CntFrontColor = campaign.CntFrontColor;
                if (CntBackColor == 0 && CntFrontColor == 0) CntFrontColor = 1;
                else
                {
                    CntBackColor = campaign.CntBackColor;// == 0 ? 1 : campaign.CntBackColor;
                    CntFrontColor = campaign.CntFrontColor;// == 0 ? 1 : campaign.CntFrontColor;
                }
                TShirtCostRecord cost = (campaign.TShirtCostRecord != null) ? campaign.TShirtCostRecord : _costService.GetCost(culture);


                //var additionalScreenCosts = cost.AdditionalScreenCosts;
                //var costOfMaterial = product.ProductRecord.BaseCost;
                //var dTGPrintPrice = cost.DTGPrintPrice;
                //var firstScreenCost = cost.FirstScreenCost;
                //var inkCost = cost.InkCost;
                //var labourCost = cost.LabourCost;
                //var labourTimePerColourPerPrint = cost.LabourTimePerColourPerPrint;
                //var labourTimePerSidePrintedPerPrint = cost.LabourTimePerSidePrintedPerPrint;
                //var percentageMarkUpRequired = cost.PercentageMarkUpRequired / 100;
                //var printsPerLitre = cost.PrintsPerLitre;
            
                //var count = soldcount;


                double B3 = cost.FirstScreenCost;	//1st Screen Cost (RM)	
                double B4 = cost.AdditionalScreenCosts;	//Additional Screen Costs (RM)	
                double B5 = cost.InkCost;	//Ink Cost (RM per litre per colour)	
                double B6 = cost.PrintsPerLitre;	//Prints per litre	
                double B7 = cost.LabourCost;	//Labour Cost (RM per hr)	
                double B8 = cost.LabourTimePerColourPerPrint;	//Labour time per colour per print (seconds)	
                double B9 = cost.LabourTimePerSidePrintedPerPrint;	//Labour time per side printed per print (seconds)	
                double B10 = (product.CostOfMaterial == 0) ? product.ProductRecord.BaseCost : product.CostOfMaterial;	//Cost of material/ t-shirt (RM each)	
                double B11 = cost.PercentageMarkUpRequired / 100;	//Percentage Mark-Up required	
                double B12 = cost.DTGPrintPrice;	//DTG print price (RM)	

                double B14 = CntFrontColor;	//Number of colours (front)	
                double B15 = CntBackColor;	//Number of colours (back)	
                double B16 = soldcount;	//Quantity	

                var x = Math.Min(B10 + B12, (B3 * Math.Min(B14, 1) +
                        B4 * Math.Max(0, B14 - 1) + B3 * Math.Min(B15, 1) +
                        B4 * Math.Max(0, B15 - 1) +
                        B7 * B8 / 3600 * B16 * (B14 + B15) +
                        B5 / B6 * B16 * (B14 + B15) +
                        B7 * (B9 / 3600) * ((B14 > 0 ? 1 : 0) + (B15 > 0 ? 1 : 0)) * B16 + B10 * B16) / B16) * (1 + B11);

                return (float)x;
        }
        private void FillOverviews(CampaignsViewModel model,
            IQueryable<CampaignRecord> campaignsQuery)
        {
            // Today
            var today = DateTime.UtcNow.Date;
            var nextDay = today.AddDays(1);

            var yesterday = today.AddDays(-1);
            //return
                //query.Where(p => p.OrderRecord.Created.Date >= yesterday && p.OrderRecord.Created.Date < today);

            var campaingIndexes = model.Campaigns.Select(aa => aa.Id);

            var todayOrders = _orderService.GetAllOrders().Where(aa =>
                           aa.IsActive == true &&
                           aa.Created >= today &&
                           aa.Created.Date < nextDay &&
                           aa.OrderStatusRecord.Id != int.Parse(OrderStatus.Cancelled.ToString("d")) &&
                           aa.OrderStatusRecord.Id != int.Parse(OrderStatus.Pending.ToString("d")) &&
                           aa.OrderStatusRecord.Id != int.Parse(OrderStatus.Refunded.ToString("d")) &&
                           aa.Campaign.ProductCountSold >= aa.Campaign.ProductMinimumGoal);//.Select(aa => aa.TotalSold).Sum();
            double tmptodayProfit = 0;
            var todayProductsOrdered = 0; ///todayOrders.Select(aa => aa.TotalSold).ToArray().Sum();


            foreach (var order in todayOrders)
            {
                if (!campaingIndexes.Contains(order.Campaign.Id)) continue;
                foreach (var p in order.Products)
                {
                    if (p.OrderRecord.Email == null || p.OrderRecord.IsActive != true) continue;
                   // todayProductsOrdered += p.Count;
                    tmptodayProfit += _priceConversionService.ConvertPrice(p.Count *
                        (p.CampaignProductRecord.Price - CalculateVersion2(p.CampaignProductRecord.CampaignRecord_Id, p.CampaignProductRecord.Id, order.Campaign.ProductCountSold)), order.Campaign.CurrencyRecord, Module.Services.ExchangeRateFor.Seller).Value;
                    todayProductsOrdered += p.Count;
                }
            }

            var todayProfit = Math.Round(tmptodayProfit, 2);

            var todayCampaignsOverview = new CampaignsOverview
            {
                Type = OverviewType.Today,
                ProductsOrdered = todayProductsOrdered,
                Profit =  todayProfit,
                ToBeAllPaid = 0
            };

            model.Overviews.Add(todayCampaignsOverview);

            // Yesterday

            var yesterDayOrders = _orderService.GetAllOrders().Where(
                           aa => aa.IsActive == true && 
                           aa.Created.Date >= yesterday && 
                           aa.Created.Date < today && 
                           aa.OrderStatusRecord.Id != int.Parse(OrderStatus.Cancelled.ToString("d")) &&
                           aa.OrderStatusRecord.Id != int.Parse(OrderStatus.Pending.ToString("d")) &&
                           aa.OrderStatusRecord.Id != int.Parse(OrderStatus.Refunded.ToString("d")) &&
                           aa.Campaign.ProductCountSold >= aa.Campaign.ProductMinimumGoal);
            double tmpyesterdayProfit = 0;
            var yesterdayProductsOrdered = 0; // yesterDayOrders.Select(aa => aa.TotalSold).ToArray().Sum();
           

            foreach (var order in yesterDayOrders)
            {
                if (!campaingIndexes.Contains(order.Campaign.Id)) continue;
                foreach (var p in order.Products)
                {
                    tmpyesterdayProfit
                        +=
                        _priceConversionService.ConvertPrice
                        (p.Count * (p.CampaignProductRecord.Price - CalculateVersion2(p.CampaignProductRecord.CampaignRecord_Id, p.CampaignProductRecord.Id, order.Campaign.ProductCountSold)), order.Campaign.CurrencyRecord, Module.Services.ExchangeRateFor.Seller).Value;
                    yesterdayProductsOrdered += p.Count;
                }
            }

            var yesterdayCampaignsOverview = new CampaignsOverview
            {
                Type = OverviewType.Yesterday,
                ProductsOrdered = yesterdayProductsOrdered,
                Profit = Math.Round(tmpyesterdayProfit, 2),
                ToBeAllPaid = 0
            };

            model.Overviews.Add(yesterdayCampaignsOverview);

            // Active
          
            double activeProfit = 0;
            double tmpactiveProfit = 0;
            var activeProductsOrdered = 0; //activeOrderes.Select(aa => aa.TotalSold).Sum();



            foreach (var campaign in model.Campaigns)
            {
                if (!campaign.IsActive) continue;

                activeProductsOrdered += campaign.Sold; 
                tmpactiveProfit += campaign.Profit;
            }

            activeProfit = Math.Round(tmpactiveProfit, 2);

            var activeCampaignsOverview = new CampaignsOverview
            {
                Type = OverviewType.Active,
                ProductsOrdered = activeProductsOrdered,
                Profit = activeProfit,
                ToBeAllPaid = 0
            };

            model.Overviews.Add(activeCampaignsOverview);

            // AllTime
            double allTimeProfit = 0;
            double tmpallTimeProfit = 0;
            var allTimeProductsOrdered = 0;
            foreach (var campaign in model.Campaigns)
            {
                var campaginRecod = _campaignService.GetCampaignById(campaign.Id);
                if (campaginRecod.ProductCountSold >= campaginRecod.ProductMinimumGoal)
                {
                    tmpallTimeProfit += campaign.Profit;
                    allTimeProductsOrdered += campaign.Sold;
                }
            }

            allTimeProfit = Math.Round(tmpallTimeProfit, 2);

            var allTimeCampaignsOverview = new CampaignsOverview
            {
                Type = OverviewType.AllTime,
                ProductsOrdered = allTimeProductsOrdered,
                Profit = allTimeProfit,
                ToBeAllPaid = 0
            };

            model.Overviews.Add(allTimeCampaignsOverview);

            foreach (var item in model.Overviews)
            {
                item.Profit = item.Profit;// -item.ToBeAllPaid;
                if (item.Profit < 0)
                {
                }
            }
        }

        [Themed]
        [Authorize]
        public ActionResult EditCampaign(int id)
        {
            var camp = _campaignService.GetCampaignById(id);
            var user = Services.WorkContext.CurrentUser;
            if (camp.TeeyootUserId != user.Id)
            {
                return View("EditCampaign", new EditCampaignViewModel { IsError = true });
            }

            var allTags = _campaignService.GetAllCategories()
                .Select(t => new TagViewModel { name = t.Name })
                .ToList();

            var tags = _campaignCategoryService.GetCategoryByCampaignId(camp.Id)
                .Select(t => t.Name)
                .ToList();

            var product = _campaignService.GetProductsOfCampaign(id).First(c => c.WhenDeleted == null).Id;

            var path = "/Media/campaigns/" + camp.Id + "/" + product + "/";
            string backImg;
            string frontImg;
            if (camp.BackSideByDefault)
            {
                backImg = path + "normal/front.png";
                frontImg = path + "normal/back.png";
            }
            else
            {
                backImg = path + "normal/back.png";
                frontImg = path + "normal/front.png";
            }

            var editCampaignViewModel = new EditCampaignViewModel
            {
                IsError = false,
                Id = camp.Id,
                Title = camp.Title,
                Description = camp.Description,
                AllTags = allTags,
                Tags = tags,
                Alias = camp.Alias,
                BackSideByDefault = camp.BackSideByDefault,
                FrontImagePath = frontImg,
                BackImagePath = backImg,
                FBPixelId = camp.FBPixelId,
                GooglePixelId = camp.GooglePixelId,
                PinterestPixelId = camp.PinterestPixelId
            };

            return View("EditCampaign", editCampaignViewModel);
        }

        public ActionResult SaveChanges(EditCampaignViewModel editCampaign)
        {
            var campaign = _campaignService.GetCampaignById(editCampaign.Id);

            campaign.Title = editCampaign.Title;
            campaign.Description = editCampaign.Description;
            //campaign.Alias = editCampaign.Alias;
            campaign.BackSideByDefault = editCampaign.BackSideByDefault;


            campaign.FBPixelId = editCampaign.FBPixelId;
            campaign.GooglePixelId = editCampaign.GooglePixelId;
            campaign.PinterestPixelId = editCampaign.PinterestPixelId;



            var campaignTags = _linkCampaignAndCategoryRepository.Table
                .Where(t => t.CampaignRecord == campaign)
                .ToList();

            // Delete existing campaign tags
            foreach (var campaignTag in campaignTags)
            {
                _linkCampaignAndCategoryRepository.Delete(campaignTag);
            }

            // Create new campaign tags
            string[] tagsToSave = { };
            if (editCampaign.TagsToSave != null)
            {
                tagsToSave = editCampaign.TagsToSave.Split(',');
            }

            foreach (var tagToSave in tagsToSave)
            {
                var tag = _campaignCategoryRepository.Table
                    .FirstOrDefault(t => t.Name.ToLowerInvariant() == tagToSave.ToLowerInvariant());

                if (tag == null)
                {
                    tag = new CampaignCategoriesRecord
                    {
                        Name = tagToSave,
                        IsVisible = false,
                        CategoriesCulture = cultureUsed
                    };

                    _campaignCategoryRepository.Create(tag);
                }

                var campaignTag = new LinkCampaignAndCategoriesRecord
                {
                    CampaignRecord = campaign,
                    CampaignCategoriesPartRecord = tag
                };

                _linkCampaignAndCategoryRepository.Create(campaignTag);
            }

            _notifier.Information(T("Campaign was updated successfully"));
            return RedirectToAction("Campaigns");
        }

        public JsonResult GetDetailTags(string filter)
        {
            int filterNull = filter.LastIndexOf(' ');
            if (filterNull == filter.Length - 1)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            string[] filters = filter.Split(' ');
            string tag = filters[filters.Length - 1];

            var entries =
                _campaignService.GetAllCategories()
                    .Where(c => c.Name.Contains(tag))
                    .Select(n => n.Name)
                    .Take(10)
                    .ToList();
            return Json(entries, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Wizard(int id)
        {
            return RedirectToAction("Index", "Wizard", new RouteValueDictionary
            {
                {"id", id},
                {"area", "Teeyoot.Module"},
                {"controller", "Wizard"},
                {"action", "Index"}
            });
        }

        public ActionResult DeleteCampaign(int id)
        {
            if (_campaignService.DeleteCampaign(id))
            {
                _notifier.Information(T("The campaign was deleted successfully!"));
            }
            else
            {
                _notifier.Error(T("The company could not be removed. Try again!"));
            }

            return RedirectToAction("Campaigns");
        }

        public ActionResult PrivateCampaign(int id, bool change)
        {
            if (_campaignService.PrivateCampaign(id, change))
            {
                if (change)
                {
                    _notifier.Information(T("Campaign set status - private"));
                }
                else
                {
                    _notifier.Information(T("Campaign set status - public"));
                }
            }
            else
            {
                _notifier.Error(T("The company could not be changed. Try again!"));
            }

            return RedirectToAction("Campaigns");
        }

        [HttpGet]
        public JsonResult GetDataForReLaunch(int id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            var products = _campaignService.GetProductsOfCampaign(id);
            var result = new RelaunchCampaignsViewModel();
            List<object> prodInfo = new List<object>();
            foreach (var product in products)
            {
                var prodRec = _productService.GetProductById(product.ProductRecord.Id);
                prodInfo.Add(
                    new
                    {
                        Price = product.Price,
                        BaseCostForProduct = prodRec.BaseCost,
                        ProductId = prodRec.Id,
                        BaseCost = product.BaseCost
                    });
            }

            var tShirtCostRecord = _tshirtService.GetCost(cultureUsed);

            result.Products = prodInfo.ToArray();
            result.CntBackColor = campaign.CntBackColor;
            result.CntFrontColor = campaign.CntFrontColor;
            result.TShirtCostRecord = tShirtCostRecord;
            result.ProductCountGoal = campaign.ProductCountGoal;

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpStatusCodeResult ReLaunchCampaign(int productCountGoal, string campaignProfit, int campaignLength,
            int minimum, RelaunchProductInfo[] baseCost, int id)
        {
            var newCampaign = _campaignService.ReLaunchCampiagn(productCountGoal, campaignProfit, campaignLength,
                minimum, baseCost, id);

            CreateImagesForCampaignProducts(newCampaign);

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        private void CreateImagesForCampaignProducts(CampaignRecord campaign)
        {
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
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
                        p.Id.ToString() + "_" + p.SecondProductColorRecord.Id.ToString(), p, data, frontPath, backPath,
                        p.SecondProductColorRecord.Value);
                }
                if (p.ThirdProductColorRecord != null)
                {
                    CreateImagesForOtherColor(campaign.Id,
                        p.Id.ToString() + "_" + p.ThirdProductColorRecord.Id.ToString(), p, data, frontPath, backPath,
                        p.ThirdProductColorRecord.Value);
                }
                if (p.FourthProductColorRecord != null)
                {
                    CreateImagesForOtherColor(campaign.Id,
                        p.Id.ToString() + "_" + p.FourthProductColorRecord.Id.ToString(), p, data, frontPath, backPath,
                        p.FourthProductColorRecord.Value);
                }
                if (p.FifthProductColorRecord != null)
                {
                    CreateImagesForOtherColor(campaign.Id,
                        p.Id.ToString() + "_" + p.FifthProductColorRecord.Id.ToString(), p, data, frontPath, backPath,
                        p.FifthProductColorRecord.Value);
                }

                int product = _campaignService.GetProductsOfCampaign(campaign.Id).First().Id;
                string destFolder = Path.Combine(Server.MapPath("/Media/campaigns/"), campaign.Id.ToString(),
                    product.ToString(), "social");
                Directory.CreateDirectory(destFolder);

                var imageSocialFolder = Server.MapPath("/Modules/Teeyoot.Module/Content/images/");
                if (!campaign.BackSideByDefault)
                {
                    var frontSocialPath = Path.Combine(imageSocialFolder,
                        "product_type_" + p.ProductRecord.Id + "_front.png");
                    var imgPath = new Bitmap(frontSocialPath);

                    _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, data.Front);
                }
                else
                {
                    var backSocialPath = Path.Combine(imageSocialFolder,
                        "product_type_" + p.ProductRecord.Id + "_back.png");
                    var imgPath = new Bitmap(backSocialPath);

                    _imageHelper.CreateSocialImg(destFolder, campaign, imgPath, data.Back);
                }
            }
        }

        public void CreateImagesForOtherColor(int campaignId, string prodIdAndColor, CampaignProductRecord p,
            DesignInfo data, string frontPath, string backPath, string color)
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
                p.ProductRecord.ProductImageRecord.Width * 4, p.ProductRecord.ProductImageRecord.Height * 4,
                p.ProductRecord.ProductImageRecord.PrintableFrontTop * 4,
                p.ProductRecord.ProductImageRecord.PrintableFrontLeft * 4,
                p.ProductRecord.ProductImageRecord.PrintableFrontWidth * 4,
                p.ProductRecord.ProductImageRecord.PrintableFrontHeight * 4);

            var backZoom = BuildProductImage(backTemplate, _imageHelper.Base64ToBitmap(data.Back), rgba,
                p.ProductRecord.ProductImageRecord.Width * 4, p.ProductRecord.ProductImageRecord.Height * 4,
                p.ProductRecord.ProductImageRecord.PrintableBackTop * 4,
                p.ProductRecord.ProductImageRecord.PrintableBackLeft * 4,
                p.ProductRecord.ProductImageRecord.PrintableBackWidth * 4,
                p.ProductRecord.ProductImageRecord.PrintableBackHeight * 4);

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
            image = _imageHelper.ApplyBackground(image, background, color, width, height);
            return _imageHelper.ApplyDesign(image, design, printableAreaTop, printableAreaLeft, printableAreaWidth,
                printableAreaHeight, color, width, height);
        }
    }
}