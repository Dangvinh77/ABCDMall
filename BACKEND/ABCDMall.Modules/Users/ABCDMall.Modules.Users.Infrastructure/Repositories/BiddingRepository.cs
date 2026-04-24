using ABCDMall.Modules.Users.Application.Services.Bidding;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Repositories;

public sealed class BiddingRepository : IBiddingRepository
{
    private readonly MallDbContext _context;

    public BiddingRepository(MallDbContext context)
    {
        _context = context;
    }

    public Task AddBidAsync(CarouselBid bid, CancellationToken cancellationToken = default)
        => _context.CarouselBids.AddAsync(bid, cancellationToken).AsTask();

    public Task AddMovieAdAsync(MovieCarouselAd movieAd, CancellationToken cancellationToken = default)
        => _context.MovieCarouselAds.AddAsync(movieAd, cancellationToken).AsTask();

    public Task<CarouselBid?> GetBidByIdAsync(string bidId, CancellationToken cancellationToken = default)
        => _context.CarouselBids.FirstOrDefaultAsync(x => x.Id == bidId, cancellationToken);

    public async Task<IReadOnlyList<CarouselBid>> GetBidsByShopIdAsync(string shopId, CancellationToken cancellationToken = default)
        => await _context.CarouselBids
            .Where(x => x.ShopId == shopId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CarouselBid>> GetBidsByTargetMondayAsync(DateTime targetMondayDate, CancellationToken cancellationToken = default)
        => await _context.CarouselBids
            .Where(x => x.TargetMondayDate == targetMondayDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CarouselBid>> GetBidsByTargetMondayAndStatusesAsync(
        DateTime targetMondayDate,
        IReadOnlyCollection<CarouselBidStatus> statuses,
        CancellationToken cancellationToken = default)
    {
        if (statuses.Count == 0)
        {
            return [];
        }

        return await _context.CarouselBids
            .Where(x => x.TargetMondayDate == targetMondayDate && statuses.Contains(x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CarouselBid>> GetActiveBidsAsync(CancellationToken cancellationToken = default)
        => await _context.CarouselBids
            .Where(x => x.Status == CarouselBidStatus.Active)
            .ToListAsync(cancellationToken);

    public Task<MovieCarouselAd?> GetMovieAdByTargetMondayDateAsync(DateTime targetMondayDate, CancellationToken cancellationToken = default)
        => _context.MovieCarouselAds.FirstOrDefaultAsync(x => x.TargetMondayDate == targetMondayDate, cancellationToken);

    public Task<MovieCarouselAd?> GetActiveMovieAdAsync(CancellationToken cancellationToken = default)
        => _context.MovieCarouselAds.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

    public async Task<IReadOnlyList<MovieCarouselAd>> GetMovieAdsAsync(CancellationToken cancellationToken = default)
        => await _context.MovieCarouselAds.ToListAsync(cancellationToken);

    public Task<bool> ShopExistsAsync(string shopId, CancellationToken cancellationToken = default)
        => _context.ShopInfos.AnyAsync(x => x.Id == shopId, cancellationToken);

    public async Task<IReadOnlyDictionary<string, ShopCatalogInfo>> GetShopCatalogInfoByIdsAsync(
        IEnumerable<string> shopIds,
        CancellationToken cancellationToken = default)
    {
        var ids = shopIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ids.Length == 0)
        {
            return new Dictionary<string, ShopCatalogInfo>(StringComparer.OrdinalIgnoreCase);
        }

        var shops = await _context.ShopInfos
            .Where(x => ids.Contains(x.Id!))
            .Select(x => new ShopCatalogInfo(x.Id!, x.ShopName, x.Slug))
            .ToListAsync(cancellationToken);

        return shops.ToDictionary(x => x.ShopId, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<ManagerContactInfo?> GetManagerContactByShopIdAsync(string shopId, CancellationToken cancellationToken = default)
    {
        var manager = await _context.Users
            .Where(x => x.Role == "Manager" && x.ShopId == shopId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new { x.ShopId, x.Email, x.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        if (manager?.ShopId is null)
        {
            return null;
        }

        var shopName = await _context.ShopInfos
            .Where(x => x.Id == manager.ShopId)
            .Select(x => x.ShopName)
            .FirstOrDefaultAsync(cancellationToken);

        return new ManagerContactInfo(manager.ShopId, shopName ?? manager.ShopId, manager.Email, manager.FullName);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
