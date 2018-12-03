using System;
using System.Collections.Generic;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Teeyoot.Localization;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Services
{
    public class PriceConversionService : IPriceConversionService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<CurrencyExchangeRecord> _currencyExchangeRepository;
        private readonly ICountryService _countryService;
        private readonly IRepository<CurrencyRecord> _currencies; 


        private IEnumerable<CurrencyExchangeRecord> _currencyExchanges;
        private bool? _isCurrentUserSeller;
        private CurrencyRecord _currentUserCurrency;

        private bool IsCurrentUserSeller
        {
            get
            {
                if (_isCurrentUserSeller.HasValue)
                {
                    return _isCurrentUserSeller.Value;
                }

                var currentUser = _orchardServices.WorkContext.CurrentUser;
                if (currentUser == null)
                {
                    _isCurrentUserSeller = false;
                    return _isCurrentUserSeller.Value;
                }

                var currentTeeyootUser = currentUser.As<TeeyootUserPart>();
                _isCurrentUserSeller = currentTeeyootUser != null;

                return _isCurrentUserSeller.Value;
            }
        }

        private IEnumerable<CurrencyExchangeRecord> CurrencyExchanges
        {
            get
            {
                if (_currencyExchanges != null)
                {
                    return _currencyExchanges;
                }

                _currencyExchanges = _currencyExchangeRepository.Table.ToList();
                return _currencyExchanges;
            }
        }

        public CurrencyRecord CurrentUserCurrency
        {
            get
            {
                if (_currentUserCurrency != null)
                {
                    return _currentUserCurrency;
                }

                if (IsCurrentUserSeller)
                {
                    var _xxxx = _orchardServices.WorkContext
                        .CurrentUser.As<TeeyootUserPart>();
                    _currentUserCurrency = _xxxx.CurrencyRecord;
                    if (_currentUserCurrency == null) _currentUserCurrency = _currencies.Fetch(aa => aa.Code == "USD").FirstOrDefault();
                    return _currentUserCurrency;
                }

                var localizationInfo = LocalizationInfoFactory.GetCurrentLocalizationInfo();
                _currentUserCurrency = _countryService.GetCurrency(localizationInfo, LocalizationInfoFactory.GetCurrency());

                return _currentUserCurrency;
            }
        }

        public PriceConversionService(
            IOrchardServices orchardServices,
            IRepository<CurrencyExchangeRecord> currencyExchangeRepository,
            ICountryService countryService,
            IRepository<CurrencyRecord> Currencies)
        {
            _orchardServices = orchardServices;
            _currencyExchangeRepository = currencyExchangeRepository;
            _countryService = countryService;
            _currencies = Currencies;
        }

        public Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom)
        {
            if (currencyFrom == CurrentUserCurrency)
            {
                return new Price
                {
                    Value = priceValue,
                    Currency = currencyFrom
                };
            }

            var currencyExchange = CurrencyExchanges
                .First(e => e.CurrencyFrom == currencyFrom &&
                            e.CurrencyTo == CurrentUserCurrency);

            var exchangeRate = IsCurrentUserSeller ? currencyExchange.RateForSeller : currencyExchange.RateForBuyer;

            var convertedPrice = new Price
            {
                Value = priceValue*exchangeRate,
                Currency = CurrentUserCurrency
            };

            return convertedPrice;
        }

        public Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom, ExchangeRateFor exchangeRateFor)
        {
            if (currencyFrom == CurrentUserCurrency)
            {
                return new Price
                {
                    Value = priceValue,
                    Currency = currencyFrom
                };
            }

            var currencyExchange = CurrencyExchanges
                .First(e => e.CurrencyFrom == currencyFrom &&
                            e.CurrencyTo == CurrentUserCurrency);

            double exchangeRate;
            switch (exchangeRateFor)
            {
                case ExchangeRateFor.Buyer:
                    exchangeRate = currencyExchange.RateForBuyer;
                    break;
                case ExchangeRateFor.Seller:
                    exchangeRate = currencyExchange.RateForSeller;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("exchangeRateFor", exchangeRateFor, null);
            }

            var convertedPrice = new Price
            {
                Value = priceValue*exchangeRate,
                Currency = CurrentUserCurrency
            };

            return convertedPrice;
        }

        public Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom, CurrencyRecord currencyTo)
        {
            if (currencyFrom == currencyTo)
            {
                return new Price
                {
                    Value = priceValue,
                    Currency = currencyFrom
                };
            }

            var currencyExchange = CurrencyExchanges
                .First(e => e.CurrencyFrom == currencyFrom &&
                            e.CurrencyTo == currencyTo);

            var exchangeRate = IsCurrentUserSeller ? currencyExchange.RateForSeller : currencyExchange.RateForBuyer;

            var convertedPrice = new Price
            {
                Value = priceValue * exchangeRate,
                Currency = CurrentUserCurrency
            };

            return convertedPrice;
        }

        public Price ConvertPrice(double priceValue, CurrencyRecord currencyFrom, string currencyTo)
        {
            var c = _currencies.Table.FirstOrDefault(aa => aa.Code == currencyTo);
            if (c == null) return new Price() { Value = priceValue, Currency = currencyFrom };
            return ConvertPrice(priceValue, currencyFrom, c);
        }


        public Price ConvertPrice(double priceValue, string currencyFrom, CurrencyRecord currencyRecord)
        {
            var c = _currencies.Table.FirstOrDefault(aa => aa.Code == currencyFrom);
            if (c == null) return new Price() { Value = priceValue, Currency = currencyRecord };
            return ConvertPrice(priceValue, c, currencyRecord);
        }
    }

    public class Price
    {
        public double Value { get; set; }
        public CurrencyRecord Currency { get; set; }
    }

    public enum ExchangeRateFor
    {
        Buyer,
        Seller
    }
}
