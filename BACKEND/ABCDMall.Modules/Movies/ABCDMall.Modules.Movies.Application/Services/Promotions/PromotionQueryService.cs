using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Domain.Entities;
using System.Text.Json;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public sealed class PromotionQueryService : IPromotionQueryService
{
    private readonly IPromotionRepository _promotionRepository;

    public PromotionQueryService(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<IReadOnlyList<PromotionResponseDto>> GetPromotionsAsync(
        string? category,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var promotions = await _promotionRepository.GetPromotionsAsync(activeOnly, cancellationToken);
        return BuildPromotionList(promotions, category);
    }

    public async Task<IReadOnlyList<PromotionResponseDto>> GetPromotionsForShowtimeAsync(
        Guid showtimeId,
        DateOnly businessDate,
        DateTime showtimeStartAtUtc,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var promotions = await _promotionRepository.GetPromotionsAsync(activeOnly, cancellationToken);
        return BuildPromotionList(promotions, null, showtimeId, businessDate, showtimeStartAtUtc);
    }

    public async Task<PromotionDetailResponseDto?> GetPromotionByIdAsync(
        Guid promotionId,
        CancellationToken cancellationToken = default)
    {
        var promotion = await _promotionRepository.GetPromotionByIdAsync(promotionId, cancellationToken);
        if (promotion is null)
        {
            return null;
        }

        // Detail API tra ve them rules va category de frontend render dung ngữ cảnh khuyen mai.
        var metadata = ReadDisplayMetadata(promotion);

        return new PromotionDetailResponseDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            Description = promotion.Description,
            Category = metadata.Category,
            Status = promotion.Status.ToString(),
            ImageUrl = metadata.ImageUrl,
            BadgeText = metadata.BadgeText,
            AccentFrom = metadata.AccentFrom,
            AccentTo = metadata.AccentTo,
            DisplayCondition = metadata.DisplayCondition,
            IsFeatured = metadata.IsFeatured,
            DisplayPriority = metadata.DisplayPriority,
            PercentageValue = promotion.PercentageValue,
            FlatDiscountValue = promotion.FlatDiscountValue,
            MaximumDiscountAmount = promotion.MaximumDiscountAmount,
            MinimumSpendAmount = promotion.MinimumSpendAmount,
            MaxRedemptions = promotion.MaxRedemptions,
            MaxRedemptionsPerCustomer = promotion.MaxRedemptionsPerCustomer,
            IsAutoApplied = promotion.IsAutoApplied,
            Rules = promotion.Rules
                .OrderBy(rule => rule.SortOrder)
                .Select(rule => new PromotionRuleDto
                {
                    Id = rule.Id,
                    RuleType = rule.RuleType.ToString(),
                    RuleValue = rule.RuleValue,
                    ThresholdValue = rule.ThresholdValue,
                    SortOrder = rule.SortOrder,
                    IsRequired = rule.IsRequired
                })
                .ToArray()
        };
    }

    private PromotionResponseDto MapPromotionListItem(Promotion promotion, PromotionDisplayMetadata metadata)
    {
        return new PromotionResponseDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            Description = promotion.Description,
            Category = metadata.Category,
            Status = promotion.Status.ToString(),
            ValidFromUtc = promotion.ValidFromUtc,
            ValidToUtc = promotion.ValidToUtc,
            IsAutoApplied = promotion.IsAutoApplied,
            ImageUrl = metadata.ImageUrl,
            BadgeText = metadata.BadgeText,
            AccentFrom = metadata.AccentFrom,
            AccentTo = metadata.AccentTo,
            DisplayCondition = metadata.DisplayCondition,
            IsFeatured = metadata.IsFeatured,
            DisplayPriority = metadata.DisplayPriority,
            MinimumSpendAmount = promotion.MinimumSpendAmount,
            Rules = MapRules(promotion)
        };
    }

    private IReadOnlyList<PromotionResponseDto> BuildPromotionList(
        IReadOnlyList<Promotion> promotions,
        string? category,
        Guid? showtimeId = null,
        DateOnly? businessDate = null,
        DateTime? showtimeStartAtUtc = null)
    {
        var normalizedCategory = NormalizeCategory(category);

        return promotions
            .Select(promotion => new
            {
                Promotion = promotion,
                Metadata = ReadDisplayMetadata(promotion)
            })
            .Where(x => IsPromotionVisibleForShowtimeContext(
                x.Promotion,
                showtimeId,
                businessDate,
                showtimeStartAtUtc))
            .OrderByDescending(x => x.Metadata.IsFeatured)
            .ThenBy(x => x.Metadata.DisplayPriority)
            .ThenByDescending(x => x.Promotion.UpdatedAtUtc)
            .Select(x => MapPromotionListItem(x.Promotion, x.Metadata))
            .Where(item => normalizedCategory == "all"
                || string.Equals(item.Category, normalizedCategory, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static bool IsPromotionVisibleForShowtimeContext(
        Promotion promotion,
        Guid? showtimeId,
        DateOnly? businessDate,
        DateTime? showtimeStartAtUtc)
    {
        if (!showtimeId.HasValue || !businessDate.HasValue || !showtimeStartAtUtc.HasValue)
        {
            return true;
        }

        foreach (var rule in promotion.Rules.Where(rule =>
                     rule.RuleType is Domain.Enums.PromotionRuleType.Showtime or Domain.Enums.PromotionRuleType.BusinessDate))
        {
            var isMatch = PromotionShowtimeRuleMatcher.MatchesShowtimeContext(
                rule,
                showtimeId.Value,
                businessDate.Value,
                showtimeStartAtUtc.Value);

            if (!isMatch && rule.IsRequired)
            {
                return false;
            }
        }

        return true;
    }

    private static IReadOnlyCollection<PromotionRuleDto> MapRules(Promotion promotion)
    {
        return promotion.Rules
            .OrderBy(rule => rule.SortOrder)
            .Select(rule => new PromotionRuleDto
            {
                Id = rule.Id,
                RuleType = rule.RuleType.ToString(),
                RuleValue = rule.RuleValue,
                ThresholdValue = rule.ThresholdValue,
                SortOrder = rule.SortOrder,
                IsRequired = rule.IsRequired
            })
            .ToArray();
    }

    private static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "all";
        }

        return category.Trim().ToLowerInvariant();
    }

    private static PromotionDisplayMetadata ReadDisplayMetadata(Promotion promotion)
    {
        var metadata = new PromotionDisplayMetadata
        {
            Category = ResolveCategoryFallback(promotion),
            BadgeText = promotion.Code,
            DisplayCondition = promotion.IsAutoApplied
                ? "Applied automatically when eligible"
                : "Select this offer before checkout"
        };

        if (!string.IsNullOrWhiteSpace(promotion.MetadataJson))
        {
            try
            {
                using var document = JsonDocument.Parse(promotion.MetadataJson);
                metadata.Category = ReadString(document.RootElement, "category") is { Length: > 0 } categoryFromMetadata
                    ? NormalizeCategory(categoryFromMetadata)
                    : metadata.Category;
                metadata.ImageUrl = ReadString(document.RootElement, "imageUrl");
                metadata.BadgeText = ReadString(document.RootElement, "badgeText") ?? metadata.BadgeText;
                metadata.AccentFrom = ReadString(document.RootElement, "accentFrom");
                metadata.AccentTo = ReadString(document.RootElement, "accentTo");
                metadata.DisplayCondition = ReadString(document.RootElement, "displayCondition") ?? metadata.DisplayCondition;
                metadata.IsFeatured = ReadBool(document.RootElement, "isFeatured");
                metadata.DisplayPriority = ReadInt(document.RootElement, "displayPriority");
            }
            catch (JsonException)
            {
                return metadata;
            }
        }

        return metadata;
    }

    private static string ResolveCategoryFallback(Promotion promotion)
    {
        if (promotion.Rules.Any(rule => rule.RuleType == Domain.Enums.PromotionRuleType.Combo))
        {
            return "combo";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == Domain.Enums.PromotionRuleType.PaymentProvider))
        {
            return "bank";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == Domain.Enums.PromotionRuleType.BusinessDate))
        {
            return "weekend";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == Domain.Enums.PromotionRuleType.BirthdayMonth))
        {
            return "member";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == Domain.Enums.PromotionRuleType.SeatType))
        {
            return "ticket";
        }

        return "all";
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static bool ReadBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => false
        };
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : 0;
    }

    private sealed class PromotionDisplayMetadata
    {
        public string Category { get; set; } = "all";
        public string? ImageUrl { get; set; }
        public string? BadgeText { get; set; }
        public string? AccentFrom { get; set; }
        public string? AccentTo { get; set; }
        public string? DisplayCondition { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayPriority { get; set; }
    }
}
