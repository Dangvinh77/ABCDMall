using ABCDMall.Modules.Movies.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class PromotionRule
    {
        public Guid Id { get; set; } 
        public Guid PromotionId { get; set; } // Khóa ngoại liên kết đến Promotion
        public PromotionRuleType RuleType { get; set; } = PromotionRuleType.Unknown; // Loại quy tắc (ví dụ: Áp dụng cho loại vé, Áp dụng cho suất chiếu, v.v.)
        public string RuleValue { get; set; } = string.Empty; // Giá trị của quy tắc (ví dụ: "Adult" nếu RuleType là ApplyToTicketType)
        // Day 2: sửa typo để EF config có tên field rõ ràng khi map decimal threshold.
        public decimal? ThresholdValue { get; set; } // Giá trị ngưỡng để áp dụng khuyến mãi (ví dụ: số lượng vé tối thiểu, số tiền tối thiểu, v.v.)
        public int SortOrder { get; set; } // Thứ tự ưu tiên khi áp dụng nhiều quy tắc
        public bool IsRequired { get; set; } = true; // Cho biết quy tắc này có bắt buộc

        // Navigation property
        public Promotion? Promotion { get; set; } // Liên kết đến Promotion
    }
}
