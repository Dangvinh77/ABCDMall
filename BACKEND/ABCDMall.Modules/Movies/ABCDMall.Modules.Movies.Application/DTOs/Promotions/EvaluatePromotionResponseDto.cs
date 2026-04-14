using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions;

public sealed class EvaluatePromotionResponseDto
{
    public Guid PromotionId { get; set; }
    public string PromotionCode { get; set; } = string.Empty;
    public PromotionEvaluationStatus Status { get; set; } = PromotionEvaluationStatus.None;
    public bool IsEligible { get; set; }
    public decimal DiscountAmount { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyCollection<string> AppliedRules { get; set; } = Array.Empty<string>();
}
