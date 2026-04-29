using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public interface IBiddingAdminService
{
    Task<IReadOnlyList<AdminCarouselBidListItemDto>> GetUpcomingWeekBidsAsync(CancellationToken cancellationToken = default);

    Task<ApplicationResult<MovieCarouselAdDto>> UpsertUpcomingWeekMovieAdAsync(
        CreateOrUpdateMovieCarouselAdRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<BidResolutionSummaryDto>> ResolveUpcomingWeekBidsAsync(CancellationToken cancellationToken = default);

    Task<ApplicationResult<BidPublishSummaryDto>> PublishUpcomingWeekCarouselAsync(CancellationToken cancellationToken = default);
}
