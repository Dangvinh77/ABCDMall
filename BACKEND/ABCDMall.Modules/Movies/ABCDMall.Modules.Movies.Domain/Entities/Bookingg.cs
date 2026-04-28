using ABCDMall.Modules.Movies.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class Booking
    {
        // Thông tin cơ bản về booking
        public Guid Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public Guid ShowtimeId { get; set; }
        public Guid? GuestCustomerId { get; set; }
        public Guid? BookingHoldId { get; set; }
        public Guid? PromotionId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

        //Thong tin khach hang khi dat ve
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhoneNumber { get; set; } = string.Empty;
        public decimal SeatSubtotal { get; set; }
        public decimal ComboSubtotal { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string Currency { get; set; } = "VND";
        public string? PromotionSnapshotJson { get; set; } // Lưu trữ thông tin khuyến mãi tại thời điểm đặt vé dưới dạng JSON

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }


        // Navigation properties
        public GuestCustomer? GuestCustomer { get; set; }
        public ICollection<BookingItem> Items { get; set; } = new List<BookingItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    }
}
