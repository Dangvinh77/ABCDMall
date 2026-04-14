using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public sealed class PromotionEvaluationService : IPromotionEvaluationService
{
    private readonly IPromotionRepository _promotionRepository;

    public PromotionEvaluationService(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<EvaluatePromotionResponseDto> EvaluateAsync(
        EvaluatePromotionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Day 3 engine nay la source of truth cho eligibility va discount amount.
        var promotion = await _promotionRepository.GetPromotionByIdAsync(request.PromotionId, cancellationToken);

        if (promotion is null)
        {
            return BuildResult(request.PromotionId, string.Empty, PromotionEvaluationStatus.NotEligible, false, 0m, "Promotion not found.");
        }

        if (promotion.Status != PromotionStatus.Active)
        {
            return BuildResult(promotion.Id, promotion.Code, PromotionEvaluationStatus.Inactive, false, 0m, "Promotion is not active.");
        }

        var now = DateTimeOffset.UtcNow;
        if (promotion.ValidFromUtc.HasValue && now < promotion.ValidFromUtc.Value)
        {
            return BuildResult(promotion.Id, promotion.Code, PromotionEvaluationStatus.Inactive, false, 0m, "Promotion is not active yet.");
        }

        if (promotion.ValidToUtc.HasValue && now > promotion.ValidToUtc.Value)
        {
            return BuildResult(promotion.Id, promotion.Code, PromotionEvaluationStatus.Expired, false, 0m, "Promotion has expired.");
        }

        if (promotion.MaxRedemptions.HasValue)
        {
            var redemptionCount = await _promotionRepository.CountRedemptionsAsync(promotion.Id, cancellationToken);
            if (redemptionCount >= promotion.MaxRedemptions.Value)
            {
                return BuildResult(promotion.Id, promotion.Code, PromotionEvaluationStatus.UsageLimitExceeded, false, 0m, "Promotion redemption limit reached.");
            }
        }

        // MaxRedemptionsPerCustomer chi co y nghia khi request gui du customer identity.
        if (promotion.MaxRedemptionsPerCustomer.HasValue)
        {
            if (!request.GuestCustomerId.HasValue)
            {
                return BuildResult(
                    promotion.Id,
                    promotion.Code,
                    PromotionEvaluationStatus.NotEligible,
                    false,
                    0m,
                    "Customer identity is required for this promotion.");
            }

            var customerRedemptionCount = await _promotionRepository.CountRedemptionsByGuestCustomerAsync(
                promotion.Id,
                request.GuestCustomerId.Value,
                cancellationToken);

            if (customerRedemptionCount >= promotion.MaxRedemptionsPerCustomer.Value)
            {
                return BuildResult(
                    promotion.Id,
                    promotion.Code,
                    PromotionEvaluationStatus.UsageLimitExceeded,
                    false,
                    0m,
                    "Customer redemption limit reached.");
            }
        }

        var appliedRules = new List<string>();
        foreach (var rule in promotion.Rules.OrderBy(x => x.SortOrder))
        {
            var passed = EvaluateRule(rule, request);
            if (!passed)
            {
                if (rule.IsRequired)
                {
                    return BuildResult(
                        promotion.Id,
                        promotion.Code,
                        PromotionEvaluationStatus.NotEligible,
                        false,
                        0m,
                        $"Promotion condition failed: {rule.RuleType}.");
                }

                continue;
            }

            appliedRules.Add(rule.RuleType.ToString());
        }

        var subtotal = request.SeatSubtotal + request.ComboSubtotal;
        if (promotion.MinimumSpendAmount.HasValue && subtotal < promotion.MinimumSpendAmount.Value)
        {
            return BuildResult(
                promotion.Id,
                promotion.Code,
                PromotionEvaluationStatus.NotEligible,
                false,
                0m,
                $"Minimum spend is {promotion.MinimumSpendAmount.Value:N0}.");
        }

        var discountAmount = CalculateDiscountAmount(promotion, subtotal);

        return new EvaluatePromotionResponseDto
        {
            PromotionId = promotion.Id,
            PromotionCode = promotion.Code,
            Status = PromotionEvaluationStatus.Applied,
            IsEligible = discountAmount > 0,
            DiscountAmount = discountAmount,
            Message = discountAmount > 0
                ? "Promotion has been applied successfully."
                : "Promotion is eligible but discount amount is zero.",
            AppliedRules = appliedRules
        };
    }

    private static bool EvaluateRule(PromotionRule rule, EvaluatePromotionRequestDto request)
    {
        switch (rule.RuleType)
        {
            case PromotionRuleType.MinimumSpend:
                return !rule.ThresholdValue.HasValue
                    || (request.SeatSubtotal + request.ComboSubtotal) >= rule.ThresholdValue.Value;

            case PromotionRuleType.SeatCount:
                return !rule.ThresholdValue.HasValue
                    || request.SeatInventoryIds.Count >= rule.ThresholdValue.Value;

            case PromotionRuleType.SeatType:
                return request.SeatTypes.Any(x =>
                    string.Equals(x, rule.RuleValue, StringComparison.OrdinalIgnoreCase));

            case PromotionRuleType.Showtime:
                return Guid.TryParse(rule.RuleValue, out var showtimeId)
                    && showtimeId == request.ShowtimeId;

            case PromotionRuleType.BusinessDate:
                if (string.Equals(rule.RuleValue, "Weekend", StringComparison.OrdinalIgnoreCase))
                {
                    if (!request.BusinessDate.HasValue)
                    {
                        return false;
                    }

                    return request.BusinessDate.Value.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
                }

                return true;

            case PromotionRuleType.PaymentProvider:
                return !string.IsNullOrWhiteSpace(request.PaymentProvider)
                    && string.Equals(request.PaymentProvider, rule.RuleValue, StringComparison.OrdinalIgnoreCase);

            case PromotionRuleType.Combo:
                if (!Guid.TryParse(rule.RuleValue, out var comboId))
                {
                    return false;
                }

                return request.SnackCombos.Any(x => x.ComboId == comboId && x.Quantity > 0);

            case PromotionRuleType.CouponCode:
                return !string.IsNullOrWhiteSpace(request.CouponCode)
                    && string.Equals(request.CouponCode, rule.RuleValue, StringComparison.OrdinalIgnoreCase);

            case PromotionRuleType.BirthdayMonth:
                if (!request.Birthday.HasValue)
                {
                    return false;
                }

                var compareDate = request.BusinessDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
                return request.Birthday.Value.Month == compareDate.Month;

            default:
                return true;
        }
    }

    private static decimal CalculateDiscountAmount(Promotion promotion, decimal subtotal)
    {
        decimal discount = 0m;

        // Ho tro 2 kieu discount co ban cho Day 3:
        // - FlatDiscountValue: giam tien co dinh
        // - PercentageValue: giam theo phan tram
        if (promotion.FlatDiscountValue.HasValue)
        {
            discount = promotion.FlatDiscountValue.Value;
        }
        else if (promotion.PercentageValue.HasValue)
        {
            discount = subtotal * promotion.PercentageValue.Value / 100m;
        }

        if (promotion.MaximumDiscountAmount.HasValue && discount > promotion.MaximumDiscountAmount.Value)
        {
            discount = promotion.MaximumDiscountAmount.Value;
        }

        if (discount > subtotal)
        {
            discount = subtotal;
        }

        return decimal.Round(discount, 2, MidpointRounding.AwayFromZero);
    }

    private static EvaluatePromotionResponseDto BuildResult(
        Guid promotionId,
        string promotionCode,
        PromotionEvaluationStatus status,
        bool isEligible,
        decimal discountAmount,
        string message)
    {
        return new EvaluatePromotionResponseDto
        {
            PromotionId = promotionId,
            PromotionCode = promotionCode,
            Status = status,
            IsEligible = isEligible,
            DiscountAmount = discountAmount,
            Message = message,
            AppliedRules = Array.Empty<string>()
        };
    }
}
