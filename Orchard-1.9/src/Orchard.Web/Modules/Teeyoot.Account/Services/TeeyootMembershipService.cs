﻿using System;
using System.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization.Records;
using Orchard.Logging;
using Orchard.Roles.Models;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Users.Models;
using Teeyoot.Localization;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Account.Services
{
    public class TeeyootMembershipService : ITeeyootMembershipService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IRoleService _roleService;
        private readonly IRepository<UserRolesPartRecord> _userRolesRepository;
        private readonly ICountryService _countryService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IRepository<CultureRecord> _cultureRepository;
        private readonly IRepository<CurrencyRecord> _currencies;
        // ReSharper disable once InconsistentNaming
        private const string PBKDF2 = "PBKDF2";

        public ILogger Logger { get; set; }

        public TeeyootMembershipService(
            IOrchardServices orchardServices,
            IMembershipService membershipService,
            IRoleService roleService,
            IRepository<UserRolesPartRecord> userRolesRepository,
            ICountryService countryService,
            IWorkContextAccessor workContextAccessor,
            IRepository<CultureRecord> cultureRepository,
            IRepository<CurrencyRecord> currencies)
        {
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _roleService = roleService;
            _userRolesRepository = userRolesRepository;
            _countryService = countryService;
            _cultureRepository = cultureRepository;
            _currencies = currencies;
            Logger = NullLogger.Instance;
            _workContextAccessor = workContextAccessor;
        }

        public IUser CreateUser(string email, string password, string name, string phone)
        {
            Logger.Information("CreateUser {0} {1}", email, password, name, phone);

            var teeyootUser = _orchardServices.ContentManager.New("TeeyootUser");

            var userPart = teeyootUser.As<UserPart>();

            userPart.UserName = email;
            userPart.Email = email;
            userPart.NormalizedUserName = email.ToLowerInvariant();
            userPart.HashAlgorithm = PBKDF2;
            _membershipService.SetPassword(userPart, password);
            userPart.RegistrationStatus = UserStatus.Approved;
            userPart.EmailStatus = UserStatus.Approved;

            var teeyootUserPart = teeyootUser.As<TeeyootUserPart>();

            var culture = _workContextAccessor.GetContext().CurrentCulture.Trim();
            var cultureRecord = _cultureRepository.Table.First(c => c.Culture == culture);

            userPart.Culture = cultureRecord;

            teeyootUserPart.CreatedUtc = DateTime.UtcNow;
            teeyootUserPart.PhoneNumber = phone;
            teeyootUserPart.PublicName = name;
            teeyootUserPart.TeeyootUserCulture = cultureRecord.Culture;

            var localizationInfo = LocalizationInfoFactory.GetCurrentLocalizationInfo();
            var USDCurrency = _currencies.Table.FirstOrDefault(aa => aa.Code == "USD");
            teeyootUserPart.CountryRecord = _countryService.GetCountry(localizationInfo);
            teeyootUserPart.CurrencyRecord = (USDCurrency == null) ? _countryService.GetCurrency(localizationInfo, LocalizationInfoFactory.GetCurrency()) : USDCurrency;

            _orchardServices.ContentManager.Create(teeyootUser);

            var role = _roleService.GetRoleByName("Seller");
            if (role != null)
            {
                _userRolesRepository.Create(new UserRolesPartRecord
                {
                    UserId = userPart.Id,
                    Role = role
                });
            }

            return userPart;
        }
    }
}
