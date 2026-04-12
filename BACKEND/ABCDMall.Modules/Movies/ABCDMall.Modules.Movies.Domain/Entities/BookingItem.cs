

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class BookingItem
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; } // ID của Booking liên quan
        public string ItemType { get; set; } = string.Empty; // Loại mục (ví dụ: Seat, Combo)
        public string ItemCode { get; set; } = string.Empty; // Mã mục (ví dụ: A1, COMBO1)
        public string Description { get; set; } = string.Empty; // Mô tả mục (ví dụ: Ghế A1, Combo Popcorn)
        public Guid? SeatInventoryId { get; set; } // ID của vé ngồi trong kho vé (nếu là mục loại Seat)
        public int Quantity { get; set; } // Số lượng (thường là 1 cho vé ngồi, có thể >1 cho combo)
        public decimal UnitPrice { get; set; } // Giá đơn vị cho mục này
        public decimal LineTotal { get; set; } // Tổng tiền cho mục này (UnitPrice * Quantity)


        // Navigation properties
        public Bookingg? Booking { get; set; } // Tham chiếu đến Booking
    }
}
