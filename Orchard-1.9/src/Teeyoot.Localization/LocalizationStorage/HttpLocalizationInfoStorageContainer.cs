using System.Web;

namespace Teeyoot.Localization.LocalizationStorage
{
    public class HttpLocalizationInfoStorageContainer : ILocalizationInfoStorageContainer
    {
        private const string LocalizationInfoKey = "LocalizationInfo";
        private const string CurrencyCodeInfoKey = "CurrencyCode";

        public ILocalizationInfo GetCurrentLocalizationInfo()
        {
            ILocalizationInfo localizationInfo = null;

            if (HttpContext.Current.Items.Contains(LocalizationInfoKey))
                localizationInfo = (ILocalizationInfo) HttpContext.Current.Items[LocalizationInfoKey];

            return localizationInfo;
        }

        public void Store(ILocalizationInfo localizationInfo)
        {
            if (HttpContext.Current.Items.Contains(LocalizationInfoKey))
                HttpContext.Current.Items[LocalizationInfoKey] = localizationInfo;
            else
                HttpContext.Current.Items.Add(LocalizationInfoKey, localizationInfo);
        }


        public void StorCurrency(string CurrencyCode)
        {
            //if (HttpContext.Current.Session..Items.Contains(CurrencyCodeInfoKey))
            //    HttpContext.Current.Items[CurrencyCodeInfoKey] = CurrencyCode;
            //else
            //    HttpContext.Current.Items.Add(CurrencyCodeInfoKey, CurrencyCode);
            HttpContext.Current.Session[CurrencyCodeInfoKey] = CurrencyCode;
        }

        public string GetCurrencyCode()
        {
            //= CurrencyCode;
            return HttpContext.Current.Session[CurrencyCodeInfoKey] == null? null: HttpContext.Current.Session[CurrencyCodeInfoKey].ToString();
            if (HttpContext.Current.Items.Contains(CurrencyCodeInfoKey)) return (string)HttpContext.Current.Items[CurrencyCodeInfoKey];
            return null;
        }
    }
}
