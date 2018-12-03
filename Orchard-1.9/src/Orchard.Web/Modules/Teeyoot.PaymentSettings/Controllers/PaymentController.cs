using Orchard;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Localization.Records;
using Orchard.UI.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teeyoot.FAQ.Models;
using Teeyoot.FAQ.Services;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;
using Teeyoot.Module.ViewModels;

namespace Teeyoot.PaymentSettings.Controllers
{
    [Admin]
    public class PaymentController : Controller
    {
        private readonly IPaymentSettingsService _paymentSettingsService;
        private IOrchardServices Services { get; set; }
        private readonly IWorkContextAccessor _workContextAccessor;
        private string cultureUsed = string.Empty;
        private readonly IRepository<CultureRecord> _cultures;

        private readonly IRepository<CountryRecord> _countries;


        public Localizer T { get; set; }

        public PaymentController(IWorkContextAccessor workContextAccessor, IPaymentSettingsService paymentSettingsService, IOrchardServices services, IRepository<CultureRecord> cultures, IRepository<CountryRecord> countries)
        {
            _workContextAccessor = workContextAccessor;
            _paymentSettingsService = paymentSettingsService;
            Services = services;
            var culture = _workContextAccessor.GetContext().CurrentCulture.Trim();
            _cultures = cultures;
            _countries = countries;

            //cultureUsed = culture == "en-SG" ? "en-SG" : (culture == "id-ID" ? "id-ID" : "en-MY");
        }

