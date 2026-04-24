namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions;

public sealed class PromotionResponseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "all";
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
    public bool IsAutoApplied { get; set; }
    public string? ImageUrl { get; set; }
    public string? BadgeText { get; set; }
    public string? AccentFrom { get; set; }
    public string? AccentTo { get; set; }
    public string? DisplayCondition { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayPriority { get; set; }
    public decimal? MinimumSpendAmount { get; set; }
    public IReadOnlyCollection<PromotionRuleDto> Rules { get; set; } = Array.Empty<PromotionRuleDto>();
}
