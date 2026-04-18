using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class BookingHold
    {
        public Guid Id { get; set; }

        // Day 2: cần mã hold riêng để tạo unique index theo tài liệu.
        public string HoldCode { get; set; } = string.Empty;

        // Day 2: showtimeId là external key từ Dev 1, Dev 2 chỉ lưu để bám flow booking.
        public Guid ShowtimeId { get; set; }

        // Day 2: thêm status + expiry để support hold lifecycle và cleanup background service.
        public BookingHoldStatus Status { get; set; } = BookingHoldStatus.Active;
        public DateTime ExpiresAtUtc { get; set; }
        public string? SessionId { get; set; }

        // Day 2: giữ snapshot số tiền ở hold để dễ quote/create booking về sau.
        public decimal SeatSubtotal { get; set; }
        public decimal ComboSubtotal { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public Guid? PromotionId { get; set; }
        public string? PromotionSnapshotJson { get; set; }
        public string? ComboSnapshotJson { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        // Day 2: quan hệ 1-n với các ghế đang được hold.
        public ICollection<BookingHoldSeat> Seats { get; set; } = new List<BookingHoldSeat>();
    }
}