        public ActionResult Index(int? countryId)
        {

            var setting = _paymentSettingsService.GetAllSettigns().FirstOrDefault();

            if (countryId.HasValue)
            {
                setting = _paymentSettingsService.GetAllSettigns().FirstOrDefault(aa => aa.CountryRecord.Id == countryId);
            }
            else
                countryId = _countries.Table.FirstOrDefault().Id;

            ViewBag.Countries = _countries.Table.ToArray();

            ViewBag.Cultures = _cultures.Table.ToArray();
            //ViewBag.Countries = 
            ViewBag.CountryId = countryId.Value;//.HasValue ? countryId : _countries.Table.FirstOrDefault().Id;

            if (setting == null)
                return View(new PaymentSettingsViewModel() { CashDeliv = false, CreditCard = false, Mol = false, PayPal = false, SettingEmpty = true });
            ViewBag.Culture = setting.CountryRecord.DefaultCulture.CultureRecord.Culture;


            return View(new PaymentSettingsViewModel()
            {
                merchantId = setting.MerchantId,
                clientToken = setting.ClientToken,
                merchantIdMol = setting.MerchantIdMol,
                privateKey = setting.PrivateKey,
                verifyKey = setting.VerifyKey,
                publicKey = setting.PublicKey,
                CashDeliv = setting.CashDeliv,
                CreditCard = setting.CreditCard,
                Mol = setting.Mol,
                PayPal = setting.PayPal,
                SettingEmpty = false,
                // Tab names for payment methods
                CashDelivTabName = setting.CashDelivTabName,
                PayPalTabName = setting.PayPalTabName,
                MolTabName = setting.MolTabName,
                CreditCardTabName = setting.CreditCardTabName,
                // Notes for payment methods
                CashDelivNote = setting.CashDelivNote,
                PayPalNote = setting.PayPalNote,
                MolNote = setting.MolNote,
                CreditCardNote = setting.CreditCardNote,
                Ipay88 = setting.Ipay88,
                Ipay88TabName = setting.Ipay88TabName,
                Ipay88MerchantCode = setting.Ipay88MerchantCode,
                Ipay88PaymentId = setting.Ipay88PaymentId,
                Ipay88MerchantKey = setting.Ipay88MerchantKey,
                Paypal_ = setting.Paypal_,
                PaypalClientID_ = setting.PaypalClientID_,
                PaypalSecret_ = setting.PaypalSecret_,
                PaypalTabName_ = setting.PaypalTabName_,
                Ipay88Note = setting.Ipay88Note,
                PayPalNote_ = setting.PayPalNote_,
                BlueSnap = setting.BlueSnap,
                BlueSnapDesc = setting.BlueSnapDesc,
                BlueSnapKey = setting.BlueSnapKey,
                BlueSnapPass = setting.BlueSnapPass,
                BlueSnapTabName = setting.BlueSnapTabName
            });
        }
        public ActionResult SaveSettings(bool CashDeliv, bool PayPal, bool Mol, bool CreditCard, string PrivateKey, string PublicKey, string MerchantId,
                                        string ClientToken, string MerchantIdMol, string VerifyKey,
            // Tab names for payment methods
                                        string CashDelivTabName,
                                        string PayPalTabName,
                                        string MolTabName,
                                        string CreditCardTabName,
            // Notes for payment methods
                                        string CashDelivNote,
                                        string PayPalNote,
                                        string MolNote,
                                        string CreditCardNote,
                                        bool Ipay88,
                                        string Ipay88TabName,
                                        string Ipay88MerchantCode,
                                        int Ipay88PaymentId,
                                        string Ipay88MerchantKey,
                                        bool Paypal_,
                                        string PaypalClientID_,
                                        string PaypalSecret_,
                                        string PaypalTabName_,
                                        string Ipay88Note,
                                        string PayPalNote_,
                                        int? CountryId,
                                        bool BlueSnap,
                                        string BlueSnapDesc,
                                        string BlueSnapKey,
                                        string BlueSnapPass,
                                        string BlueSnapTabName
            )
        {
            if (!CountryId.HasValue) CountryId = _paymentSettingsService.GetAllSettigns().FirstOrDefault().CountryRecord.Id;
            var setting = _paymentSettingsService.GetAllSettigns().FirstOrDefault(aa => aa.CountryRecord.Id == CountryId);

            //setting.PaymentMethod = Convert.ToInt32(PaymentMethod);
            setting.PublicKey = PublicKey;
            setting.PrivateKey = PrivateKey;
            setting.MerchantId = MerchantId;
            setting.MerchantIdMol = MerchantIdMol;
            setting.VerifyKey = VerifyKey;
            setting.ClientToken = ClientToken;
            setting.CashDeliv = CashDeliv;
            setting.PayPal = PayPal;
            setting.Mol = Mol;
            setting.CreditCard = CreditCard;

            // Tab names for payment methods
            setting.CashDelivTabName = CashDelivTabName;
            setting.PayPalTabName = PayPalTabName;
            setting.MolTabName = MolTabName;
            setting.CreditCardTabName = CreditCardTabName;
            // Notes for payment methods
            setting.CashDelivNote = CashDelivNote;
            setting.PayPalNote = PayPalNote;
            setting.MolNote = MolNote;
            setting.CreditCardNote = CreditCardNote;
            setting.Ipay88 = Ipay88;
            setting.Ipay88MerchantCode = Ipay88MerchantCode;
            setting.Ipay88TabName = Ipay88TabName;
            setting.Ipay88PaymentId = Ipay88PaymentId;
            setting.Ipay88MerchantKey = Ipay88MerchantKey;

            setting.Paypal_ = Paypal_;
            setting.PaypalClientID_ = PaypalClientID_;
            setting.PaypalSecret_ = PaypalSecret_;
            setting.PaypalTabName_ = PaypalTabName_;
            setting.Ipay88Note = Ipay88Note;
            setting.PayPalNote_ = PayPalNote_;
            setting.BlueSnap = BlueSnap;
            setting.BlueSnapDesc = BlueSnapDesc;
            setting.BlueSnapKey = BlueSnapKey;
            setting.BlueSnapPass = BlueSnapPass;
            setting.BlueSnapTabName = BlueSnapTabName;


            _paymentSettingsService.UpdateSettings(setting);
            return RedirectToAction("Index", "Payment");
        }
        public ActionResult AddSetting(int countryID)
        {
            var Country = _countries.Table.FirstOrDefault(aa => aa.Id == countryID);
            if (Country == null) return RedirectToAction("Index");

            _paymentSettingsService.AddSettings(new PaymentSettingsRecord() { Culture = Country.DefaultCulture.CultureRecord.Culture, CountryRecord = Country });
            ViewBag.Culture = Country.DefaultCulture.CultureRecord.Culture;
            ViewBag.CountryId = countryID;

            return RedirectToAction("Index", new { countryId = Country.Id });
        }

        //public ActionResult EditSetting(string language, int paumentMethod)
        //{
        //    var setting = _paymentSettingsService.GetAllSettigns().FirstOrDefault(s => s.Culture == language);
        //    setting.PaymentMethod = paumentMethod;
        //    _paymentSettingsService.UpdateSettings(setting);
        //    //_paymentSettingsService.AddSettings(new PaymentSettingsRecord() { Culture = language, PaymentMethod = 1 });
        //    return RedirectToAction("Index", "Payment", new { culture = language });
        //}


    }
}