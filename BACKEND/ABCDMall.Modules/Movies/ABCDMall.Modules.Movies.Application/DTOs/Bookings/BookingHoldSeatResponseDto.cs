using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class BookingHoldSeatResponseDto
    {
        //file này sẽ mô tả thông tin ghế đang được giữ, loại ghế, giá tiền cho frontend hiển thị
        public Guid SeatInventoryId { get; set; }
        public string SeatCode { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string? CoupleGroupCode { get; set; }
    }
}
