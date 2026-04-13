using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions
{
    public sealed class EvaluatePromotionResposeDto
    {
        public PromotionEvaluationStatus Status { get; set; } = PromotionEvaluationStatus.None;
        public Guid? PromotionId { get; set; }
        public string PromotionCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
