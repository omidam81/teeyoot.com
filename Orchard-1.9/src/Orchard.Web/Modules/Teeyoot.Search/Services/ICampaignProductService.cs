using System.Collections.Generic;
using Orchard;
using Teeyoot.Module.Models;

namespace Teeyoot.Search.Services
{
    public interface ICampaignProductService : IDependency
    {
        List<CampaignProductRecord> GetCampaignProductsByCampaign(IEnumerable<int> campaignIds);
    }
}
