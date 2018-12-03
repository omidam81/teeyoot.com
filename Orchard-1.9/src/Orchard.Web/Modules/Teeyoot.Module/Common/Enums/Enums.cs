using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teeyoot.Module.Common.Enums
{
    public enum CampaignStatus
    {
        Unpaid = 1,
        //PartiallyPaid,
        Paid
    }

    public enum OrderStatus
    {
        Pending = 1,
        Approved = 2,
        Printing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7
    }

    public enum OverviewType
    {
        Today = 1,
        Yesterday = 2,
        Active = 4,
        AllTime = 8
    }

    public enum CampaignSortOrder
    {
        StartDate = 0,
        EndDate,
        Sales,
        //Reservations,
        Name
    }
}