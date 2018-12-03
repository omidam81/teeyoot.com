using System;
using Orchard.Localization.Records;

namespace Teeyoot.Module.Models
{
    public class CheckoutCampaignRequest
    {
        public virtual int Id { get; protected set; }
        public virtual DateTime RequestUtcDate { get; set; }
        public virtual string Email { get; set; }
        public virtual DateTime? EmailSentUtcDate { get; set; }
        public virtual CultureRecord BuyerCultureRecord { get; set; }
    }
}