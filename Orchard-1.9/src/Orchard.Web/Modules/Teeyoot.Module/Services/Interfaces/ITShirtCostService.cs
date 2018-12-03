using Orchard;
using Teeyoot.Module.Models;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface ITShirtCostService : IDependency
    {
        TShirtCostRecord GetCost(string culture);
        bool UpdateCost(TShirtCostRecord cost);
        bool InsertCost(TShirtCostRecord cost);
    }
}
