namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions;

public sealed class PromotionRuleDto
{
    public Guid Id { get; set; }
    public string RuleType { get; set; } = string.Empty;
    public string RuleValue { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; }
}
