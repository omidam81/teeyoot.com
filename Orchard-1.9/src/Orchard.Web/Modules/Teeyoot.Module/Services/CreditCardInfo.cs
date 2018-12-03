using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teeyoot.Module.Models
{
    public 
        class CreditCardInfo
    {
        public string CardLastFour { get; set; }

        public string CardType { get; set; }

        public string ExpirationYear { get; set; }

        public string ExpirationMonth { get; set; }

        public string SecurityCode { get; set; }

        public string CardNumber { get; set; }

        public string LastName { get; set; }

        public string Name { get; set; }
    }
}
