using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using System.Web.Mvc;

namespace Orchard.Users.ViewModels {
    public class UserSettingsViewModel  {

        public int Id { get; set; }
        
        public string PublicName { get; set; }
        [Phone]
        public string PhoneNumber { get; set; }

        public string Street1 { get; set; }

        public string Street2 { get; set; }

        public string Suit { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Country { get; set; }

        public string CurrentEmail { get; set; }

        [EmailAddress]
        public string NewEmailAddress { get; set; }

        [EmailAddress]
        public string ConfirmNewEmailAddress { get; set; }
        
        public string CurrentPassword { get; set; }
       
        public string NewPassword { get; set; }
      
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }

        public string InfoMessage { get; set; }

        public string DefaultFBPixelId { get; set; }
        public string DefaultTwitterPixelId { get; set; }
        public string DefaultPinterestPixelId { get; set; }
        public string DefaultGooglePixelId { get; set; }
        public string DefaultGoogleLabelPixelId { get; set; }
        public string DefaultGoogleAnalyticsTrackingSnippet { get; set; }
        public string DefaultFacebookCustomAudiencePixel { get; set; }


        public int CurrencyId { get; set; }

        //public Teeyoot.Module.Models.CurrecnyRecord CurrecnyRecord { get; set; }

    }
}