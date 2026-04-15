using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class BookingQuotePromotionDto
    {
        //Day la thong tin ket qua danh gia promotion xem co ap dung duoc khong, neu co thi giam bao nhieu tien
        public Guid? PromotionId { get; set; } 
        public string PromotionCode { get; set; } = string.Empty; //Ma promotion, de frontend biet ma nao duoc ap dung
        public string Status { get; set; } = string.Empty; //enum ben PromotionEvaluationResult duoc convert sang string de tra ve api
        public bool IsEligible { get; set; } //Cho biet promotion nay co ap dung duoc khong
        public string Message { get; set; } = string.Empty; //Neu khong ap dung duoc, day la ly do (vi du: "Promotion expired", "Minimum spend not met", etc.)
        public decimal DiscountAmount { get; set; } //So tien duoc giam neu ap dung duoc promotion nay
    }
}
