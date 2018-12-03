using Orchard;
using Teeyoot.Module.Models;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface IPriceConversionService : IDependency
    {
        CurrencyRecord CurrentUserCurrency { get; }
        Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom);
        Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom, ExchangeRateFor exchangeRateFor);
        Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom, CurrencyRecord currencyTo);
        Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom, string currencyTo);

        Price ConvertPrice(double priceValue, string currencyFrom, CurrencyRecord currencyRecord);
    }
}
