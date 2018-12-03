using IpToCountry;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teeyoot.Module.Models;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface IMultiCountryService : IDependency
    {
        CurrencyRecord GetCurrency();
        CurrencyRecord GetDefaultCurrecny();
        CountryRecord GetDefaultCountry();
        CountryRecord GetCountry();
        IpAddressLocation GetCountryCode();
    }
}
