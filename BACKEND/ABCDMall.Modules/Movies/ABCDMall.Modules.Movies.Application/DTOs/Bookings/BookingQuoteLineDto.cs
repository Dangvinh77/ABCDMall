using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class BookingQuoteLineDto
    {
        //class nay là một dòng chi tiết trong bảng tạm tính tiền
        /*
         Seat A1 — 90,000
         Seat A2 — 90,000
         Service fee — 20,000
         Combo Popcorn x2 — 90,000
         Promotion discount — -30,000

         Mỗi dòng như vậy là một BookingQuoteLineDto.
         */
        public string Type { get; set; } = string.Empty; //Loại dòng: seat, service_fee, combo, promotion_discount
        public string Code { get; set; } = string.Empty; //Mã cụ thể của dòng, ví dụ: mã ghế (A1), mã combo, mã promotion, hoặc null đối với service_fee
        public string Label { get; set; } = string.Empty; //Nhãn hiển thị cho dòng, ví dụ: "Seat A1", "Service fee", "Combo Popcorn x2", "Promotion discount"
        public decimal Amount { get; set; } //Số tiền của dòng, có thể là dương (đối với seat, service_fee, combo) hoặc âm (đối với promotion_discount)
    }
}
