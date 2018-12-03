using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace RM.Localization
{
    // Used especcialy to inject data from Teeyoot.Module
    //  to avoid circular modules dependency.

    public interface ICurrentUserCulturesProvider : IDependency
    {
        List<string> GetCulturesForCurrentUser();
    }
}
