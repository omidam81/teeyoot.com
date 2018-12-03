using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teeyoot.Module.Models
{
    public class Outbox
    {
        [StringLengthMax]
        public virtual string Data { get; set; }
        public virtual int Id { get; set; }
        public virtual DateTime Added { get; set; }

        public virtual int? OrderId { get; set; }

        public virtual string EmailType { get; set; }
    }
}