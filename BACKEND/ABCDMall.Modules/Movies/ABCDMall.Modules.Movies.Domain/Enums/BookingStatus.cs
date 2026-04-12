using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Enums
{
    public enum BookingStatus
    {
        PendingPayment = 1,
        Confirmed = 2,
        Cancelled = 3,
        Expired = 4,
        Refunded = 5
    }
}
