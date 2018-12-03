using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;

namespace Teeyoot.FAQ
{
    // Used especcialy to inject data from Teeyoot.Module
    //  to avoid circular modules dependency.

    public interface ICountryCulturesProvider : IDependency
    {
        CountryCulturesProviderData GetData(int forCountryId, int forCultureId);

        List<KeyValuePair<int, string>> GetCountryCultures(int countryId);
    }

    public class CountryCulturesProviderData
    {
        public int CountryId { get; set; }
        public int CultureId { get; set; }

        public List<KeyValuePair<int, string>> Countries { get; set; }
        public List<KeyValuePair<int, string>> Cultures { get; set; }

        public string Culture { get; set; }
    }
}