using ABCDMall.Modules.Movies.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
     public class Payment
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; } // ID của Booking liên quan
        public PaymentProvider Provider { get; set; } = PaymentProvider.Unknown;// Nhà cung cấp thanh toán (ví dụ: PayPal, Stripe, VNPay)
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;// Trạng thái thanh toán
        public decimal Amount { get; set; } // Số tiền thanh toán
        public string Currency { get; set; } = "VND"; // Đơn vị tiền tệ
        public string? PaymentIntentId { get; set; } // ID của Payment Intent từ nhà cung cấp thanh toán (nếu có)
        public string? ProviderTransactionId { get; set; } // ID giao dịch từ nhà cung cấp thanh toán (nếu có)
        public string? CallbackPayloadJson { get; set; } // Lưu trữ dữ liệu phản hồi từ nhà cung cấp thanh toán dưới dạng JSON (nếu có)
        public string? FailureReason { get; set; } // Lý do thất bại (nếu có)
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; } // Thời điểm hoàn thành thanh toán (nếu có)

        // Navigation properties
        public Booking? Booking { get; set; } // Tham chiếu đến Booking
    }
}
