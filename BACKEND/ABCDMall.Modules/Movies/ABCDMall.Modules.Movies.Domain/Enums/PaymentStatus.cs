using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4,
        Cancelled = 5,
        Refunded = 6
    }
}
