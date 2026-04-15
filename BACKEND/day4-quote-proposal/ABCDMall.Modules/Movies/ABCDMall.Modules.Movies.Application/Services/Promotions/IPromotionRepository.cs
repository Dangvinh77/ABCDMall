using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public interface IPromotionRepository
{
    Task<IReadOnlyList<Promotion>> GetPromotionsAsync(bool activeOnly, CancellationToken cancellationToken = default);
    Task<Promotion?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SnackCombo>> GetSnackCombosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SnackCombo>> GetSnackCombosByIdsAsync(
        IReadOnlyCollection<Guid> comboIds,
        CancellationToken cancellationToken = default);
    Task<SnackCombo?> GetSnackComboByIdAsync(Guid comboId, CancellationToken cancellationToken = default);
    Task<int> CountRedemptionsAsync(Guid promotionId, CancellationToken cancellationToken = default);
    Task<int> CountRedemptionsByGuestCustomerAsync(Guid promotionId, Guid guestCustomerId, CancellationToken cancellationToken = default);
}
