using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Teeyoot.Module.Models
{
    public class CountryRecord
    {
        public virtual int Id { get; protected set; }
        public virtual string Code { get; set; }
        public virtual string Name { get; set; }
        public virtual IList<LinkCountryCurrencyRecord> CountryCurrencies { get; set; }

        [CascadeAllDeleteOrphan]
        public virtual IList<LinkCountryCultureRecord> CountryCultures { get; set; }

        public virtual LinkCountryCultureRecord DefaultCulture { get; set; }

        public CountryRecord()
        {
            CountryCurrencies = new List<LinkCountryCurrencyRecord>();
            CountryCultures = new List<LinkCountryCultureRecord>();
        }
    }
}
