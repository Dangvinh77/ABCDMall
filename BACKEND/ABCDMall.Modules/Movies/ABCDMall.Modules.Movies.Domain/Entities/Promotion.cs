using ABCDMall.Modules.Movies.Domain.Enums;


namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class Promotion
    {
        public Guid Id { get; set; }
        // Day 2: thêm code vì execution plan yêu cầu unique index Promotion.Code.
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PromotionStatus Status { get; set; } = PromotionStatus.Draft;

        public DateTimeOffset? ValidFromUtc { get; set; }// Thời điểm bắt đầu hiệu lực của khuyến mãi (UTC)
        public DateTimeOffset? ValidToUtc { get; set; }// Thời điểm kết thúc hiệu lực của khuyến mãi (UTC)
        public decimal? PercentageValue { get; set; } // Giá trị phần trăm giảm giá (ví dụ: 10 cho 10%)
        public decimal? FlatDiscountValue { get; set; } // Giá trị giảm giá cố định (ví dụ: 50000 cho giảm 50,000 VND)
        public decimal? MaximumDiscountAmount { get; set; } // Số tiền giảm giá tối đa (áp dụng khi sử dụng PercentageValue)
        public decimal? MinimumSpendAmount { get; set; } // Số tiền tối thiểu để áp dụng khuyến mãi
        // Day 2: đổi sang dạng số nhiều để khớp ý nghĩa rule và config/index sau này.
        public int? MaxRedemptions { get; set; } // Số lần tối đa mà khuyến mãi có thể được sử dụng
        public int? MaxRedemptionsPerCustomer { get; set; } // Số lần tối đa mà mỗi khách hàng có thể sử dụng khuyến mãi
        public bool IsAutoApplied { get; set; } // Cho biết khuyến mãi có được tự động áp dụng khi khách hàng đủ điều kiện hay không
        public string? MetadataJson { get; set; } // Lưu trữ thông tin bổ sung về khuyến mãi dưới dạng JSON (ví dụ: điều kiện áp dụng, loại khuyến mãi, v.v.)
        // Day 2: sửa typo để đồng bộ convention audit fields giữa các entity.
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }


        // Navigation properties
        public ICollection<PromotionRule> Rules { get; set; } = new List<PromotionRule>();
        public ICollection<PromotionRedemption> Redemptions { get; set; } = new List<PromotionRedemption>();
    }
}
