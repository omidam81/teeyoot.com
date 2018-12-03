using Braintree;
using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.SqlServer.Server;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Teeyoot.Module.Common.Enums;
using Teeyoot.Module.Messaging.CampaignService;
using Teeyoot.Module.Models;
using Teeyoot.Module.ViewModels;
using Teeyoot.Module.Services.Interfaces;
using Orchard.ContentManagement.Drivers;
using Teeyoot.Localization;
using System.Web.Mvc;
using Orchard.Themes;

namespace Teeyoot.Module.Drivers
{
    public class SelectCurrencyWidget : ContentPartDriver<SelectCurrencyWidgetPart>
    {
        private readonly ICountryService _countryService;
        private IOrchardServices Services { get; set; }
        private readonly IWorkContextAccessor _wca;
        private readonly IRepository<TeeyootUserPart> _userPartRepository;
        private readonly IMultiCountryService _countries;
        private readonly IRepository<CurrencyExchangeRecord> _currencyExchangeRepository;


        public SelectCurrencyWidget(ICountryService countryService, IOrchardServices services, IWorkContextAccessor wca, IRepository<TeeyootUserPart> userPartRepository, IMultiCountryService countries, IRepository<CurrencyExchangeRecord> currencyExchangeRepository)
        {
            _countryService = countryService;
            Services = services;
            _wca = wca;
            _userPartRepository = userPartRepository;
            _countries = countries;
            _currencyExchangeRepository = currencyExchangeRepository;
        }


        protected override DriverResult Display(SelectCurrencyWidgetPart part, string displayType, dynamic shapeHelper)
        {
            var user = Services.WorkContext.CurrentUser;
            var currency = _countries.GetCurrency();
            var Currencies = _countryService.GetAllCurrecies();
            var Selected = currency;
            var IsLoggedIn = (_wca.GetContext().CurrentUser != null);
            var User = (IsLoggedIn) ? user.ContentItem.As<TeeyootUserPart>() : null;
            //var ExchangeRate = Newtonsoft.Json.JsonConvert.SerializeObject();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var ExchangeRate = serializer.Serialize(_currencyExchangeRepository.Table.Select(aa => new { From = aa.CurrencyFrom.Code, To = aa.CurrencyTo.Code, RateForSeller = aa.RateForSeller, RateForBuyer = aa.RateForBuyer }).ToArray());

            return ContentShape("Parts_SelectCurrencyWidget", () => shapeHelper.Parts_SelectCurrencyWidget(
                Currencies: Currencies,
                Selected: Selected,
                IsLoggedIn: IsLoggedIn,
                User: User,
                ExchangeRate: ExchangeRate
            ));
        }
    }
}