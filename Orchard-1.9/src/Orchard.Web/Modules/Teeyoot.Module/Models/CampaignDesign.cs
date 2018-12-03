using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teeyoot.Module.Models
{
    public class CampaignDesign
    {
        public virtual int Id { get; set; }
        
        [StringLengthMax]
        public virtual string Data { get; set; }
    }
}