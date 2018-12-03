using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Mandrill;
using Mandrill.Model;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Users.Models;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services.Interfaces;

namespace Teeyoot.Module.Services
{
    public class TeeyootMessagingService : ITeeyootMessagingService
    {
        private readonly IMailSubjectService _mailSubjectService;
        private readonly IRepository<CurrencyRecord> _currencyRepository;
        private readonly IContentManager _contentManager;
        private readonly IMessageService _messageService;
        private readonly IMailChimpSettingsService _settingsService;
        private readonly IRepository<CampaignRecord> _campaignRepository;
        private readonly IRepository<OrderRecord> _orderRepository;
        private readonly IRepository<LinkOrderCampaignProductRecord> _ocpRepository;
        private readonly IRepository<UserRolesPartRecord> _userRolesPartRepository;
        private readonly IRepository<PaymentInformationRecord> _payoutInformRepository;
        private readonly IRepository<CampaignProductRecord> _campaignProductRepository;
        private readonly IRepository<BringBackCampaignRecord> _backCampaignRepository;
        private readonly IRepository<UserPartRecord> _userPartRepository;
        private readonly IRepository<Outbox> _mailOutBox;

        private readonly IPriceConversionService _priceConvert;

        private readonly IWorkContextAccessor _wca;
        public Localizer T { get; set; }
        private const string ADMIN_EMAIL = "noreply@teeyoot.com";
        private const string MESSAGE_TEMPLATES_PATH = "/Modules/Teeyoot.Module/Content/message-templates/";

        public TeeyootMessagingService(
            IRepository<MailTemplateSubjectRecord> subjectRepository,
            IContentManager contentManager,
            IRepository<CampaignRecord> campaignRepository,
            IMailChimpSettingsService settingsService,
            IMessageService messageService,
            IRepository<OrderRecord> orderRepository,
            IRepository<LinkOrderCampaignProductRecord> ocpRepository,
            IRepository<UserRolesPartRecord> userRolesPartRepository,
            IRepository<PaymentInformationRecord> payoutInformRepository,
            IWorkContextAccessor wca,
            IRepository<CampaignProductRecord> campaignProductRepository,
            IRepository<CurrencyRecord> currencyRepository,
            IRepository<BringBackCampaignRecord> backCampaignRepository,
            IRepository<UserPartRecord> userPartRepository,
            IRepository<Outbox> mailOutBox,
            IPriceConversionService priceConvert
        )
        {
            _mailSubjectService = new MailSubjectService(subjectRepository);
            _contentManager = contentManager;
            _messageService = messageService;
            _settingsService = settingsService;
            _orderRepository = orderRepository;
            _ocpRepository = ocpRepository;
            _currencyRepository = currencyRepository;
            _campaignRepository = campaignRepository;
            _userRolesPartRepository = userRolesPartRepository;
            _payoutInformRepository = payoutInformRepository;
            _wca = wca;
            _campaignProductRepository = campaignProductRepository;
            _backCampaignRepository = backCampaignRepository;
            _userPartRepository = userPartRepository;
            _mailOutBox = mailOutBox;
            _priceConvert = priceConvert;
        }

        public void SendCheckoutRequestEmails(IEnumerable<CheckoutCampaignRequest> checkoutCampaignRequests)
        {
            var pathToTemplates = HttpContext.Current.Server.MapPath(MESSAGE_TEMPLATES_PATH);

            var sentAddresses = new List<string>();
            foreach (var request in checkoutCampaignRequests)
            {
                if (!(sentAddresses.Exists(a => a == request.Email)))
                {
                    var api =
                        new MandrillApi(
                            _settingsService.GetSettingByCulture(/*request.BuyerCultureRecord.Culture*/ "en-MY")
                                .List()
                                .First()
                                .ApiKey);

                    var mandrillMessage = new MandrillMessage
                    {
                        MergeLanguage = MandrillMessageMergeLanguage.Handlebars,
                        FromEmail = "noreply@teeyoot.com",
                        FromName = "Teeyoot",
                        Subject = "Join us now"
                    };
                    mandrillMessage.To = new List<MandrillMailAddress>
                    {
                        new MandrillMailAddress(request.Email, "Seller")
                    };
                    mandrillMessage.Html =
                        File.ReadAllText(pathToTemplates + "/en-MY/" + "make_the_campaign_seller.html");
                    SendTmplMessage(api, mandrillMessage);
                    sentAddresses.Add(request.Email);
                }
            }
        }

        public void SendExpiredCampaignMessageToSeller(int campaignId, bool isSuccesfull)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
                _userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture("en-MY").List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";

