using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class PromotionRedemption
    {
        //class này dùng để lưu trữ thông tin về việc một khách hàng đã sử dụng một khuyến mãi cụ thể nào đó
        //, bao gồm số lần đã sử dụng và thời điểm sử dụng cuối cùng.
        public Guid Id { get; set; }
        public Guid PromotionId { get; set; } // Khóa ngoại liên kết đến Promotion
        public Guid? BookingId { get; set; } // Khóa ngoại liên kết đến Booking 
        public Guid? GuestCustomerId { get; set; } // Khóa ngoại liên kết đến GuestCustomer 
        public string? CouponCode { get; set; } // Mã coupon đã sử dụng (nếu có)
        // Day 2: lưu số tiền giảm thực tế để audit và support báo cáo/redemption history.
        public decimal DiscountAmount { get; set; }
        public DateTime RedeemedAtUtc { get; set; } // Thời điểm sử dụng khuyến mãi (UTC)


        // Navigation properties
        public Promotion? Promotion { get; set; } // Liên kết đến Promotion
    }
}
