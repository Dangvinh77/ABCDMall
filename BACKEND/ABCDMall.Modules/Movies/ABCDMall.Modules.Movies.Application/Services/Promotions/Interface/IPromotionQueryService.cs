using ABCDMall.Modules.Movies.Application.DTOs.Promotions;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public interface IPromotionQueryService
{
    Task<IReadOnlyList<PromotionResponseDto>> GetPromotionsAsync(string? category, bool activeOnly, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PromotionResponseDto>> GetPromotionsForShowtimeAsync(
        Guid showtimeId,
        DateOnly businessDate,
        DateTime showtimeStartAtUtc,
        bool activeOnly,
        CancellationToken cancellationToken = default);
    Task<PromotionDetailResponseDto?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default);
}
