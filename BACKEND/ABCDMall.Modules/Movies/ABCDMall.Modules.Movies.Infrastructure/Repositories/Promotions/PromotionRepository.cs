using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Promotions;

public sealed class PromotionRepository : IPromotionRepository
{
    private readonly MoviesBookingDbContext _dbContext;

    public PromotionRepository(MoviesBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Promotion>> GetPromotionsAsync(
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Promotions
            .AsNoTracking()
            .Include(x => x.Rules)
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(x => x.Status == PromotionStatus.Active);
        }

        return await query
            .OrderByDescending(x => x.IsAutoApplied)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Promotion?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Promotions
            .AsNoTracking()
            .Include(x => x.Rules.OrderBy(r => r.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == promotionId, cancellationToken);
    }

    public async Task<IReadOnlyList<SnackCombo>> GetSnackCombosAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SnackCombos
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<SnackCombo?> GetSnackComboByIdAsync(Guid comboId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SnackCombos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == comboId && x.IsActive, cancellationToken);
    }

    public async Task<int> CountRedemptionsAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromotionRedemptions
            .AsNoTracking()
            .CountAsync(x => x.PromotionId == promotionId, cancellationToken);
    }

    public async Task<int> CountRedemptionsByGuestCustomerAsync(
        Guid promotionId,
        Guid guestCustomerId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PromotionRedemptions
            .AsNoTracking()
            .CountAsync(
                x => x.PromotionId == promotionId && x.GuestCustomerId == guestCustomerId,
                cancellationToken);
    }
}
