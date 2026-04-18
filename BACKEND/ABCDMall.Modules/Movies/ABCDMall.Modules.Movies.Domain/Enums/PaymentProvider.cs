using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Enums
{
    public enum PaymentProvider
    {
        Unknown = 0,
        Mock = 1,
        Momo = 2,
        VnPay = 3,
        Stripe = 4,
        PayPal = 5
    }
}
