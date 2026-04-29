using ABCDMall.Modules.Users.Application.DTOs.Bidding;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public interface IPublicCarouselQueryService
{
    Task<IReadOnlyList<PublicCarouselItemDto>> GetActiveCarouselItemsAsync(CancellationToken cancellationToken = default);
}