            if (isSuccesfull)
            {
                mandrillMessage.Subject = _mailSubjectService
                    .GetMailSubject("campaign-is-printing-seller-template", sellerCulture);
                //mandrillMessage.Subject = "We are printing one of your designs!";
                mandrillMessage.Html =
                    System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "campaign-is-printing-seller-template.html");
            }
            else
            {
                if (campaign.ProductCountSold < campaign.ProductMinimumGoal)
                {
                    mandrillMessage.Subject = _mailSubjectService
                        .GetMailSubject("not-reach-goal-seller-template", sellerCulture);
                    //mandrillMessage.Subject = "Your campaign didn't reach the minimum";
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "not-reach-goal-seller-template.html");
                }
                else
                {
                    mandrillMessage.Subject = _mailSubjectService
                        .GetMailSubject("not-reach-goal-met-minimum-seller-template", sellerCulture);
                    //mandrillMessage.Subject = "Your campaign has ended, you did just fine!";
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "not-reach-goal-met-minimum-seller-template.html");
                }
            }

            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress>()
            {
                new MandrillMailAddress(seller.Email, "Seller")
            };

            FillCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendExpiredCampaignMessageToAdmin(int campaignId, bool isSuccesfull)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);

            var userIds = _userRolesPartRepository.Table.Where(x => x.Role.Name == "Administrator")
                .Select(x => x.UserId);
            var users = _contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);
            foreach (var user in users)
            {

                var teeyoot_user_part = user.As<TeeyootUserPart>();
                if (teeyoot_user_part != null && !teeyoot_user_part.ReceiveEmail) continue; 

                var adminCulture = "en-MY";//_userPartRepository.Table.Where(u => u.Id == user.Id).First().CultureRecord.Culture;

                var api = new MandrillApi(_settingsService.GetSettingByCulture(adminCulture).List().First().ApiKey);
                var mandrillMessage = new MandrillMessage() { };
                mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                mandrillMessage.FromEmail = ADMIN_EMAIL;
                mandrillMessage.FromName = "Teeyoot";

                if (isSuccesfull)
                {
                    mandrillMessage.Subject = _mailSubjectService
                        .GetMailSubject("expired-campaign-successfull-admin-template", adminCulture);
                    //mandrillMessage.Subject = "A campaign just ended - target";
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "expired-campaign-successfull-admin-template.html");
                }
                else
                {
                    if (campaign.ProductCountSold < campaign.ProductMinimumGoal)
                    {
                        mandrillMessage.Subject = _mailSubjectService
                            .GetMailSubject("expired-campaign-notSuccessfull-admin-template", adminCulture);
                        //mandrillMessage.Subject = "A campaign just ended - no success";
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" +
                                                       "expired-campaign-notSuccessfull-admin-template.html");
                    }
                    else
                    {
                        mandrillMessage.Subject = _mailSubjectService
                            .GetMailSubject("expired-campaign-met-minimum-admin-template", adminCulture);
                        //mandrillMessage.Subject = "A campaign just ended - minimum";
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "expired-campaign-met-minimum-admin-template.html");
                    }
                }

                FillCampaignMergeVars(mandrillMessage, campaignId, user.Email, pathToMedia, pathToTemplates, true);
                mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(user.Email, "Admin") };

                SendTmplMessage(api, mandrillMessage);
            }
        }

        public void SendExpiredCampaignMessageToBuyers(int campaignId, bool isSuccesfull)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);
            //List<string> sentEmail = new List<string>();
            var send = true;

            var sentAddresses = new List<string>();
            List<LinkOrderCampaignProductRecord> ordersList =
                _ocpRepository.Table.Where(
                    p => p.CampaignProductRecord.CampaignRecord_Id == campaignId && p.OrderRecord.IsActive).ToList();
            foreach (var orderItem in ordersList)
            {
                if ((orderItem.OrderRecord.Email != null) &&
                    !(sentAddresses.Exists(a => a == orderItem.OrderRecord.Email)))
                {
                    var buyerCulture = "en-MY";//orderItem.OrderRecord.BuyerCultureRecord.Culture;

                    var api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
                    var mandrillMessage = new MandrillMessage() { };
                    mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                    mandrillMessage.FromEmail = ADMIN_EMAIL;
                    mandrillMessage.FromName = "Teeyoot";
                    if (isSuccesfull)
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject(
                            "order-is-printing-buyer-template", buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "order-is-printing-buyer-template.html");
                    }
                    else
                    {
                        if (campaign.ProductCountSold < campaign.ProductMinimumGoal)
                        {
                            send = false;
                            mandrillMessage.Subject = _mailSubjectService.GetMailSubject(
                                "not-reach-goal-buyer-template", buyerCulture);
                            mandrillMessage.Html =
                                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "not-reach-goal-buyer-template.html");
                        }
                        else
                        {
                            mandrillMessage.Subject =
                                _mailSubjectService.GetMailSubject("order-is-printing-buyer-template", buyerCulture);
                            mandrillMessage.Html =
                                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "order-is-printing-buyer-template.html");
                        }
                    }

                    FillUserMergeVars(mandrillMessage, orderItem.OrderRecord);
                    FillProductsMergeVars(mandrillMessage, orderItem.OrderRecord.Products, pathToMedia,
                        orderItem.OrderRecord.Email, orderItem.OrderRecord.OrderPublicId);
                    FillCampaignMergeVars(mandrillMessage, campaignId, orderItem.OrderRecord.Email, pathToMedia,
                        pathToTemplates, false);

                    mandrillMessage.To = new List<MandrillMailAddress>
                    {
                        new MandrillMailAddress(orderItem.OrderRecord.Email, "Buyer")
                    };
                    if (send)
                    {
                        SendTmplMessage(api, mandrillMessage);
                        sentAddresses.Add(orderItem.OrderRecord.Email);
                    }
                }
            }
        }

        public void SendCampaignMetMinimumMessageToBuyers(int campaignId)
        {
            return;


            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);

            var sentAddresses = new List<string>();
            List<LinkOrderCampaignProductRecord> ordersList =
                _ocpRepository.Table.Where(
                    p => p.CampaignProductRecord.CampaignRecord_Id == campaignId && p.OrderRecord.IsActive).ToList();
            foreach (var orderItem in ordersList)
            {
                if ((orderItem.OrderRecord.Email != null) &&
                    !(sentAddresses.Exists(a => a == orderItem.OrderRecord.Email)))
                {
                    var buyerCulture = "en-MY";// orderItem.OrderRecord.BuyerCultureRecord.Culture;

                    var api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
                    var mandrillMessage = new MandrillMessage();
                    mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                    mandrillMessage.FromEmail = ADMIN_EMAIL;
                    mandrillMessage.FromName = "Teeyoot";

                    mandrillMessage.Subject = _mailSubjectService
                        .GetMailSubject("definitely-go-to-print-buyer-template", buyerCulture);
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "definitely-go-to-print-buyer-template.html");

                    FillUserMergeVars(mandrillMessage, orderItem.OrderRecord);
                    FillProductsMergeVars(mandrillMessage, orderItem.OrderRecord.Products, pathToMedia,
                        orderItem.OrderRecord.Email, orderItem.OrderRecord.OrderPublicId);
                    FillCampaignMergeVars(mandrillMessage, campaignId, orderItem.OrderRecord.Email, pathToMedia,
                        pathToTemplates, false);

                    mandrillMessage.To = new List<MandrillMailAddress>
                    {
                        new MandrillMailAddress(orderItem.OrderRecord.Email, "Buyer")
                    };
                    SendTmplMessage(api, mandrillMessage);
                    sentAddresses.Add(orderItem.OrderRecord.Email);
                }
            }
        }

        public void SendCampaignMetMinimumMessageToSeller(int campaignId)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
                "en-MY";//_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("definitely-go-to-print-seller-template",
                sellerCulture);
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "definitely-go-to-print-seller-template.html");
            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress>()
            {
                new MandrillMailAddress(seller.Email, "Seller")
            };
            FillCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendAllOrderDeliveredMessageToSeller(int campaignId)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);
            if (campaign.IsActive) return;
            if (campaign.ProductCountGoal > campaign.ProductCountSold) return;
            var sellerCulture =
                "en-MY";//_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("all-orders-delivered-seller-template",
                sellerCulture);
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "all-orders-delivered-seller-template.html");
            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress>()
            {
                new MandrillMailAddress(seller.Email, "Seller")
            };
            FillCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendLaunchCampaignMessage(string pathToTemplates, string pathToMedia, int campaignId)
        {
            var campaign = _campaignRepository.Get(campaignId);

            if (campaign.TeeyootUserId == null)
                throw new ApplicationException("Campaign's TeeyootUserId is somehow null");

            var sellerUserPart = _contentManager.Get<UserPart>(campaign.TeeyootUserId.Value);
            var sellerTeeyootUserPart = sellerUserPart.As<TeeyootUserPart>();
            var sellerCulture = "en-MY";//sellerTeeyootUserPart.TeeyootUserCulture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage
            {
                MergeLanguage = MandrillMessageMergeLanguage.Handlebars,
                FromEmail = ADMIN_EMAIL,
                FromName = "Teeyoot",
                Subject = _mailSubjectService.GetMailSubject("launch-template", sellerCulture),
                To = new List<MandrillMailAddress>
                {
                    new MandrillMailAddress(sellerUserPart.Email, "Seller")
                }
            };

            FillCampaignMergeVars(mandrillMessage, campaignId, sellerUserPart.Email, pathToMedia, pathToTemplates, false);
            mandrillMessage.Html = File.ReadAllText(pathToTemplates + "/en-MY/" + "launch-template.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendReLaunchApprovedCampaignMessageToSeller(string pathToTemplates, string pathToMedia,
            int campaignId)
        {
            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
               "en-MY";// _userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = "noreply@teeyoot.com";
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("relaunch-", sellerCulture);

            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress>()
            {
                new MandrillMailAddress(seller.Email, "Seller")
            };

            FillRelaunchCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);

            mandrillMessage.Html = System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "relaunch.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendReLaunchApprovedCampaignMessageToBuyers(string pathToTemplates, string pathToMedia,
            int campaignId)
        {
            var campaign = _campaignRepository.Get(campaignId);

            var sentAddresses = new List<string>();
            var buyers =
                _backCampaignRepository.Table.Where(c => c.CampaignRecord.Id == campaign.BaseCampaignId).ToList();
            foreach (var buyer in buyers)
            {
                if (!sentAddresses.Contains(buyer.Email))
                {
                    var buyerCulture = "en-MY";//buyer.BuyerCultureRecord.Culture;
                    var api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
                    var mandrillMessage = new MandrillMessage();
                    mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                    mandrillMessage.FromEmail = "noreply@teeyoot.com";
                    mandrillMessage.FromName = "Teeyoot";
                    mandrillMessage.Subject = _mailSubjectService.GetMailSubject("relaunch-buyer-", buyerCulture);

                    FillRelaunchCampaignMergeVars(mandrillMessage, campaignId, buyer.Email, pathToMedia, pathToTemplates, false);

                    mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(buyer.Email, "Buyer") };
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "relaunch-buyer.html");
                    SendTmplMessage(api, mandrillMessage);
                    sentAddresses.Add(buyer.Email);
                }
            }
        }

        public void SendReLaunchCampaignMessageToAdmin(int campaignId)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var campaign = _campaignRepository.Get(campaignId);

            var sentAddresses = new List<string>();
            var userIds = _userRolesPartRepository.Table.Where(x => x.Role.Name == "Administrator")
                .Select(x => x.UserId);
            var admins = _contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);
            foreach (var admin in admins)
            {
                var teeyoot_user_part = admin.As<TeeyootUserPart>();
                if (teeyoot_user_part != null && !teeyoot_user_part.ReceiveEmail) continue; 


                if (!sentAddresses.Contains(admin.Email))
                {
                    var adminCulture =
                        "en-MY";//_userPartRepository.Table.Where(u => u.Id == admin.Id).First().CultureRecord.Culture;

                    var api = new MandrillApi(_settingsService.GetSettingByCulture(adminCulture).List().First().ApiKey);
                    var mandrillMessage = new MandrillMessage();
                    mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                    mandrillMessage.FromEmail = "noreply@teeyoot.com";
                    mandrillMessage.FromName = "Teeyoot";
                    mandrillMessage.Subject = _mailSubjectService.GetMailSubject("relaunch-to-admin-seller-",
                        adminCulture);

                    FillRelaunchCampaignMergeVars(mandrillMessage, campaignId, admin.Email, pathToMedia, pathToTemplates, true);

                    mandrillMessage.To = new List<MandrillMailAddress>() { new MandrillMailAddress(admin.Email, "Admin") };
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "relaunch-to-admin-seller.html");
                    SendTmplMessage(api, mandrillMessage);
                    sentAddresses.Add(admin.Email);
                }
            }
        }

        public void SendReLaunchCampaignMessageToSeller(int campaignId)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
                "en-MY";//_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage() { };
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = "noreply@teeyoot.com";
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("relaunch-to-admin-seller-", sellerCulture);

            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(seller.Email, "Seller") };

            FillRelaunchCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "relaunch-to-admin-seller.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendTermsAndConditionsMessageToSeller()
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var campaigns =
                _campaignRepository.Table.Where(
                    camp =>
                        camp.WhenApproved < DateTime.UtcNow.AddDays(-1) &&
                        camp.WhenApproved > DateTime.UtcNow.AddDays(-3));
            foreach (var campaign in campaigns)
            {
                var sellerCulture =
                    "en-MY";//_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

                var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
                var mandrillMessage = new MandrillMessage();
                mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                mandrillMessage.FromEmail = ADMIN_EMAIL;
                mandrillMessage.FromName = "Teeyoot";
                mandrillMessage.Subject = _mailSubjectService.GetMailSubject("terms-conditions-template", sellerCulture);

                var seller =
                    _contentManager.Query<UserPart, UserPartRecord>()
                        .List()
                        .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
                mandrillMessage.To = new List<MandrillMailAddress>() { new MandrillMailAddress(seller.Email, "Seller") };
                FillCampaignMergeVars(mandrillMessage, campaign.Id, seller.Email, pathToMedia, pathToTemplates, false);
                mandrillMessage.Html =
                    System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "terms-conditions-template.html");
                SendTmplMessage(api, mandrillMessage);
            }

        }

        public void SendCampaignFinished1DayMessageToSeller()
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var campaigns =
                _campaignRepository.Table.Where(
                    camp =>
                        camp.EndDate < DateTime.UtcNow.AddDays(-1) && camp.EndDate > DateTime.UtcNow.AddDays(-3) &&
                        camp.IsApproved);
            foreach (var campaign in campaigns)
            {
                var sellerCulture =
                    "en-MY";//_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

                var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
                var mandrillMessage = new MandrillMessage();
                mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                mandrillMessage.FromEmail = ADMIN_EMAIL;
                mandrillMessage.FromName = "Teeyoot";
                mandrillMessage.Subject = _mailSubjectService.GetMailSubject("campaign-is-finished-template",
                    sellerCulture);

                var seller =
                    _contentManager.Query<UserPart, UserPartRecord>()
                        .List()
                        .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
                mandrillMessage.To = new List<MandrillMailAddress>() { new MandrillMailAddress(seller.Email, "Seller") };
                FillCampaignMergeVars(mandrillMessage, campaign.Id, seller.Email, pathToMedia, pathToTemplates, false);
                mandrillMessage.Html =
                    System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "campaign-is-finished-template.html");
                SendTmplMessage(api, mandrillMessage);
            }

        }

        public void SendOrderShipped3DaysToBuyer()
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var orders =
                _orderRepository.Table.Where(
                    order =>
                        order.WhenSentOut < DateTime.UtcNow.AddDays(-1) &&
                        order.WhenSentOut > DateTime.UtcNow.AddDays(-3));
            foreach (var order in orders)
            {
                var buyerCulture = "en-MY";//order.BuyerCultureRecord.Culture;

                var api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
                var mandrillMessage = new MandrillMessage();
                mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                mandrillMessage.FromEmail = ADMIN_EMAIL;
                mandrillMessage.FromName = "Teeyoot";
                mandrillMessage.Subject = _mailSubjectService.GetMailSubject("shipped-order-3day-after-template",
                    buyerCulture);
                mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(order.Email, "Buyer") };

                FillUserMergeVars(mandrillMessage, order, order.Email);
                FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                    order.Email, pathToMedia, pathToTemplates, false);

                mandrillMessage.Html =
                    System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "shipped-order-3day-after-template.html");
                SendTmplMessage(api, mandrillMessage);
            }

        }

        public void SendRejectedCampaignMessage(string pathToTemplates, string pathToMedia, int campaignId)
        {
            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
                "en-MY";//_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("reject-template", sellerCulture);
            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress>()
            {
                new MandrillMailAddress(seller.Email, "Seller")
            };

            FillCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);

            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "reject-template.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendNewCampaignAdminMessage(string pathToTemplates, string pathToMedia, int campaignId)
        {
            var userIds = _userRolesPartRepository.Table
                .Where(x => x.Role.Name == "Administrator")
                .Select(x => x.UserId);

            var admins = _contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);

            foreach (var admin in admins)
            {
                var teeyoot_user_part = admin.As<TeeyootUserPart>();
                if (teeyoot_user_part != null && !teeyoot_user_part.ReceiveEmail) continue; 

                var adminUserPart = admin.As<UserPart>();
                var adminCulture = "en-MY"; //(adminUserPart.Culture == null) ? "en-MY" : adminUserPart.Culture.Culture;
                var api = new MandrillApi(_settingsService.GetSettingByCulture(adminCulture).List().First().ApiKey);

                var mandrillMessage = new MandrillMessage
                {
                    MergeLanguage = MandrillMessageMergeLanguage.Handlebars,
                    FromEmail = ADMIN_EMAIL,
                    FromName = "Teeyoot",
                    Subject = _mailSubjectService.GetMailSubject("new-campaign-admin-template", adminCulture)
                };

                FillCampaignMergeVars(mandrillMessage, campaignId, admin.Email, pathToMedia, pathToTemplates, true);

                mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(admin.Email, "Admin") };
                mandrillMessage.Html = File
                    .ReadAllText(pathToTemplates + "/en-MY/" + "new-campaign-admin-template.html");

                SendTmplMessage(api, mandrillMessage);
            }
        }

        public void SendCompletedPayoutMessage(string pathToTemplates, string pathToMedia, PayoutRecord payout)
        {
            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == payout.UserId);
            var payoutInf = _payoutInformRepository.Table.Where(inf => inf.TranzactionId == payout.Id).FirstOrDefault();
            var currency = _currencyRepository.Get(payout.Currency_Id).Code;

            var sellerCulture =
                "en-MY"; //_userPartRepository.Table.Where(u => u.Id == payout.UserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage() { };
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = "noreply@teeyoot.com";
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("withdraw-completed-template", sellerCulture);

            FillPayoutRequestMergeVars(mandrillMessage, seller.Email, seller.Id, payoutInf.AccountNumber.ToString(),
                payoutInf.BankName.ToString(), payoutInf.AccountHolderName.ToString(),
                payoutInf.ContactNumber.ToString(), "", payout.Amount, currency);

            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(seller.Email, "Seller") };
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "withdraw-completed-template.html");
            SendTmplMessage(api, mandrillMessage);

        }

        public void SendChangedCampaignStatusMessage(int campaignId, string campaignStatus)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");
            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
               "en-MY";// _userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";
            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(seller.Email, "Seller") };

            FillCampaignMergeVars(mandrillMessage, campaignId, seller.Email, pathToMedia, pathToTemplates, false);
            switch (campaignStatus)
            {
                case "Unpaid":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("unpaid-campaign-template",
                            sellerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "unpaid-campaign-template.html");
                        break;
                    }
                    ;
                case "Paid":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("paid-campaign-template", sellerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "paid-campaign-template.html");
                        break;
                    }
                    ;
            }
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendSellerMessage(int messageId, string pathToMedia, string pathToTemplates)
        {
            var sentAddresses = new List<string>();
            var message = _messageService.GetMessage(messageId);
            List<LinkOrderCampaignProductRecord> ordersList =
                _ocpRepository.Table.Where(
                    p => p.CampaignProductRecord.CampaignRecord_Id == message.CampaignId && p.OrderRecord.IsActive)
                    .ToList();
            foreach (var order in ordersList)
            {
                if ((order.OrderRecord.Email != null) && !(sentAddresses.Contains(order.OrderRecord.Email)))
                {
                    var buyerCulture = "en-MY";//order.OrderRecord.BuyerCultureRecord.Culture;

                    var api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
                    var mandrillMessage = new MandrillMessage();
                    mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                    mandrillMessage.FromEmail = message.Sender;
                    mandrillMessage.Subject = message.Subject;
                    mandrillMessage.FromName = "Teeyoot";

                    FillUserMergeVars(mandrillMessage, order.OrderRecord);
                    FillSellerToBuyersProductsMergeVars(mandrillMessage, order.OrderRecord.Products, pathToMedia,
                        order.OrderRecord.Email, order.OrderRecord.OrderPublicId);
                    FillCampaignMergeVars(mandrillMessage, message.CampaignId, order.OrderRecord.Email, pathToMedia,
                        pathToTemplates, false);

                    mandrillMessage.To = new List<MandrillMailAddress>
                    {
                        new MandrillMailAddress(order.OrderRecord.Email, "Buyer")
                    };
                    mandrillMessage.Html =
                        System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "seller-template.html")
                            .Replace("{{Text}}", message.Text);
                    message.IsApprowed = true;
                    SendTmplMessage(api, mandrillMessage);
                    sentAddresses.Add(order.OrderRecord.Email);
                }
            }
        }

        public void SendNewOrderMessageToAdmin(int orderId, string pathToMedia, string pathToTemplates)
        {
            var order = _orderRepository.Get(orderId);

            var userIds = _userRolesPartRepository.Table.Where(x => x.Role.Name == "Administrator")
                .Select(x => x.UserId);
            var admins = _contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);
            foreach (var admin in admins)
            {
                var teeyoot_user_part = admin.As<TeeyootUserPart>();
                if (teeyoot_user_part != null && !teeyoot_user_part.ReceiveEmail) continue; 


                var adminCulture = "en-MY"; //TODO: _userPartRepository.Table.Where(u => u.Id == admin.Id).First().CultureRecord.Culture;

                var api = new MandrillApi(_settingsService.GetSettingByCulture(adminCulture).List().First().ApiKey);
                var mandrillMessage = new MandrillMessage();
                mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                mandrillMessage.FromEmail = "noreply@teeyoot.com";
                mandrillMessage.Subject = _mailSubjectService.GetMailSubject("new-order-template", adminCulture);
                mandrillMessage.FromName = "Teeyoot";

                FillUserMergeVars(mandrillMessage, order, admin.Email);
                FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, admin.Email, order.OrderPublicId);
                FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                    admin.Email, pathToMedia, pathToTemplates, true);

                mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(admin.Email, "Admin") };
                mandrillMessage.Html =
                    System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "new-order-template.html");
                SendTmplMessage(api, mandrillMessage);
            }
        }

        public void SendNewOrderMessageToBuyer(int orderId, string pathToMedia, string pathToTemplates)
        {
            var order = _orderRepository.Get(orderId);

            var buyerCulture = "en-MY";//order.BuyerCultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = "noreply@teeyoot.com";
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("new-order-buyer-template", buyerCulture);

            FillUserMergeVars(mandrillMessage, order, order.Email);
            FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
            FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                order.Email, pathToMedia, pathToTemplates, false);

            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(order.Email, "Buyer") };
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "new-order-buyer-template.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendEditedCampaignMessageToSeller(int campaignId, string pathToMedia, string pathToTemplates)
        {
            var campaign = _campaignRepository.Get(campaignId);

            var sellerCulture =
                "en-MY"; //_userPartRepository.Table.Where(u => u.Id == campaign.TeeyootUserId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = "noreply@teeyoot.com";
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("edited-campaign-template", sellerCulture);

            var seller =
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId);
            List<CampaignProductRecord> orderedProducts =
                _campaignProductRepository.Table.Where(
                    prod => prod.CampaignRecord_Id == campaign.Id && prod.WhenDeleted == null).ToList();

            FillCampaignProductsMergeVars(mandrillMessage, orderedProducts, pathToMedia, seller.Email);
            FillCampaignMergeVars(mandrillMessage, campaign.Id, seller.Email, pathToMedia, pathToTemplates, false);
            FillAdditionalCampaignMergeVars(mandrillMessage, campaign.Id, seller.Email, pathToMedia, pathToTemplates);

            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(seller.Email, "Seller") };
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "edited-campaign-template.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendPayoutRequestMessageToAdmin(int userId, string accountNumber, string bankName,
            string accHoldName, string contNum, string messAdmin)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var userIds = _userRolesPartRepository.Table.Where(x => x.Role.Name == "Administrator")
                .Select(x => x.UserId);
            var admins = _contentManager.GetMany<IUser>(userIds, VersionOptions.Published, QueryHints.Empty);
            foreach (var admin in admins)
            {
                var teeyoot_user_part = admin.As<TeeyootUserPart>();
                if (teeyoot_user_part != null && !teeyoot_user_part.ReceiveEmail) continue; 

                var adminCulture = "en-MY"; //_userPartRepository.Table.Where(u => u.Id == admin.Id).First().CultureRecord.Culture;

                var api = new MandrillApi(_settingsService.GetSettingByCulture(adminCulture).List().First().ApiKey);
                var mandrillMessage = new MandrillMessage() { };
                mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
                mandrillMessage.FromEmail = "noreply@teeyoot.com";
                mandrillMessage.FromName = "Teeyoot";
                mandrillMessage.Subject = _mailSubjectService.GetMailSubject("withdraw-template", adminCulture);

                FillPayoutRequestMergeVars(mandrillMessage, admin.Email, userId, accountNumber, bankName, accHoldName,
                    contNum, messAdmin, 0.00, "");

                mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(admin.Email, "Admin") };
                mandrillMessage.Html =
                    System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "withdraw-template.html");
                SendTmplMessage(api, mandrillMessage);
            }
        }

        public void SendPayoutRequestMessageToSeller(int userId, string accountNumber, string bankName,
            string accHoldName, string contNum)
        {
            string pathToMedia = AppDomain.CurrentDomain.BaseDirectory;
            string pathToTemplates = Path.Combine(pathToMedia, "Modules/Teeyoot.Module/Content/message-templates/");

            var sellerCulture = "en-MY";//_userPartRepository.Table.Where(u => u.Id == userId).First().CultureRecord.Culture;

            var api = new MandrillApi(_settingsService.GetSettingByCulture(sellerCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = "noreply@teeyoot.com";
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("withdraw-seller-template", sellerCulture);

            var user = _contentManager.Get<UserPart>(userId, VersionOptions.Latest);

            FillPayoutRequestMergeVars(mandrillMessage, user.Email, userId, accountNumber, bankName, accHoldName,
                contNum, "", 0.00, "");

            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(user.Email, "Seller") };
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "withdraw-seller-template.html");
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendOrderStatusMessage(string pathToTemplates, string pathToMedia, int orderId, string orderStatus)
        {
            var order = _orderRepository.Get(orderId);

            var buyerCulture = "en-MY";// (order.BuyerCultureRecord == null) ? "en-MY" : order.BuyerCultureRecord.Culture;
            var settings = _settingsService.GetSettingByCulture(buyerCulture).List();
            MandrillApi api = null;
            if (settings == null || settings.Count() == 0)
            {
                api = new MandrillApi(_settingsService.GetSettingByCulture("en-MY").List().First().ApiKey);
            }
            else
            {
                api = new MandrillApi(_settingsService.GetSettingByCulture(buyerCulture).List().First().ApiKey);
            }
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";

            switch (orderStatus)
            {
                case "Approved":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("place-order-template", buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "place-order-template.html");

                        FillUserMergeVars(mandrillMessage, order);
                        FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                            order.Email, pathToMedia, pathToTemplates, false);
                        FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                        break;
                    }
                    ;
                case "Printing":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("order-is-printing-buyer-template",
                            buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "order-is-printing-buyer-template.html");

                        FillUserMergeVars(mandrillMessage, order);
                        FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                            order.Email, pathToMedia, pathToTemplates, false);
                        FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                        break;
                    }
                    ;
                case "Shipped":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("shipped-order-template", buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "shipped-order-template.html");

                        FillUserMergeVars(mandrillMessage, order);
                        FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                            order.Email, pathToMedia, pathToTemplates, false);
                        FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                        break;
                    }
                    ;
                case "Delivered":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("delivered-order-template",
                            buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "delivered-order-template.html");

                        FillUserMergeVars(mandrillMessage, order);
                        FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                            order.Email, pathToMedia, pathToTemplates, false);
                        FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                        break;
                    }
                    ;
                case "Cancelled":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("cancelled-order-template",
                            buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "cancelled-order-template.html");

                        FillUserMergeVars(mandrillMessage, order);
                        FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                            order.Email, pathToMedia, pathToTemplates, false);
                        FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                        break;
                    }
                case "Refunded":
                    {
                        mandrillMessage.Subject = _mailSubjectService.GetMailSubject("refounded-order-template",
                           buyerCulture);
                        mandrillMessage.Html =
                            System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "refunded-order-template.html");

                        FillUserMergeVars(mandrillMessage, order);
                        FillCampaignMergeVars(mandrillMessage, order.Products[0].CampaignProductRecord.CampaignRecord_Id,
                            order.Email, pathToMedia, pathToTemplates, false);
                        FillProductsMergeVars(mandrillMessage, order.Products, pathToMedia, order.Email, order.OrderPublicId);
                        break;
                    }

            }
            mandrillMessage.To = new List<MandrillMailAddress>() { new MandrillMailAddress(order.Email, "Buyer") };
            SendTmplMessage(api, mandrillMessage);
        }

        public void SendRecoverOrderMessage(string pathToTemplates, IList<OrderRecord> orders, string email)
        {
            var currentCulture = "en-MY";//_wca.GetContext().CurrentCulture.Trim();

            var api = new MandrillApi(_settingsService.GetSettingByCulture(currentCulture).List().First().ApiKey);
            var mandrillMessage = new MandrillMessage();
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.FromName = "Teeyoot";
            mandrillMessage.Subject = _mailSubjectService.GetMailSubject("recover_orders_for_buyer", currentCulture);
            mandrillMessage.To = new List<MandrillMailAddress> { new MandrillMailAddress(email, "Buyer") };
            FillOrdersMergeVars(mandrillMessage, orders, email, pathToTemplates);
            mandrillMessage.Html =
                System.IO.File.ReadAllText(pathToTemplates + "/en-MY/" + "recover_orders_for_buyer.html");
            SendTmplMessage(api, mandrillMessage);
        }

        private void FillOrdersMergeVars(MandrillMessage message, IList<OrderRecord> orders, string email,
            string orderPublicId)
        {
            List<Dictionary<string, object>> ordersList = new List<Dictionary<string, object>>();
            foreach (var item in orders)
            {
                int index = orders.IndexOf(item);
                int quantity = item.Products.Sum(m => m.Count);
                var campaign =
                    _campaignRepository.Get(item.Products.FirstOrDefault().CampaignProductRecord.CampaignRecord_Id);

                ordersList.Add(new Dictionary<string, object>
                {
                    {"id", item.OrderPublicId},
                    {"quantity", quantity},
                    {"campaign", campaign.Title},
                    {"created", item.Created.ToLocalTime().ToString()}
                });

            }
            var arr = ordersList.ToArray();
            message.AddRcptMergeVars(email, "ORDERS", ordersList.ToArray());
        }

        private void FillCampaignMergeVars(MandrillMessage message, int campaignId, string email, string pathToMedia,
            string pathToTemplates, bool forAdmin)
        {
            var baseUrl = "";
            string remaining = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                baseUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') +
                          "/";
            }
            else
            {
                baseUrl = _wca.GetContext().CurrentSite.BaseUrl + "/";
            }
            string side = "";
            var campaign = _campaignRepository.Get(campaignId);

            if (campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days > 0)
            {
                remaining = campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days + " days";
            }
            else if (campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days <= -1)
            {
                remaining = Math.Abs(campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days) + "days ago";
            }
            else
            {
                if (campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Hours > 0)
                {
                    remaining = campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Hours + "hours";
                }
                else
                {
                    remaining = Math.Abs(campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Hours) + "hours ago";
                }
            }


            if (campaign.BackSideByDefault)
            {
                side = "back";
            }
            else
            {
                side = "front";
            }
            if(forAdmin) message.AddRcptMergeVars(email, "CampaignTitle", campaign.Title + "(" + campaign.Id + ")");
            else message.AddRcptMergeVars(email, "CampaignTitle", campaign.Title);
            message.AddRcptMergeVars(email, "Campaignremaining", remaining);
            message.AddRcptMergeVars(email, "Url", baseUrl);
            message.AddRcptMergeVars(email, "CampaignAlias", campaign.Alias);
            message.AddRcptMergeVars(email, "ReservedCount", campaign.ProductCountSold.ToString());
            message.AddRcptMergeVars(email, "Goal", campaign.ProductCountGoal.ToString());
            message.AddRcptMergeVars(email, "SellerEmail",
                _contentManager.Query<UserPart, UserPartRecord>()
                    .List()
                    .FirstOrDefault(user => user.Id == campaign.TeeyootUserId)
                    .Email);
            message.AddRcptMergeVars(email, "CampaignPreviewUrl",
                baseUrl + "/Media/campaigns/" + campaign.Id + "/" +
                campaign.Products.First(p => p.WhenDeleted == null).Id + "/normal/" + side + ".png");
            message.AddRcptMergeVars(email, "VideoPreviewUrl",
                baseUrl + "/Media/Default/images/video_thumbnail_521x315.jpg/");
            message.AddRcptMergeVars(email, "CurrencyFlagFileName",
                baseUrl.TrimEnd('/') + campaign.CurrencyRecord.FlagFileName);
        }

        private void FillRelaunchCampaignMergeVars(MandrillMessage message, int campaignId, string email,
            string pathToMedia, string pathToTemplates, bool forAdmin)
        {
            var baseUrl = "";
            string remaining = "";
            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                baseUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') +
                          "/";
            }
            else
            {
                baseUrl = _wca.GetContext().CurrentSite.BaseUrl + "/";
            }
            string side = "";
            var campaign = _campaignRepository.Get(campaignId);

            if (campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days > 0)
            {
                remaining = campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days + " days";
            }
            else if (campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days <= -1)
            {
                remaining = Math.Abs(campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Days) + "days ago";
            }
            else
            {
                if (campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Hours > 0)
                {
                    remaining = campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Hours + "hours";
                }
                else
                {
                    remaining = Math.Abs(campaign.EndDate.ToLocalTime().Subtract(DateTime.UtcNow).Hours) + "hours ago";
                }
            }


            if (campaign.BackSideByDefault)
            {
                side = "back";
            }
            else
            {
                side = "front";
            }
            if (forAdmin) message.AddRcptMergeVars(email, "CampaignTitle", campaign.Title + "(" + campaign.Id + ")");
            else message.AddRcptMergeVars(email, "CampaignTitle", campaign.Title);
            message.AddRcptMergeVars(email, "Url", baseUrl);
            message.AddRcptMergeVars(email, "Campaignremaining", remaining);
            message.AddRcptMergeVars(email, "CampaignEndDate", campaign.EndDate.ToLocalTime().ToShortDateString());
            message.AddRcptMergeVars(email, "CampaignAlias", campaign.Alias);
            message.AddRcptMergeVars(email, "CampaignPreviewUrl",
                baseUrl + "/Media/campaigns/" + campaign.Id + "/" +
                campaign.Products.First(p => p.WhenDeleted == null).Id + "/normal/" + side + ".png");
            message.AddRcptMergeVars(email, "CurrencyFlagFileName",
                baseUrl.TrimEnd('/') + campaign.CurrencyRecord.FlagFileName);
        }

        private void FillAdditionalCampaignMergeVars(MandrillMessage message, int campaignId, string email,
            string pathToMedia, string pathToTemplates)
        {
            var campaign = _campaignRepository.Get(campaignId);
            message.AddRcptMergeVars(email, "Description", campaign.Description);
            message.AddRcptMergeVars(email, "Expiration", campaign.EndDate.ToShortDateString());
            message.AddRcptMergeVars(email, "Profit", campaign.CampaignProfit);

        }

        private void FillUserMergeVars(MandrillMessage message, OrderRecord record)
        {

            message.AddRcptMergeVars(record.Email, "FNAME", record.FirstName);
            message.AddRcptMergeVars(record.Email, "LNAME", record.LastName);
            message.AddRcptMergeVars(record.Email, "CITY", record.City);
            message.AddRcptMergeVars(record.Email, "CLIENT_EMAIL", record.Email);
            message.AddRcptMergeVars(record.Email, "PHONE", record.PhoneNumber);
            message.AddRcptMergeVars(record.Email, "STATE", record.State);
            message.AddRcptMergeVars(record.Email, "POSTAL_CODE", record.PostalCode);
            message.AddRcptMergeVars(record.Email, "STREET_ADDRESS", record.StreetAddress);
            message.AddRcptMergeVars(record.Email, "COUNTRY", record.Country);

            double totalPrice;

            if (record.TotalPriceWithPromo > 0.0)
            {
                totalPrice = _priceConvert.ConvertPrice(record.TotalPriceWithPromo + record.Delivery, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value;
                message.AddRcptMergeVars(record.Email, "TOTALPRICE",
                    totalPrice.ToString("F", CultureInfo.InvariantCulture));
            }
            else
            {
                totalPrice = _priceConvert.ConvertPrice(record.TotalPrice + record.Delivery, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value;
                message.AddRcptMergeVars(record.Email, "TOTALPRICE",
                    totalPrice.ToString("F", CultureInfo.InvariantCulture));
            }
            if (record.Delivery > 0.0)
            {
                message.AddRcptMergeVars(record.Email, "DELIVERYPRICE",
                    _priceConvert.ConvertPrice(record.Delivery, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value.ToString("F", CultureInfo.InvariantCulture));
            }

            if (record.TotalPriceWithPromo > 0.0)
            {
                message.AddRcptMergeVars(record.Email, "PROMOSIZE",
                     _priceConvert.ConvertPrice(record.Promotion, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value.ToString("F", CultureInfo.InvariantCulture));
            }
            else
            {
                message.AddRcptMergeVars(record.Email, "PROMOSIZE",
                    "0.00");
            }
        }

        private void FillPayoutRequestMergeVars(MandrillMessage message, string adminEmail, int userId,
            string accountNumber, string bankName, string accHoldName, string contNum, string messAdmin, double amount,
            string currencyCode)
        {

            var baseUrl = "";

            if (HttpContext.Current != null)
            {
                var request = HttpContext.Current.Request;
                baseUrl = request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/') +
                          "/";
            }
            else
            {
                baseUrl = _wca.GetContext().CurrentSite.BaseUrl + "/";
            }
            var requester = _contentManager.Get<TeeyootUserPart>(userId, VersionOptions.Latest);

            message.AddRcptMergeVars(adminEmail, "Requester_Name", requester.PublicName);
            message.AddRcptMergeVars(adminEmail, "AccountNumber", accountNumber);
            message.AddRcptMergeVars(adminEmail, "BankName", bankName);
            message.AddRcptMergeVars(adminEmail, "AccHolderName", accHoldName);
            message.AddRcptMergeVars(adminEmail, "ContactNumber", contNum);
            message.AddRcptMergeVars(adminEmail, "Text", messAdmin);
            message.AddRcptMergeVars(adminEmail, "Amount", amount.ToString("F", CultureInfo.InvariantCulture));
            message.AddRcptMergeVars(adminEmail, "Currency", currencyCode);
            message.AddRcptMergeVars(adminEmail, "Url", baseUrl);
        }

        private void FillUserMergeVars(MandrillMessage message, OrderRecord record, string adminEmail)
        {
            message.AddRcptMergeVars(adminEmail, "FNAME", record.FirstName);
            message.AddRcptMergeVars(adminEmail, "LNAME", record.LastName);
            message.AddRcptMergeVars(adminEmail, "CITY", record.City);
            message.AddRcptMergeVars(adminEmail, "CLIENT_EMAIL", record.Email);
            message.AddRcptMergeVars(adminEmail, "STATE", record.State);
            message.AddRcptMergeVars(adminEmail, "PHONE", record.PhoneNumber);
            message.AddRcptMergeVars(adminEmail, "POSTAL_CODE", record.PostalCode);
            message.AddRcptMergeVars(adminEmail, "POSTALCODE", record.PostalCode);
            message.AddRcptMergeVars(adminEmail, "COUNTRY", record.Country);
            message.AddRcptMergeVars(adminEmail, "STREET_ADDRESS", record.StreetAddress);

            double totalPrice;

            if (record.TotalPriceWithPromo > 0.0)
            {
                totalPrice = record.TotalPriceWithPromo + record.Delivery;
                totalPrice = _priceConvert.ConvertPrice(totalPrice, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value;
                
                message.AddRcptMergeVars(adminEmail, "TOTALPRICE",
                    totalPrice.ToString("F", CultureInfo.InvariantCulture));
            }
            else
            {
                totalPrice = record.TotalPrice + record.Delivery;
                
                totalPrice = _priceConvert.ConvertPrice(totalPrice, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value;

                message.AddRcptMergeVars(adminEmail, "TOTALPRICE",
                    totalPrice.ToString("F", CultureInfo.InvariantCulture));
            }
            if (record.Delivery > 0.0)
            {

                message.AddRcptMergeVars(adminEmail, "DELIVERYPRICE",
                   _priceConvert.ConvertPrice(record.Delivery, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value.ToString("F", CultureInfo.InvariantCulture));
            }
            else
            {
                message.AddRcptMergeVars(adminEmail, "DELIVERYPRICE", "0.00");
            }
            if (record.Promotion > 0.0)
            {
                message.AddRcptMergeVars(record.Email, "PROMOSIZE",
                    _priceConvert.ConvertPrice(record.Promotion, record.Campaign.CurrencyRecord, record.CurrencyRecord.Code).Value.ToString("F", CultureInfo.InvariantCulture));
            }
            else
            {
                message.AddRcptMergeVars(record.Email, "PROMOSIZE", "0.00");
            }
            

        }

        private void FillProductsMergeVars(MandrillMessage message,
            IList<LinkOrderCampaignProductRecord> orderedProducts, string pathToMedia, string email,
            string orderPublicId)
        {
            string baseUrl = _wca.GetContext().CurrentSite.BaseUrl + "/";
            List<Dictionary<string, object>> products = new List<Dictionary<string, object>>();
            var order = _orderRepository.Fetch(aa=>aa.OrderPublicId == orderPublicId).FirstOrDefault();
            foreach (var item in orderedProducts)
            {

                string side = "";
                var campaign = _campaignRepository.Get(item.CampaignProductRecord.CampaignRecord_Id);
                if (campaign.BackSideByDefault)
                {
                    side = "back";
                }
                else
                {
                    side = "front";
                }
                int index = orderedProducts.IndexOf(item);
                int idSize = item.ProductSizeRecord.Id;
                float costSize = 0;
                try
                {
                    costSize = item.CampaignProductRecord.ProductRecord.SizesAvailable.Where(c => c.ProductSizeRecord.Id == idSize).Count() > 0 ?
                     item.CampaignProductRecord.ProductRecord.SizesAvailable.Where(c => c.ProductSizeRecord.Id == idSize).First().SizeCost : 0;
                }
                catch
                {
                    costSize = 0;
                }
                float price = (float)item.CampaignProductRecord.Price + costSize;
                string prodColor = "";
                if (item.ProductColorRecord != null)
                {
                    if (item.CampaignProductRecord.ProductColorRecord.Id == item.ProductColorRecord.Id)
                    {
                        prodColor = item.CampaignProductRecord.Id.ToString();
                    }
                    else
                    {
                        prodColor = item.CampaignProductRecord.Id + "_" + item.ProductColorRecord.Id.ToString();
                    }
                }
                else
                {
                    prodColor = item.CampaignProductRecord.Id.ToString();
                }
                products.Add(new Dictionary<string, object>
                {
                    {"quantity", item.Count},
                    {"name", item.CampaignProductRecord.ProductRecord.Name},
                    {"description", item.CampaignProductRecord.ProductRecord.Details},
                    {"price", price},
                    {"size", item.ProductSizeRecord.SizeCodeRecord.Name},
                    {"currency", item.OrderRecord.CurrencyRecord.Code},
                    {"total_price", _priceConvert.ConvertPrice((price*item.Count), item.OrderRecord.Campaign.CurrencyRecord, item.OrderRecord.CurrencyRecord.Code).Value.ToString("F", CultureInfo.InvariantCulture)},
                    {
                        "preview_url",
                        baseUrl + "/Media/campaigns/" + item.CampaignProductRecord.CampaignRecord_Id + "/" + prodColor +
                        "/normal/" + side + ".png"
                    }
                });

            }
            var arr = products.ToArray();
            message.AddRcptMergeVars(email, "PRODUCTS", products.ToArray());
            message.AddRcptMergeVars(email, "orderPublicId", orderPublicId);
            message.AddRcptMergeVars(email, "order_id", (order == null) ? "" : order.Id.ToString());
            message.AddRcptMergeVars(email, "Campaign_name", (order == null) ? "" : order.Campaign.Title + "(" + order.Campaign.Id + ")");
        }

        private void FillCampaignProductsMergeVars(MandrillMessage message,
            IList<CampaignProductRecord> campaignProducts, string pathToMedia, string email)
        {
            string baseUrl = _wca.GetContext().CurrentSite.BaseUrl + "/";
            List<Dictionary<string, object>> products = new List<Dictionary<string, object>>();
            foreach (var item in campaignProducts)
            {
                string side = "";
                var campaign = _campaignRepository.Get(item.CampaignRecord_Id);
                if (campaign.BackSideByDefault)
                {
                    side = "back";
                }
                else
                {
                    side = "front";
                }
                products.Add(new Dictionary<string, object>
                {
                    {"name", item.ProductRecord.Name},
                    {"price", item.Price},
                    {"currency", item.CurrencyRecord.Code},
                    {
                        "preview_url",
                        baseUrl + "/Media/campaigns/" + item.CampaignRecord_Id + "/" + item.Id + "/normal/" + side +
                        ".png"
                    }
                });
            }
            var arr = products.ToArray();
            message.AddRcptMergeVars(email, "PRODUCTS", products.ToArray());
        }

        private void FillSellerToBuyersProductsMergeVars(MandrillMessage message,
            IList<LinkOrderCampaignProductRecord> orderedProducts, string pathToMedia, string email,
            string orderPublicId)
        {
            string products = "";
            var i = 0;
            //List<Dictionary<string, object>> products = new List<Dictionary<string, object>>();
            foreach (var item in orderedProducts)
            {

                int index = orderedProducts.IndexOf(item);
                int idSize = item.ProductSizeRecord.Id;
                float costSize =
                    item.CampaignProductRecord.ProductRecord.SizesAvailable.Where(c => c.ProductSizeRecord.Id == idSize)
                        .First()
                        .SizeCost;
                float price = (float)item.CampaignProductRecord.Price + costSize;
                if (i > 0)
                {
                    products += item.Count.ToString() + " x " + item.ProductSizeRecord.SizeCodeRecord.Name + " " +
                                item.CampaignProductRecord.ProductRecord.Name + ", " + Environment.NewLine;
                }
                else
                {
                    products += item.Count.ToString() + " x " + item.ProductSizeRecord.SizeCodeRecord.Name + " " +
                                item.CampaignProductRecord.ProductRecord.Name + Environment.NewLine;
                }
                i++;


            }
            message.AddRcptMergeVars(email, "PRODUCTS", products);
            message.AddRcptMergeVars(email, "orderPublicId", orderPublicId);
        }

        private MandrillMessage InitMandrillMessage(OrderRecord order)
        {
            var mandrillMessage = new MandrillMessage() { };
            mandrillMessage.MergeLanguage = MandrillMessageMergeLanguage.Handlebars;
            mandrillMessage.FromEmail = ADMIN_EMAIL;
            mandrillMessage.Subject = "Your order";
            List<MandrillMailAddress> emails = new List<MandrillMailAddress>();
            emails.Add(new MandrillMailAddress(order.Email));
            mandrillMessage.To = emails;
            return mandrillMessage;
        }

        private string SendTmplMessage(MandrillApi mAPI, Mandrill.Model.MandrillMessage message)
        {
            //#if DEBUG
            //            return "SENT :)";
            //#else
            //    //var result = mAPI.Messages.Send(message);
            //    //return result.ToString();
            //#endif
            Outbox o = new Outbox()
            {
                Added = DateTime.Now,
                Data = Newtonsoft.Json.JsonConvert.SerializeObject(message),
            };

            _mailOutBox.Create(o);
            return "send :)";
        }
    }
}
