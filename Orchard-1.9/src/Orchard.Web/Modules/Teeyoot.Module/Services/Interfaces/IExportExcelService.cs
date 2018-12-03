using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface IExportExcelService : IDependency
    {
        /// <summary>
        /// Get Order Id and return Path of Generated Excel file
        /// </summary>
        /// <param name="orderID">Order Record ID</param>
        /// <returns>File Address</returns>
        string ExportOrderToInvoiceExcelFile(int orderID);

        string ExportCampaign(int campaignId);

        string ExpotZip(IQueryable<Models.OrderRecord> orders);

        string ExportCampaign2(int campaignID);

    }
}
