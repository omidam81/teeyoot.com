using System.Linq;
using Orchard.Data;
using Orchard.Localization.Records;
using RM.Localization.Services;
using Teeyoot.Localization;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;
using System;

namespace Teeyoot.Module.Services
{
    public class CountryService : ICountryService
    {
        private readonly IRepository<CountryRecord> _countryRepository;
        private readonly IRepository<LinkCountryCultureRecord> _linkCountryCultureRecord;
        private readonly ICultureService _cultureService;
        private readonly IRepository<LinkCountryCurrencyRecord> _linkCountryCurrencyRecord;
        private readonly IRepository<CurrencyRecord> _currencyRecord;
        
        private const string UnitedStatesCountryIsoCode = "US";

        public CountryService(
            IRepository<CountryRecord> countryRepository,
            IRepository<LinkCountryCultureRecord> linkCountryCultureRecord,
            ICultureService cultureService,
            IRepository<LinkCountryCurrencyRecord> linkCountryCurrencyRecord,
            IRepository<CurrencyRecord> currencyRecord)
        {
            _countryRepository = countryRepository;
            _linkCountryCultureRecord = linkCountryCultureRecord;
            _cultureService = cultureService;
            _linkCountryCurrencyRecord = linkCountryCurrencyRecord;
            _currencyRecord = currencyRecord;
        }

        public IQueryable<CountryRecord> GetAllCountry()
        {
            return _countryRepository.Table;
        }

        public CurrencyRecord GetCurrencyByCulture(string culture, string xcurrency)
        {
            var cult = _cultureService.ListCultures().Where(c => c.Culture == culture).FirstOrDefault();
            if (cult == null)
            {
                cult = _cultureService.ListCultures().Where(c => c.Culture == "en-MY").First();
            }

            var country =
                _linkCountryCultureRecord.Table.Where(l => l.CultureRecord.Culture == cult.Culture)
                    .Select(l => l.CountryRecord)
                    .First();
            var currency = (string.IsNullOrWhiteSpace(xcurrency))?
                _linkCountryCurrencyRecord.Table.Where(u => u.CountryRecord.Id == country.Id)
                    .Select(u => u.CurrencyRecord)
                    .First():
                    GetAllCurrecies().Where(aa => aa.Code.ToLower() == xcurrency.Trim()).First();
            return currency;
        }

        public CountryRecord GetCountryByCulture(string culture)
        {
            var cult = _cultureService.ListCultures().Where(c => c.Culture == culture).FirstOrDefault();
            if (cult == null)
            {
                cult = _cultureService.ListCultures().Where(c => c.Culture == "en-MY").First();
            }
            var country =
            _linkCountryCultureRecord.Table.Where(l => l.CultureRecord.Culture == cult.Culture)
               .Select(l => l.CountryRecord)
               .First();
            return country;
        }

        public IQueryable<CultureRecord> GetCultureByCountry(int countryId)
        {
            return _linkCountryCultureRecord.Table.Where(l => l.CountryRecord.Id == countryId)
                .Select(l => l.CultureRecord);
        }

        public IQueryable<LinkCountryCultureRecord> GetAllCultureWithAllCountry()
        {
            return _linkCountryCultureRecord.Table;
        }

        public CountryRecord GetCountry(ILocalizationInfo localizationInfo)
        {
            var country = _countryRepository.Table
                .FirstOrDefault(c => c.Code == localizationInfo.CountryIsoCode);

            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (country == null)
            {
                country = _countryRepository.Table.First(c => c.Code == UnitedStatesCountryIsoCode);
            }

            return country;
        }

        public CurrencyRecord GetCurrency(ILocalizationInfo localizationInfo, string xcurrency)
        {
            var country = GetCountry(localizationInfo);
            var currency = (string.IsNullOrWhiteSpace(xcurrency))
                ? country.CountryCurrencies.First().CurrencyRecord :
                GetAllCurrecies().Where(aa => aa.Code.ToLower() == xcurrency.Trim()).First();

            return currency;
        }


        public IQueryable<CurrencyRecord> GetAllCurrecies()
        {
            return _currencyRecord.Table;
        }

        //public CurrencyRecord GetCurrencyByCulture(string culture, string currency)
        //{
        //    throw new NotImplementedException();
        //}

        //public object GetCurrency(ILocalizationInfo localizationInfo, string currency)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
