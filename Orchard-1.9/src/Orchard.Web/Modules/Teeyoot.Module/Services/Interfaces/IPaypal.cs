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
using Teeyoot.Localization;
using PayPal.Api;

namespace Teeyoot.Module.Services.Interfaces
{
    public interface IPaypal : IDependency
    {
        APIContext GetAPIContext();
        Payment CreatePayment(OrderRecord order, string cancelUrl, string returnUrl);
        Payment ConfirmPayment(string token, string payerId, OrderRecord order);
        Amount CreateAmount(OrderRecord order);
        


        /*
         Direct Credit card payment
         */

        string PayWithCreditCard(CreditCardInfo info, OrderRecord rec);

        string GetAccessToken();
    }
}
