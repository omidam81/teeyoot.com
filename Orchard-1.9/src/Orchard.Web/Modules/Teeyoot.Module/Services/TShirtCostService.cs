using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.Models;
using Orchard.Data;

namespace Teeyoot.Module.Services
{
    public class TShirtCostService : ITShirtCostService
    {
        private readonly IRepository<TShirtCostRecord> _costRepository;

        public TShirtCostService(IRepository<TShirtCostRecord> costRepository)
        {
            _costRepository = costRepository;
        }

        public TShirtCostRecord GetCost(string culture)
        {
            return _costRepository.Table.Where(c => c.CostCulture == culture).OrderByDescending(aa => aa.CostRevision).FirstOrDefault();
        }

        public bool UpdateCost(TShirtCostRecord cost)
        {
            int new_revisision = cost.CostRevision+1;

            TShirtCostRecord tshirt = new TShirtCostRecord()
            {
                AdditionalScreenCosts = cost.AdditionalScreenCosts,
                CostCulture = cost.CostCulture,
                CostRevision = new_revisision,
                DateCreated = DateTime.UtcNow,
                DTGPrintPrice = cost.DTGPrintPrice,
                FirstScreenCost = cost.FirstScreenCost,
                InkCost = cost.InkCost,
                LabourCost = cost.LabourCost,
                LabourTimePerColourPerPrint = cost.LabourTimePerColourPerPrint,
                LabourTimePerSidePrintedPerPrint= cost.LabourTimePerSidePrintedPerPrint,
                MaxColors = cost.MaxColors,
                PercentageMarkUpRequired = cost.PercentageMarkUpRequired,
                PrintsPerLitre = cost.PrintsPerLitre,
                SalesGoal = cost.SalesGoal
            };

            return InsertCost(tshirt);
        }

        public bool InsertCost(TShirtCostRecord cost)
        {
            //cost.CostRevision = 0;
            cost.DateCreated = DateTime.UtcNow;

            try
            {
                _costRepository.Create(cost);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}