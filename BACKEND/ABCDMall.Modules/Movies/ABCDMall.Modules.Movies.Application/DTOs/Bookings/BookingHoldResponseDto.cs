using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class BookingHoldResponseDto
    {
        //file này sẽ mô tả response sau khi tạo hold thành công
        //trả đủ dữu liệu để frontend hiển thị hiển thị countdown,  hiển thị hóa đơn tạm tính,  hiển thị danh sách ghế đang giữ
        public Guid HoldId { get; set; }
        public string HoldCode { get; set; } = string.Empty;//mã giữ chỗ có thể dùng để hiển thị cho khách hàng hoặc dùng để tra cứu khi cần thiết
        public Guid ShowtimeId { get; set; } //id của suất chiếu liên quan đến hold này
        public string Status { get; set; } = string.Empty;//trạng thái hiện tại của hold
        public DateTime ExpiresAtUtc { get; set; } //thời điểm hold hết hạn, frontend có thể dùng để hiển thị countdown cho khách hàng biết còn bao nhiêu thời gian để hoàn tất đặt chỗ trước khi hold bị hủy
        public int RemainingSeconds { get; set; }

        public decimal SeatSubtotal { get; set; } //tổng tiền của các ghế đang giữ
        public decimal ComboSubtotal { get; set; } //tổng tiền của các combo đang giữ
        public decimal DiscountAmount { get; set; } //số tiền giảm giá áp dụng
        public decimal GrandTotal { get; set; } //tổng tiền cuối cùng sau khi áp dụng giảm giá

        public IReadOnlyCollection<BookingHoldSeatResponseDto> Seats { get; set; } = Array.Empty<BookingHoldSeatResponseDto>();//danh sách các ghế đang giữ


    }
}
