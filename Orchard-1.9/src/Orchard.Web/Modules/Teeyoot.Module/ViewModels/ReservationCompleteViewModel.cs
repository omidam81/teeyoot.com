using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Teeyoot.Module.Models;

namespace Teeyoot.Module.ViewModels
{
    public class ReservationCompleteViewModel
    {
        public ReservationCompleteViewModel()
        {

        }

        public dynamic[] Campaigns { get; set; }

        public string Message { get; set; }

        public bool Oops { get; set; }

        public OrderRecord Order { get; set; }

        public string SellerFbPixel { get; set; }

        public string FacebookCustomAudiencePixel { get; set; }
    }
}