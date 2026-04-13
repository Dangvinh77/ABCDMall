using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions.Interface
{
    public interface IPromotionRepository
    {
        Task<IReadOnlyCollection<Promotion>> GetActiveAsync(CancellationToken cancellationToken = default);
        Task<Promotion?> GetByIdAsync(Guid promotionId, CancellationToken cancellationToken = default);
        Task<Promotion?> GetByCodeAsync(string promotionCode, CancellationToken cancellationToken = default);
    }
}
