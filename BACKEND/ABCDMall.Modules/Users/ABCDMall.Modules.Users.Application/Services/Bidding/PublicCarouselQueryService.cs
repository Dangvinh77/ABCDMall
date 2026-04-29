using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class PublicCarouselQueryService : IPublicCarouselQueryService
{
    private readonly IBiddingRepository _repository;

    public PublicCarouselQueryService(IBiddingRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<PublicCarouselItemDto>> GetActiveCarouselItemsAsync(CancellationToken cancellationToken = default)
    {
        var bids = (await _repository.GetActiveBidsAsync(cancellationToken))
            .OrderByDescending(x => x.BidAmount)
            .ThenBy(x => x.CreatedAt)
            .Take(5)
            .ToArray();

        var shopLookup = await _repository.GetShopCatalogInfoByIdsAsync(
            bids.Select(x => x.ShopId).Distinct(StringComparer.OrdinalIgnoreCase),
            cancellationToken);

        var items = new List<PublicCarouselItemDto>(6);
        var activeMovieAd = await _repository.GetActiveMovieAdAsync(cancellationToken);
        if (activeMovieAd is not null)
        {
            items.Add(new PublicCarouselItemDto
            {
                Id = activeMovieAd.Id ?? string.Empty,
                SlotType = "MovieAd",
                TargetMondayDate = activeMovieAd.TargetMondayDate,
                ImageUrl = activeMovieAd.ImageUrl,
                Description = activeMovieAd.Description,
                LinkUrl = "/movies"
            });
        }

        foreach (var bid in bids)
        {
            shopLookup.TryGetValue(bid.ShopId, out var shopInfo);
            items.Add(BiddingTemplateSerializer.MapBidToPublicItem(bid, shopInfo?.ShopName, shopInfo?.ShopSlug));
        }

        return items
            .OrderBy(x => x.SlotType == "MovieAd" ? 0 : 1)
            .ThenByDescending(x => bids.FirstOrDefault(b => b.Id == x.Id)?.BidAmount ?? 0m)
            .Take(6)
            .ToArray();
    }
}
