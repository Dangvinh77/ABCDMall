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
        // Day 3 list API duoc dung de frontend bo mock promotion list.
        // category filter duoc support du dung schema hien tai chua co cot Category rieng.
        var promotions = await _promotionRepository.GetPromotionsAsync(activeOnly, cancellationToken);
        var normalizedCategory = NormalizeCategory(category);

        return promotions
            .Select(MapPromotionListItem)
            .Where(item => normalizedCategory == "all"
                || string.Equals(item.Category, normalizedCategory, StringComparison.OrdinalIgnoreCase))
            .ToList();
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
        return new PromotionDetailResponseDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            Description = promotion.Description,
            Category = ResolveCategory(promotion),
            Status = promotion.Status.ToString(),
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

    private PromotionResponseDto MapPromotionListItem(Promotion promotion)
    {
        return new PromotionResponseDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            Description = promotion.Description,
            Category = ResolveCategory(promotion),
            Status = promotion.Status.ToString(),
            ValidFromUtc = promotion.ValidFromUtc,
            ValidToUtc = promotion.ValidToUtc,
            IsAutoApplied = promotion.IsAutoApplied
        };
    }

    private static string NormalizeCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return "all";
        }

        return category.Trim().ToLowerInvariant();
    }

    private static string ResolveCategory(Promotion promotion)
    {
        // Uu tien metadata seed de category tra ve on dinh.
        if (!string.IsNullOrWhiteSpace(promotion.MetadataJson))
        {
            try
            {
                using var document = JsonDocument.Parse(promotion.MetadataJson);
                if (document.RootElement.TryGetProperty("category", out var categoryElement))
                {
                    var categoryFromMetadata = categoryElement.GetString();
                    if (!string.IsNullOrWhiteSpace(categoryFromMetadata))
                    {
                        return NormalizeCategory(categoryFromMetadata);
                    }
                }
            }
            catch (JsonException)
            {
                // MetadataJson la optional; neu parse fail thi fallback sang infer tu rules/code.
            }
        }

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
}
