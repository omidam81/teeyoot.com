using Orchard.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teeyoot.Dashboard.ViewModels;
using Teeyoot.Module.Models;
using Teeyoot.Module.Services;

namespace Teeyoot.Dashboard.Controllers
{
    public partial class DashboardController : Controller
    {

        public ActionResult Accounts()
        {
            int currentUserId = Services.WorkContext.CurrentUser.Id;
            var currentUser = Services.WorkContext.CurrentUser;
            var teeUser = _contentManager.Get<TeeyootUserPart>(currentUser.Id, VersionOptions.Latest);

            var payouts = _payoutService.GetAllPayouts().Where(aa => aa.UserId == currentUserId && aa.IsOrder == false);
            var list = payouts.ToList();

            var model = new PayoutPageView();
            model.Currency = (teeUser.CurrencyRecord != null) ? teeUser.CurrencyRecord : _currencyRepository.Table.FirstOrDefault(aa => aa.Code == "USD");
            model.Payouts = payouts.OrderByDescending(aa => aa.Date).Take(100).ToList();
            model.UserID = currentUserId;
            try
            {
                model.UnclaimableProfit = 0;

                foreach (var campaignRecod in _campaignService.GetCampaignsOfUser(currentUserId).Where(c=> c.IsApproved))
                {
                    _campaignService.CalculateCampaignProfit(campaignRecod.Id);
                    //model.ClaimableProfit += _priceConversionService.ConvertPrice(campaignRecod.ClaimableProfit, campaignRecod.CurrencyRecord, model.Currency.Code).Value;
                    //.Select(c => c.UnclaimableProfit).Sum();
                    model.UnclaimableProfit += _priceConversionService.ConvertPrice(campaignRecod.UnclaimableProfit, campaignRecod.CurrencyRecord, model.Currency.Code).Value;
                }
            }
            catch
            {
                model.UnclaimableProfit = 0;
            }
            model.ClaimableProfit = 0;
            foreach (var item in payouts.Where(aa => aa.Status != "Pending" && aa.IsOrder == false))
            {
                var campaign = _campaignService.GetCampaignById(item.CampaignId);
                if (campaign != null)
                {

                    model.ClaimableProfit += _priceConversionService.ConvertPrice((item.IsPlus) ? item.Amount : (-1 * item.Amount), campaign.CurrencyRecord, model.Currency.Code).Value;
                    //Response.Write(model.ClaimableProfit);
                }
                else
                {
                    var currencyRec = _currencyRepository.Table.FirstOrDefault(aa => aa.Id == item.Currency_Id);


                    if (currencyRec != null)
                    {
                        model.ClaimableProfit += _priceConversionService.ConvertPrice((item.IsPlus) ? item.Amount : (-1 * item.Amount), currencyRec, model.Currency.Code).Value;
                    }
                    else
                    {
                        model.ClaimableProfit += (item.IsPlus) ? item.Amount : (-1 * item.Amount);
                    }
                }
            }
                //.Select(aa => (aa.IsPlus) ? aa.Amount : -aa.Amount).ToArray().Sum();
            var processing = _payoutService.GetAllPayouts().Where(aa => aa.UserId == currentUser.Id && aa.Status == "Pending");
            //if (processing == null || processing.Count() == 0) model.Processing = 0;
            //else model.Processing = processing.Sum(aa => aa.Amount);
            model.Processing = processing;
            if (processing == null || processing.Count() == 0)
            {
                model.ProcessingAmount = 0;
            }
            else
            {
                model.ProcessingAmount = processing.Select(aa => aa.Amount).ToArray().Sum();
            }
            ViewBag.PriceConversionService = _priceConversionService;
            return View(model);
        }

        public ActionResult StartPayout()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendMail(string accountNumber, string bankName, string accHoldName, string contNum, string messAdmin, int currId)
        {
            int currentUserId = Services.WorkContext.CurrentUser.Id;

            //var payouts = _payoutService.GetAllPayouts().ToList();
            var balances = _payoutService.GetAllPayouts().Where(aa => aa.UserId == currentUserId && aa.IsOrder == false && aa.Status != "Pending");//.Select(aa => aa.Amount).Sum();
            double balance = 0;

            foreach (var bal in balances)
            {
                balance += (bal.IsPlus) ? bal.Amount : (-1) * bal.Amount;
            }
            

            var payout = new PayoutRecord()
            {
                Date = DateTime.Now,
                Amount = balance,
                Event = T("You requested a payout").ToString(),
                Currency_Id = currId,
                IsPlus = false,
                UserId = currentUserId,
                Status = "Pending",
                IsProfitPaid = false,
                Description = "You requested a payout"
            };
            _payoutService.AddPayout(payout);
            _paymentInfService.AddPayment(new PaymentInformationRecord
            {
                AccountNumber = accountNumber,
                AccountHolderName = accHoldName,
                BankName = bankName,
                ContactNumber = contNum,
                MessAdmin = messAdmin,
                TranzactionId = payout.Id
            });

            _teeyootMessagingService.SendPayoutRequestMessageToAdmin(currentUserId, accountNumber, bankName, accHoldName, contNum, messAdmin);
            _teeyootMessagingService.SendPayoutRequestMessageToSeller(currentUserId, accountNumber, bankName, accHoldName, contNum);

            return RedirectToAction("Accounts");
        }

        public ActionResult Recalculate(int userID)
        {
            var campaigns = _campaignService.GetCampaignsOfUser(userID);
            campaigns = campaigns.Where(c=> c.IsApproved);
            foreach (var campaign in campaigns)
            {
                var isSuccesfull = campaign.ProductMinimumGoal <= campaign.ProductCountSold;
                if (isSuccesfull) _campaignService.CalculateCampaignProfit(campaign.Id);
                if (isSuccesfull && campaign.ClaimableProfit > 0) _campaignService.CreatePayoutData(campaign.Id);
            }
            return Redirect("Accounts");
        }
    }
}