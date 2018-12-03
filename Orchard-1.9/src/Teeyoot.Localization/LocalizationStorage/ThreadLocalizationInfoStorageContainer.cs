using System.Collections;
using System.Threading;

namespace Teeyoot.Localization.LocalizationStorage
{
    public class ThreadLocalizationInfoStorageContainer : ILocalizationInfoStorageContainer
    {
        private static readonly Hashtable LocalizationInfos = new Hashtable();
        private static readonly Hashtable Currencies = new Hashtable();

        private static int CurrentThreadId
        {
            get { return Thread.CurrentThread.ManagedThreadId; }
        }

        public ILocalizationInfo GetCurrentLocalizationInfo()
        {
            ILocalizationInfo localizationInfo = null;

            if (LocalizationInfos.Contains(CurrentThreadId))
                localizationInfo = (ILocalizationInfo)LocalizationInfos[CurrentThreadId];

            return localizationInfo;
        }

        public void Store(ILocalizationInfo localizationInfo)
        {
            if (LocalizationInfos.Contains(CurrentThreadId))
                LocalizationInfos[CurrentThreadId] = localizationInfo;
            else
                LocalizationInfos.Add(CurrentThreadId, localizationInfo);
        }


        public void StorCurrency(string CurrencyCode)
        {
            if (Currencies.Contains(CurrentThreadId))
                Currencies[CurrentThreadId] = CurrencyCode;
            else
                Currencies.Add(CurrentThreadId, CurrencyCode);
        }

        public string GetCurrencyCode()
        {
            if (Currencies.Contains(CurrentThreadId))
                return (string)Currencies[CurrentThreadId];
            return null;
        }
    }
}
