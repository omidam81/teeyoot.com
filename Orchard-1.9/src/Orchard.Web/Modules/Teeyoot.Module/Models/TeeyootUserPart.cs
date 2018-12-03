using System;
using Orchard.ContentManagement;

namespace Teeyoot.Module.Models
{
    public class TeeyootUserPart : ContentPart<TeeyootUserPartRecord>
    {
        public DateTime CreatedUtc
        {
            get { return Retrieve(p => p.CreatedUtc); }
            set { Store(p => p.CreatedUtc, value); }
        }

        public string PublicName
        {
            get { return Retrieve(p => p.PublicName); }
            set { Store(p => p.PublicName, value); }
        }

        public string PhoneNumber
        {
            get { return Retrieve(p => p.PhoneNumber); }
            set { Store(p => p.PhoneNumber, value); }
        }

        public string Street
        {
            get { return Retrieve(p => p.Street); }
            set { Store(p => p.Street, value); }
        }

        public string Suit
        {
            get { return Retrieve(p => p.Suit); }
            set { Store(p => p.Suit, value); }
        }

        public string City
        {
            get { return Retrieve(p => p.City); }
            set { Store(p => p.City, value); }
        }

        public string State
        {
            get { return Retrieve(p => p.State); }
            set { Store(p => p.State, value); }
        }

        public string Zip
        {
            get { return Retrieve(p => p.Zip); }
            set { Store(p => p.Zip, value); }
        }

        public string Country
        {
            get { return Retrieve(p => p.Country); }
            set { Store(p => p.Country, value); }
        }

        public string TeeyootUserCulture
        {
            get { return Retrieve(p => p.TeeyootUserCulture); }
            set { Store(p => p.TeeyootUserCulture, value); }
        }

        public CountryRecord CountryRecord
        {
            get { return Record.CountryRecord; }
            set { Record.CountryRecord = value; }
        }

        public CurrencyRecord CurrencyRecord
        {
            get { return Record.CurrencyRecord; }
            set { Record.CurrencyRecord = value; }
        }

        public string DefaultFBPixelId
        {
            get { return Retrieve(p => p.DefaultFBPixelId); }
            set { Store(p => p.DefaultFBPixelId, value); }
        }
        public string DefaultTwitterPixelId
        {
            get { return Retrieve(p => p.DefaultTwitterPixelId); }
            set { Store(p => p.DefaultTwitterPixelId, value); }
        }
        public string DefaultPinterestPixelId
        {
            get { return Retrieve(p => p.DefaultPinterestPixelId); }
            set { Store(p => p.DefaultPinterestPixelId, value); }
        }
        public string DefaultGooglePixelId
        {
            get { return Retrieve(p => p.DefaultGooglePixelId); }
            set { Store(p => p.DefaultGooglePixelId, value); }
        }
        public string DefaultGoogleLabelPixelId
        {
            get { return Retrieve(p => p.DefaultGoogleLabelPixelId); }
            set { Store(p => p.DefaultGoogleLabelPixelId, value); }
        }
        public string DefaultGoogleAnalyticsTrackingSnippet
        {
            get { return Retrieve(p => p.DefaultGoogleAnalyticsTrackingSnippet); }
            set { Store(p => p.DefaultGoogleAnalyticsTrackingSnippet, value); }
        }
        public string DefaultFacebookCustomAudiencePixel
        {
            get { return Retrieve(p => p.DefaultFacebookCustomAudiencePixel); }
            set { Store(p => p.DefaultFacebookCustomAudiencePixel, value); }
        }

        public bool ReceiveEmail
        {
            get { return Retrieve(p => p.ReceiveEmail); }
            set { Store(p => p.ReceiveEmail, value); }
        }
    }
}