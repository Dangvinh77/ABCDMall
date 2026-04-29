using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public interface IBiddingRepository
{
    Task AddBidAsync(CarouselBid bid, CancellationToken cancellationToken = default);

    Task AddMovieAdAsync(MovieCarouselAd movieAd, CancellationToken cancellationToken = default);

    Task<CarouselBid?> GetBidByIdAsync(string bidId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarouselBid>> GetBidsByShopIdAsync(string shopId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarouselBid>> GetBidsByTargetMondayAsync(DateTime targetMondayDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarouselBid>> GetBidsByTargetMondayAndStatusesAsync(
        DateTime targetMondayDate,
        IReadOnlyCollection<CarouselBidStatus> statuses,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CarouselBid>> GetActiveBidsAsync(CancellationToken cancellationToken = default);

    Task<MovieCarouselAd?> GetMovieAdByTargetMondayDateAsync(DateTime targetMondayDate, CancellationToken cancellationToken = default);

    Task<MovieCarouselAd?> GetActiveMovieAdAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovieCarouselAd>> GetMovieAdsAsync(CancellationToken cancellationToken = default);

    Task<bool> ShopExistsAsync(string shopId, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, ShopCatalogInfo>> GetShopCatalogInfoByIdsAsync(
        IEnumerable<string> shopIds,
        CancellationToken cancellationToken = default);

    Task<ManagerContactInfo?> GetManagerContactByShopIdAsync(string shopId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record ShopCatalogInfo(string ShopId, string ShopName, string? ShopSlug);

public sealed record ManagerContactInfo(string ShopId, string ShopName, string? Email, string? FullName);
