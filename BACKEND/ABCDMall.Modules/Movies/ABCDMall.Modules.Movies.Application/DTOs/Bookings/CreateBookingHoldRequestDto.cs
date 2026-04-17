using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class CreateBookingHoldRequestDto
    {
        //file này cho biết use case này cần nhận những input gì để giữ ghế
        public Guid ShowtimeId { get; set; } //id của suất chiếu
        public IReadOnlyCollection<Guid> SeatInventoryIds { get; set; } = Array.Empty<Guid>(); //danh sách id của các ghế cần giữ
        public IReadOnlyCollection<BookingQuoteComboItemDto> SnackCombos { get; set; } = Array.Empty<BookingQuoteComboItemDto>(); //danh sách các combo đồ ăn kèm
        public Guid? PromotionId { get; set; } //id của khuyến mãi nếu có
        public string? PaymentProvider { get; set; } //nhà cung cấp thanh toán
        public DateOnly? Birthday { get; set; } //ngày sinh của khách hàng
        public Guid? GuestCustomerId { get; set; } //id của khách hàng nếu là khách vãng lai
        public string? SessionId { get; set; } //id của phiên làm việc
    }
}
