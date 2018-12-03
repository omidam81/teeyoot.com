using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Localization.Records;

namespace Teeyoot.Module.Models
{
    public class BringBackCampaignRecord
    {
        public virtual int Id { get; set; }

        public virtual CampaignRecord CampaignRecord { get; set; }

        public virtual string Email { get; set; }

        public virtual CultureRecord BuyerCultureRecord { get; set; }
    }
}