using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class BookingHoldSeat
    {
        public Guid Id { get; set; }
        public Guid BookingHoldId { get; set; } // ID của BookingHold liên quan
        public Guid SeatInventoryId { get; set; } // ID của vé ngồi trong kho vé
        public string SeatCode { get; set; } = string.Empty; // Mã ghế (ví dụ: A1, B2)
        public string SeatType { get; set; } = string.Empty; // Loại ghế (ví dụ: Thường, VIP)
        public decimal UnitPrice { get; set; } // Giá vé cho ghế này
        public string? CoupleGroupCode { get; set; } // Mã nhóm ghế đôi (nếu là ghế đôi)


        // Navigation properties
        public BookingHold? BookingHold { get; set; } // Tham chiếu đến BookingHold
    }
}
