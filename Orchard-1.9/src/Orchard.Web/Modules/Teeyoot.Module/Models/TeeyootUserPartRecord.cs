using System;
using Orchard.ContentManagement.Records;

namespace Teeyoot.Module.Models
{
    public class TeeyootUserPartRecord : ContentPartRecord
    {
        public virtual DateTime CreatedUtc { get; set; }
        public virtual string PublicName { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string Street { get; set; }
        public virtual string Suit { get; set; }
        public virtual string City { get; set; }
        public virtual string State { get; set; }
        public virtual string Zip { get; set; }
        public virtual string Country { get; set; }
        public virtual string TeeyootUserCulture { get; set; }
        public virtual CountryRecord CountryRecord { get; set; }
        public virtual CurrencyRecord CurrencyRecord { get; set; }

        public virtual bool ReceiveEmail { get; set; }



        public virtual string DefaultFBPixelId { get; set; }


        public virtual string DefaultTwitterPixelId { get; set; }
        public virtual string DefaultPinterestPixelId { get; set; }
        public virtual string DefaultGooglePixelId { get; set; }
        public virtual string DefaultGoogleLabelPixelId { get; set; }
        public virtual string DefaultGoogleAnalyticsTrackingSnippet { get; set; }
        public virtual string DefaultFacebookCustomAudiencePixel { get; set; }
    }
}