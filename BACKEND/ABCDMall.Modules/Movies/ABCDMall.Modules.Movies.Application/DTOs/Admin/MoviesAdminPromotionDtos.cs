namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public class MoviesAdminPromotionListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "all";
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
    public decimal? PercentageValue { get; set; }
    public decimal? FlatDiscountValue { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public decimal? MinimumSpendAmount { get; set; }
    public int? MaxRedemptions { get; set; }
    public int? MaxRedemptionsPerCustomer { get; set; }
    public bool IsAutoApplied { get; set; }
    public string? ImageUrl { get; set; }
    public string? BadgeText { get; set; }
    public string? AccentFrom { get; set; }
    public string? AccentTo { get; set; }
    public string? DisplayCondition { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayPriority { get; set; }
    public int RuleCount { get; set; }
    public int RedemptionCount { get; set; }
}

public sealed class MoviesAdminPromotionDetailDto : MoviesAdminPromotionListItemDto
{
    public string? MetadataJson { get; set; }
    public IReadOnlyList<MoviesAdminPromotionRuleDto> Rules { get; set; } = [];
}

public sealed class MoviesAdminPromotionUpsertDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "all";
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? ValidFromUtc { get; set; }
    public DateTimeOffset? ValidToUtc { get; set; }
    public decimal? PercentageValue { get; set; }
    public decimal? FlatDiscountValue { get; set; }
    public decimal? MaximumDiscountAmount { get; set; }
    public decimal? MinimumSpendAmount { get; set; }
    public int? MaxRedemptions { get; set; }
    public int? MaxRedemptionsPerCustomer { get; set; }
    public bool IsAutoApplied { get; set; }
    public string? ImageUrl { get; set; }
    public string? BadgeText { get; set; }
    public string? AccentFrom { get; set; }
    public string? AccentTo { get; set; }
    public string? DisplayCondition { get; set; }
    public bool IsFeatured { get; set; }
    public int DisplayPriority { get; set; }
    public string? MetadataJson { get; set; }
    public IReadOnlyList<MoviesAdminPromotionRuleDto> Rules { get; set; } = [];
}

public sealed class MoviesAdminPromotionRuleDto
{
    public string RuleType { get; set; } = string.Empty;
    public string RuleValue { get; set; } = string.Empty;
    public decimal? ThresholdValue { get; set; }
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
}
