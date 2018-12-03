using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.FAQ;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;
using Orchard.Data;
using Orchard.Localization.Records;


namespace Teeyoot.Module.Common
{
    // Created especcialy for Teeyoot.FAQ.FaqAdminController
    //  to pass there the data for "Coutry-Culture dropdowns".

    public class CountryCulturesProvider : ICountryCulturesProvider
    {
        private readonly ICountryService _countryService;
        private readonly IRepository<CultureRecord> _cultureRepository;

        public CountryCulturesProvider(
            ICountryService countryService,
            IRepository<CultureRecord> cultureRepository
            )
        {
            _countryService = countryService;
            _cultureRepository = cultureRepository;
        }



        CountryCulturesProviderData ICountryCulturesProvider.GetData(int forCountryId, int forCultureId)
        {
            var data = new CountryCulturesProviderData() { Culture = "" };

            data.Countries = _countryService.GetAllCountry().Select(c => new KeyValuePair<int, string>(c.Id, c.Name )).ToList();
            if (forCountryId > 0)
            {
                data.Cultures = _countryService.GetCultureByCountry(forCountryId).Select(c => new KeyValuePair<int, string>(c.Id, c.Culture)).ToList();
                data.CountryId = forCountryId;
                if (forCultureId == 0)
                {
                    data.CultureId = data.Cultures.First().Key;
                }
                else
                {
                    data.CultureId = forCultureId;
                }
            }
            else
            {
                if (data.Countries.Count > 0)
                {
                    data.Cultures = _countryService.GetCultureByCountry(data.Countries.First().Key).Select(c => new KeyValuePair<int, string>(c.Id, c.Culture)).ToList();
                    data.CountryId = data.Countries.First().Key;
                    if (data.Cultures.Count > 0)
                    {
                        data.CultureId = data.Cultures.First().Key;
                    }
                }
            }
            var cultureRecord = _cultureRepository.Get(data.CultureId);
            if (cultureRecord != null)
            {
                data.Culture = cultureRecord.Culture;
            }

            return data;
        }


        public List<KeyValuePair<int, string>> GetCountryCultures(int countryId)
        {
            return _countryService.GetCultureByCountry(countryId).Select(c => new KeyValuePair<int, string>(c.Id, c.Culture)).ToList();
        }
    }

}