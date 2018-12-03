using Ionic.Zip;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Services
{
    public class ExportExcelService : IExportExcelService
    {
        private readonly IOrderService _orderService;
        private readonly ICampaignService _campaignServiec;
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;

        public ExportExcelService(IOrderService service, ICampaignService campaignServiec, IOrchardServices orchardServices, IContentManager contentManager)
        {
            _orderService = service;
            _campaignServiec = campaignServiec;
            _orchardServices = orchardServices;
            _contentManager = contentManager;
        }
        public string ExportOrderToInvoiceExcelFile(int orderID)
        {
            var order = _orderService.GetOrderById(orderID);
            
            if(order.Products.Count <= 0 || order.Products.First().CampaignProductRecord == null) throw new Exception("Order Record is not Valid!");

            var campaign = _campaignServiec.GetCampaignById(order.Products.First().CampaignProductRecord.CampaignRecord_Id);


            ///
            ///TODO: Chnage later to read from setting. 
            ///
            //string templateAddress = Path.Combine(Server.MapPath());
            string template = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/Templates/Invoice.xlsx");
            string newFileName =
                string.Format("TYT-{0}-{1}-{2}-{3}-{4}-{5}-{6}{7}.xlsx",
                order.Created.ToShortDateString().Replace('.', '-').Replace("/", "-").Replace("\\", "-"),
                order.Id, 
                order.OrderPublicId ,
                order.Email.Replace("@", "--"),
                order.Products.First().CampaignProductRecord.CampaignRecord_Id,
                campaign.Alias,
                order.CurrencyRecord.Name,
                order.TotalPrice);
            newFileName = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/tmp/" + newFileName);

            using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(new FileInfo(newFileName), new FileInfo(template)))
            {
                ExcelWorkbook myWorkbook = package.Workbook;
                ExcelWorksheet myWorksheet = myWorkbook.Worksheets[myWorkbook.Worksheets.First().Name];
                myWorksheet.Cells[11, 1].Value = order.Created.ToShortDateString();
                myWorksheet.Cells[11, 2].Value = order.OrderPublicId;
                myWorksheet.Cells[11, 3].Value = campaign.Alias;
                myWorksheet.Cells[11, 4].Value = campaign.Id;
                myWorksheet.Cells[11, 5].Value = order.Email;
                myWorksheet.Cells[13, 5].Value = order.PaymentMethod; //(order.Paid == null) ? "COD" : "Online Banking";
                myWorksheet.Cells[15, 5].Value = (order.CurrencyRecord != null) ? order.CurrencyRecord.Name : "";


                myWorksheet.Cells[13, 2].Value = order.FirstName;
                myWorksheet.Cells[14, 2].Value = order.LastName;
                myWorksheet.Cells[15, 2].Value = order.StreetAddress;
                myWorksheet.Cells[16, 2].Value = order.City;
                myWorksheet.Cells[17, 2].Value = order.PostalCode;
                myWorksheet.Cells[18, 2].Value = order.State;
                myWorksheet.Cells[19, 2].Value = order.Country;
                myWorksheet.Cells[20, 2].Value = order.PhoneNumber;

                var totalprice = order.TotalPriceWithPromo;

                double realprice = 0;
                double ratio = 1;

                if (totalprice != 0)
                {
                    foreach (var product in order.Products)
                    {
                        realprice += (product.Count * product.CampaignProductRecord.Price);
                    }

                    ratio = totalprice / realprice;
                }
                


                int i = 23;




                var DC = order.Delivery;
                var nMethod = false;

                if (order.Created.Year > 2015 && order.Created.Month >= 2 && order.Created.Day > 18)
                {
                    DC = order.Delivery / (1 + (order.TotalSold - 1) / 2);
                    nMethod = true;

                }
                foreach (var product in order.Products)
                {
                    //"Style Name", "Colour", "Size", "Unit Price", "Delivery Cost
                    myWorksheet.Cells[i, 1].Value = product.CampaignProductRecord.ProductRecord.Name;//"Style Name
                    myWorksheet.Cells[i, 2].Value = product.ProductColorRecord.Name != null ? product.ProductColorRecord.Name : "";//"Colour
                    myWorksheet.Cells[i, 3].Value = (product.ProductSizeRecord != null) ? product.ProductSizeRecord.SizeCodeRecord.Name : "";//"Size
                    myWorksheet.Cells[i, 4].Value = product.Count;//count
                    myWorksheet.Cells[i, 5].Value = product.CampaignProductRecord.Price;//"Unit Price;
                    if (i == 23)
                    {
                        if (nMethod) myWorksheet.Cells[i, 6].Value = order.Delivery + ((product.Count - 1) * order.Delivery / 2);//"Unit Price;
                        else myWorksheet.Cells[i, 6].Value = order.Delivery;
                    }
                    else
                    {
                        if (nMethod)
                            myWorksheet.Cells[i, 6].Value = order.Delivery / 2 * product.Count;
                        else
                            myWorksheet.Cells[i, 6].Value = 0; /// order.Delivery / 2 * product.Count;

                    }
                    //if (product.ProductColorRecord != null && product.ProductSizeRecord != null)
                    //{
                    //    if (i == 23)
                    //    {
                    //        myWorksheet.Cells[i, 5].Value = ratio * product.CampaignProductRecord.Price + order.Delivery;
                    //    }
                    //    else
                    //    {
                    //        myWorksheet.Cells[i, 5].Value = ratio * product.CampaignProductRecord.Price;
                    //    }
                    //}

                    i++;
                }


                package.Save();
            }
            
            return newFileName;
        }

        public string ExportCampaign(int campaignId)
        {


            var _campaign = _campaignServiec.GetCampaignById(campaignId);
            
            
            string template = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/Templates/campaign.xlsx");
            string newFileName =
                string.Format("TYT-{0}-{1}{2}-Items.xlsx",
                DateTime.Now.ToShortDateString().Replace('.', '-').Replace("/", "-").Replace("\\", "-"),
                campaignId,
                _campaign.Alias);
            newFileName = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/tmp/" + newFileName);

            using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(new FileInfo(newFileName), new FileInfo(template)))
            {

                ExcelWorkbook myWorkbook = package.Workbook;
                ExcelWorksheet myWorksheet = myWorkbook.Worksheets[myWorkbook.Worksheets.First().Name];

                myWorksheet.Cells[8, 2].Value = campaignId;
                myWorksheet.Cells[9, 2].Value = _campaign.Alias;

                var seller = _contentManager.Query<UserPart, UserPartRecord>()
                        .Where(user => user.Id == _campaign.TeeyootUserId)
                        .List()
                        .First();

                var xuser = _contentManager.Get<TeeyootUserPart>(seller.Id, VersionOptions.Latest);

                myWorksheet.Cells[10, 2].Value = xuser.PublicName;
                myWorksheet.Cells[11, 2].Value = seller.Email;
                myWorksheet.Cells[12, 2].Value = xuser.PhoneNumber;
                myWorksheet.Cells[13, 2].Value = _campaign.URL;
                myWorksheet.Cells[14, 2].Value = _campaign.WhenApproved.HasValue ? _campaign.WhenApproved.Value.ToShortDateString() : "";
                myWorksheet.Cells[15, 2].Value = _campaign.EndDate.ToShortDateString();
                myWorksheet.Cells[16, 2].Value = _campaign.CampaignProfit;
                myWorksheet.Cells[17, 2].Value = _campaign.ProductCountGoal;
                myWorksheet.Cells[18, 2].Value = _campaign.ProductMinimumGoal;



                int left = 4;
                foreach (var p in _campaign.Products.Where(aa => aa.WhenDeleted == null))
                {
                    if (left > 8)
                    {
                        for (int i = 8; i < 19; i++)
                        {
                            for (int k = left; k < left + 3; k++)
                            {
                                using (var rng = myWorksheet.Cells[i, k])
                                {

                                    //rng.Style.Font.Bold = true;
                                    //rng.Style.WrapText = true;
                                    //rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    //rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    //rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    //rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(237, 237, 237));
                                    //rng.Style.Font.Size = 12;
                                    //rng.Style.Border =
                                    //rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;


                                }



                                //myWorksheet.Cells[i, k].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                //myWorksheet.Cells[i, k].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                //myWorksheet.Cells[i, k].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                //myWorksheet.Cells[i, k].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                        }
                        if (!myWorksheet.Cells[8, left, 8, left + 2].Merge)
                            myWorksheet.Cells[8, left, 8, left + 2].Merge = true;

                        //myWorksheet.Cells[].Style.Border.Left.Color 

                        for (int j = 9; j < 18; j++)
                        {
                            if (!myWorksheet.Cells[left + 1, j, left + 2, j].Merge)
                                myWorksheet.Cells[j, left + 1, j, left + 2].Merge = true;
                        }


                        myWorksheet.Cells[9, left].Value = "Selling Price";
                        myWorksheet.Cells[10, left].Value = "Cost Price";
                        myWorksheet.Cells[11, left].Value = "Profit";
                        myWorksheet.Cells[12, left].Value = "Total Tees";
                        myWorksheet.Cells[13, left].Value = "Paid Online (Tshirts)";
                        myWorksheet.Cells[14, left].Value = "Profits for online purchase";
                        myWorksheet.Cells[15, left].Value = "COD (Tshirts)";
                        myWorksheet.Cells[16, left].Value = "Total profits for COD";
                        myWorksheet.Cells[17, left].Value = "Total Costs of Tshirt";

                    }

                    //product name
                    myWorksheet.Cells[8, left].Value = p.ProductRecord.Name;


                    myWorksheet.Cells[9, left + 1].Value = p.Price;
                    myWorksheet.Cells[10, left + 1].Value = p.BaseCost;
                    myWorksheet.Cells[11, left + 1].Value = p.Price - p.BaseCost;
                    left++;
                    left++;
                    left++;
                }




                
                
                
                var top = 26;

                foreach (var order in _orderService.GetOrdersByCampaignID(campaignId)) //_orderService.GetAllOrders().Where(aa => aa.Email != null && aa.Products != null && aa.Products.Count > 0 && aa.Products.First().CampaignProductRecord.CampaignRecord_Id == campaignId))
                {
                    if (order.Email == null) continue;
                    if (!(order.OrderStatusRecord.Name.ToLower() == "approved" ||
                       order.OrderStatusRecord.Name.ToLower() == "printing" ||
                       order.OrderStatusRecord.Name.ToLower() == "shipped" ||
                       order.OrderStatusRecord.Name.ToLower() == "delivered")) continue;
                    var product_count = order.Products.Count;
                    var deliveryadded = false;

                    if (product_count <= 0) continue;

                    double totalprice = 0;
                    //order.Delivery
                    double realprice = 0;
                    double ratio = 1;
                    if (order.TotalPriceWithPromo != 0)
                    {
                        totalprice = order.TotalPriceWithPromo;
                        foreach (var product in order.Products)
                        {
                            realprice += product.CampaignProductRecord.Price * product.Count;
                        }

                       ratio = totalprice / realprice;
                    }
                    


                    foreach (var product in order.Products)
                    {
                        if (product.ProductColorRecord == null) continue;

                        myWorksheet.Cells[top, 1].Value = order.OrderPublicId;
                        myWorksheet.Cells[top, 2].Value = order.Created.ToShortDateString();
                        myWorksheet.Cells[top, 3].Value = product.CampaignProductRecord.ProductRecord.Name;
                        myWorksheet.Cells[top, 4].Value = product.ProductColorRecord.Name;
                        myWorksheet.Cells[top, 5].Value = product.ProductSizeRecord.SizeCodeRecord.Name;// order.Products.First().CampaignProductRecord.ProductColorRecord.Name;
                        myWorksheet.Cells[top, 6].Value = product.Count;

                        if (!deliveryadded)
                        {
                            myWorksheet.Cells[top, 7].Value = ratio * product.CampaignProductRecord.Price * product.Count + order.Delivery; // product.CampaignProductRecord.BaseCost;
                            myWorksheet.Cells[top, 8].Value = order.Delivery;
                            deliveryadded = true;
                        }
                        else
                        {
                            myWorksheet.Cells[top, 7].Value = ratio * product.CampaignProductRecord.Price * product.Count; // product.CampaignProductRecord.BaseCost;
                            myWorksheet.Cells[top, 8].Value = "0";

                        }
                        //myWorksheet.Cells[top, 7].Value = ratio.ToString() + ":" + order.TotalPrice + ":" + (product.CampaignProductRecord.Price * product.Count + order.Delivery).ToString() + ":" +realprice.ToString(); // product.CampaignProductRecord.BaseCost;

                        myWorksheet.Cells[top, 9].Value = order.PaymentMethod; //(order.Paid == null) ? "COD" : "Online Banking";// product.CampaignProductRecord.BaseCost;
                        
                        top++;
                    }
                }

                package.Save();
            }


            return newFileName;
        }


        public string ExpotZip(IQueryable<Models.OrderRecord> orders)
        {
            if (orders == null || orders.Count() == 0) return "";

            List<string> files = new List<string>();
            foreach (var item in orders)
            {
                files.Add(ExportOrderToInvoiceExcelFile(item.Id));
            }

            var zipFilename = DateTime.Now.Ticks.ToString() + "_" + Guid.NewGuid().ToString() + ".zip";
            zipFilename = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/tmp/" + zipFilename);




            using (Ionic.Zip.ZipFile Z = new Ionic.Zip.ZipFile())
            {
                foreach (string fileName in files)
                {
                    Z.AddFile(fileName);
                }
                Z.Save(zipFilename);
            }
            return zipFilename;
        }

        public string ExportCampaign2(int campaignID)
        {
            var _campaign = _campaignServiec.GetCampaignById(campaignID);

            var _orders = _orderService.GetOrdersByCampaignID(campaignID);


            var max_k = 0;

            string template = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/Templates/campaign2.xlsx");
            string newFileName =
                string.Format("TYT-{0}-{1}{2}-Items-" + Guid.NewGuid().ToString() + ".xlsx",
                DateTime.Now.ToShortDateString().Replace('.', '-').Replace("/", "-").Replace("\\", "-"),
                campaignID,
                _campaign.Alias);
            newFileName = System.Web.HttpContext.Current.Server.MapPath("/Media/Excel/tmp/" + newFileName);

            using (OfficeOpenXml.ExcelPackage package = new OfficeOpenXml.ExcelPackage(new FileInfo(newFileName)))
            {

                ExcelWorkbook myWorkbook = package.Workbook;
                ExcelWorksheet myWorksheet = myWorkbook.Worksheets.Add("Total to purchase");//myWorkbook.Worksheets[myWorkbook.Worksheets.First().Name];

                myWorksheet.Cells[2, 1].Value = "Campaign Name";
                myWorksheet.Cells[3, 2].Value = "Campaign ID";
                myWorksheet.Cells[2, 2].Value = _campaign.Title;
                myWorksheet.Cells[3, 2].Value = campaignID;

                int left = 1;


                foreach (var product in _campaign.Products)
                {
                    var product_count = product.ProductRecord.ColorsAvailable.Count();
                    //myWorksheet.Cells[6, left, 6, left + product_count + 1].Merge = false;
                    //myWorksheet.Cells[6, left, 6, left + product_count + 1].Merge = true;
                    myWorksheet.Cells[6, left, 6, left + product_count].Value = product.ProductRecord.Name;
                    int j = 1;
                    myWorksheet.Cells[7, left].Value = "Sizes";

                    foreach (var productColor in product.ProductRecord.ColorsAvailable)
                    {
                        myWorksheet.Cells[7, j + left].Value = productColor.ProductColorRecord.Name;

                        j++;
                    }

                    int k = 8;

                    foreach (var productSize in product.ProductRecord.SizesAvailable)
                    {
                        myWorksheet.Cells[k, left].Value = productSize.ProductSizeRecord.SizeCodeRecord.Name;
                        k++;
                    }

                    myWorksheet.Cells[k, left].Value = "Total";
                    if (max_k < k) max_k = k + 1;

                    int j1 = 1;
                    var alltottal = 0;
                    foreach (var product_color in product.ProductRecord.ColorsAvailable)
                    {
                        int k1 = 8;
                        var total1 = 0;
                        foreach (var product_size in product.ProductRecord.SizesAvailable)
                        {
                            //var orders =
                            //    _orders.Where(bb => (bb.OrderStatusRecord.Name.ToLower() == "approved" ||
                            //        bb.OrderStatusRecord.Name.ToLower() == "printing" ||
                            //        bb.OrderStatusRecord.Name.ToLower() == "shipped" ||
                            //        bb.OrderStatusRecord.Name.ToLower() == "delivered"));


                            //orders = orders.Where(aa => aa.Products.Any(bb => bb.CampaignProductRecord.Id == product.Id));
                            //orders = orders.Where(aa => aa.Email != null && aa.IsActive == true);

                            var allProduct = 0;
                            if (_orders.Count() > 0)
                            {
                                foreach (var order in _orders)
                                {
                                    if (" approved, printing, shipped, delivered".IndexOf(order.OrderStatusRecord.Name.ToLower()) >= 0)
                                    {
                                        foreach (var p in order.Products)
                                        {

                                            if (p.ProductColorRecord != null)
                                                if (p.CampaignProductRecord.Id == product.Id && p.ProductColorRecord.Id == product_color.ProductColorRecord.Id && p.ProductSizeRecord.Id == product_size.ProductSizeRecord.Id)
                                                {
                                                    allProduct += p.Count;
                                                }
                                        }
                                    }
                                }
                            }
                            myWorksheet.Cells[k1, j1 + left].Value = allProduct;
                            total1 += allProduct;
                            k1++;
                        }
                        myWorksheet.Cells[k1, j1 + left].Value = total1;
                        alltottal += total1;
                        j1++;
                    }

                    myWorksheet.Cells[max_k + 1, left].Value = "Grand Total";
                    myWorksheet.Cells[max_k + 1, left + 1].Value = alltottal;
                    left += (product_count + 1);


                }
                package.Save();
            }

            return newFileName;
        }
    
    }
}