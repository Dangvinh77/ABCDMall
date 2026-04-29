using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public interface IBiddingManagerService
{
    Task<ApplicationResult<ManagerCarouselBidDto>> SubmitBidAsync(
        string shopId,
        SubmitCarouselBidRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ManagerCarouselBidDto>> GetBidHistoryAsync(
        string shopId,
        CancellationToken cancellationToken = default);

    Task<ApplicationResult<BidPaymentCheckoutSessionDto>> CreatePaymentCheckoutSessionAsync(
        string shopId,
        string bidId,
        CancellationToken cancellationToken = default);
}
