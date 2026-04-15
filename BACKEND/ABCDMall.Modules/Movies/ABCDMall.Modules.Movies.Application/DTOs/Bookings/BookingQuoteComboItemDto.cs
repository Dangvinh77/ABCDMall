using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class BookingQuoteComboItemDto
    {
        //Day la class dai dien cho 1 combo ma khach chon
        public Guid ComboId { get; set; }
        public int Quantity { get; set; }
    }
}
