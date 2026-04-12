

namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions
{
    public sealed class PromotionRuleDto
    {
        //sealed class chi co the duoc ke thua tu chinh no, khong the duoc ke thua tu lop khac
        public Guid Id { get; set; }
        public string RuleType { get; set; } = string.Empty; //RuleType the hien loai dieu kien, co the la MinimumSpend, SeatCount, SeatType, Showtime, BusinessDate, PaymentProvider, Combo, CouponCode
        public string RuleValue { get; set; } = string.Empty;//RuleValue the hien gia tri cua dieu kien, nếu RuleType = "MovieCategory" thì RuleValue = "Horror"
        public decimal? ThresholdValue { get; set; } //ThresholdValue the hien gia tri nguong, neu RuleType = "MinimumSpend" thi ThresholdValue = 100.00
        public int SortOrder { get; set; } //SortOrder the hien thu tu ap dung cua cac dieu kien, dieu kien co SortOrder nho hon se duoc ap dung truoc
        public bool IsRequired { get; set; } //IsRequired cho biet dieu kien co bat buoc hay khong, neu IsRequired = true thi dieu kien phai duoc ap dung
    }
}