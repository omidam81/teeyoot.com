using System;
using System.Net;
using Teeyoot.Localization.GeoLocation;
using Teeyoot.Localization.IpAddress;
using Teeyoot.Localization.LocalizationStorage;

namespace Teeyoot.Localization
{
    public static class LocalizationInfoFactory
    {
        private static IIpAddressProvider _ipAddressProvider;
        private static IGeoLocationInfoProvider _geoLocationInfoProvider;

        public static void Init(
            IIpAddressProvider ipAddressProvider,
            IGeoLocationInfoProvider geoLocationInfoProvider)
        {
            //IpToCountry.IpToCountryCache.Load();

            _ipAddressProvider = ipAddressProvider;
            _geoLocationInfoProvider = geoLocationInfoProvider;
        }

        private static ILocalizationInfo GetNewLocalizationInfo()
        {
            
            //var tmp = new CountryInfo()
            //{
            //    Country = Country.Malaysia,
            //    CountryIsoCode = "MY"
            //};
            //return new TeeyootLocalizationInfo(tmp);

            var ipAddress = _ipAddressProvider.GetIpAddress();
            CountryInfo country = null;

            try
            {
                //WebClient web = new WebClient();
                //var address = string.Format("http://freegeoip.net/{0}/{1}", "json", ipAddress);
                //string retVal = web.DownloadString(address);

                var ipAddressLocation = IpToCountry.IpToCountryCache.GetIpAddressLocation(IPAddress.Parse(ipAddress));
                //var isoCode = (string)Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(retVal).country_code;

                country = GetCurrentCountryFrom(ipAddressLocation.CountryCode);

            }
            catch (Exception ex)
            {
               country = _geoLocationInfoProvider.GetCountry(ipAddress);
            }
            
            return new TeeyootLocalizationInfo(country);
        }

        public static ILocalizationInfo GetCurrentLocalizationInfo()
        {

            var localizationStorageContainer = LocalizationInfoStorageContainerFactory.GetStorageContainer();

            var localizationInfo = localizationStorageContainer.GetCurrentLocalizationInfo();

            if (localizationInfo != null)
                return localizationInfo;

            localizationInfo = GetNewLocalizationInfo();
            localizationStorageContainer.Store(localizationInfo);
            
            return localizationInfo;
        }

        public static string GetCurrency()
        {
            /**TO DO, IT MUST CHANGE THE ORIGINAL FORM**/
            /*return "MYR";*/
            var localizationStorageContainer = LocalizationInfoStorageContainerFactory.GetStorageContainer();
            return localizationStorageContainer.GetCurrencyCode();
        }

        private static CountryInfo GetCurrentCountryFrom(string isoCode)
        {
            ///////////////////////////////
            //return new CountryInfo
            //{
            //    Country = Country.Malaysia,
            //    CountryIsoCode = "MY"
            //};
            //////////////////////////////
            //if (string.IsNullOrWhiteSpace(isoCode))
            //{
            //    return new CountryInfo { Country = Country.Unknown, CountryIsoCode = "" };
            //}

            switch (isoCode)
            {
                case "MY":
                    {
                        return new CountryInfo
                        {
                            Country = Country.Malaysia,
                            CountryIsoCode = isoCode
                        };
                    }
                case "SG":
                    {
                        return new CountryInfo
                        {
                            Country = Country.Singapore,
                            CountryIsoCode = isoCode
                        };
                    }
                case "ID":
                    {
                        return new CountryInfo
                        {
                            Country = Country.Indonesia,
                            CountryIsoCode = isoCode
                        };
                    }
                default:
                    {
                        return new CountryInfo
                        {
                            Country = Country.Malaysia,
                            CountryIsoCode = "US"
                        };
                    }
            }
        }
    }
}
