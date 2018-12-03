using IpToCountry;
using Orchard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Teeyoot.Localization.IpAddress;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Services
{
    public class MultiCountryService : IMultiCountryService
    {
        public readonly IRepository<CurrencyRecord> _currencies;
        public readonly IRepository<CountryRecord> _countries;


        public MultiCountryService(IRepository<CurrencyRecord> currencies, IRepository<CountryRecord> countries)
        {
            _currencies = currencies;
            _countries = countries;
        }

        public CountryRecord GetDefaultCountry()
        {
            var country = _countries.Table.Where(aa => aa.Code == "US").FirstOrDefault();
            return country == null ? _countries.Table.FirstOrDefault() : country;
        }

        public CountryRecord GetCountry()
        {
            var ipAddress = GetIpAddress();
            var ipAddressLocation = IpToCountry.IpToCountryCache.GetIpAddressLocation(IPAddress.Parse(ipAddress));
            var country = _countries.Table.Where(aa => aa.Code == ipAddressLocation.CountryCode).FirstOrDefault();
            return country == null ? GetDefaultCountry() : country;
        }

        public IpAddressLocation GetCountryCode()
        {
            var ipAddress = GetIpAddress();
            return IpToCountry.IpToCountryCache.GetIpAddressLocation(IPAddress.Parse(ipAddress));
        }
        public CurrencyRecord GetCurrency()
        {
            return
                GetCountry().CountryCurrencies.FirstOrDefault() == null ? _currencies.Table.FirstOrDefault(aa => aa.Code == "USD") :
                GetCountry().CountryCurrencies.FirstOrDefault().CurrencyRecord;
        }

        public CurrencyRecord GetDefaultCurrecny()
        {
            return GetDefaultCountry().CountryCurrencies.FirstOrDefault() == null ? _currencies.Table.FirstOrDefault(aa => aa.Code == "USD") :
                GetDefaultCountry().CountryCurrencies.FirstOrDefault().CurrencyRecord;
        }

        public string GetIpAddress()
        {
            try
            {
                var userHostAddress = HttpContext.Current.Request.UserHostAddress;
                IPAddress.Parse(userHostAddress);
                return userHostAddress;
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }
    }
}