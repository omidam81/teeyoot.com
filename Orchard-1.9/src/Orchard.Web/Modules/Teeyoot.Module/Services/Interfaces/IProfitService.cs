using System.Collections.Generic;
using System.Linq;
using Orchard;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Messaging.CampaignService;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;
using Orchard.Data;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface IProfitService : IDependency
    {
        //IRepository<CampaignRecord> _campaings;

        double CalculateProfitForCampain();
        double CalculateProfitForProduct();
        double CalCulateProfitForOrder();
        double PriceCalculator(double baseprice, int soldCount, int target);
    }
}
