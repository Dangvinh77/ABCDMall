
namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; } // ID của Booking liên quan
        public Guid? BookingItemId { get; set; } // ID của BookingItem liên quan (nếu có)
        public Guid? SeatInventoryId    { get; set; } // ID của vé ngồi trong kho vé (nếu là vé ngồi)
        public string TicketCode { get; set; } = string.Empty; // Mã vé duy nhất (ví dụ: TICKET-123456)
        public string? SeatCode { get; set; } // Mã ghế (ví dụ: A1, B2) - chỉ áp dụng nếu vé này là vé ngồi
        public string? QrCodeContent { get; set; } // Nội dung mã QR (có thể là URL hoặc dữ liệu mã hóa khác)
        public string DeliveryStatus { get; set; } = "Pending"; // Trạng thái giao vé (ví dụ: Pending, Delivered)
        public DateTime IssuedAtUtc { get; set; } // Thời điểm phát hành vé

        // Navigation properties
        public Bookingg? Booking { get; set; } // Tham chiếu đến Booking
    }
}
