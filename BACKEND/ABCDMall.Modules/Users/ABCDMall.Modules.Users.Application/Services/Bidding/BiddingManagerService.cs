using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class BiddingManagerService : IBiddingManagerService
{
    private readonly IBiddingRepository _repository;
    private readonly IBidPaymentService _bidPaymentService;
    private readonly IFileStorageService _fileStorageService;

    public BiddingManagerService(
        IBiddingRepository repository,
        IBidPaymentService bidPaymentService,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _bidPaymentService = bidPaymentService;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApplicationResult<ManagerCarouselBidDto>> SubmitBidAsync(
        string shopId,
        SubmitCarouselBidRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopId))
        {
            return ApplicationResult<ManagerCarouselBidDto>.BadRequest("Manager account does not have a shop id.");
        }

        if (!await _repository.ShopExistsAsync(shopId, cancellationToken))
        {
            return ApplicationResult<ManagerCarouselBidDto>.NotFound("Shop not found.");
        }

        if (request.BidAmount <= 0)
        {
            return ApplicationResult<ManagerCarouselBidDto>.BadRequest("Bid amount must be greater than zero.");
        }

        if (!Enum.TryParse<CarouselBidTemplateType>(request.TemplateType, true, out var templateType))
        {
            return ApplicationResult<ManagerCarouselBidDto>.BadRequest("Unsupported template type.");
        }

        var nowUtc = DateTime.UtcNow;
        var templateBuildResult = await BuildTemplateDataAsync(templateType, request, nowUtc, cancellationToken);
        if (!templateBuildResult.Succeeded)
        {
            return ApplicationResult<ManagerCarouselBidDto>.BadRequest(templateBuildResult.Error!);
        }

        var targetMondayDate = BiddingBusinessClock.GetUpcomingWeekMonday(nowUtc);
        var bid = new CarouselBid
        {
            ShopId = shopId,
            BidAmount = decimal.Round(request.BidAmount, 2, MidpointRounding.AwayFromZero),
            TemplateType = templateType,
            TemplateData = templateBuildResult.TemplateData!,
            Status = CarouselBidStatus.Pending,
            TargetMondayDate = targetMondayDate,
            CreatedAt = nowUtc
        };

        await _repository.AddBidAsync(bid, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var shopLookup = await _repository.GetShopCatalogInfoByIdsAsync([shopId], cancellationToken);
        return ApplicationResult<ManagerCarouselBidDto>.Ok(MapManagerBid(
            bid,
            shopLookup.TryGetValue(shopId, out var shopInfo) ? shopInfo.ShopName : null));
    }

    public async Task<IReadOnlyList<ManagerCarouselBidDto>> GetBidHistoryAsync(
        string shopId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shopId))
        {
            return [];
        }

        var bids = await _repository.GetBidsByShopIdAsync(shopId, cancellationToken);
        var shopLookup = await _repository.GetShopCatalogInfoByIdsAsync([shopId], cancellationToken);
        var shopName = shopLookup.TryGetValue(shopId, out var shopInfo) ? shopInfo.ShopName : null;

        return bids
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapManagerBid(x, shopName))
            .ToArray();
    }

    public Task<ApplicationResult<BidPaymentCheckoutSessionDto>> CreatePaymentCheckoutSessionAsync(
        string shopId,
        string bidId,
        CancellationToken cancellationToken = default)
        => _bidPaymentService.CreateCheckoutSessionAsync(bidId, shopId, cancellationToken);

    private static ManagerCarouselBidDto MapManagerBid(CarouselBid bid, string? shopName)
    {
        var dto = new ManagerCarouselBidDto
        {
            Id = bid.Id ?? string.Empty,
            ShopId = bid.ShopId,
            ShopName = shopName,
            TemplateType = bid.TemplateType.ToString(),
            BidAmount = bid.BidAmount,
            Status = bid.Status.ToString(),
            TargetMondayDate = bid.TargetMondayDate,
            CreatedAt = bid.CreatedAt
        };

        return BiddingTemplateSerializer.ApplyTemplateDetails(dto, bid);
    }

    private async Task<TemplateBuildResult> BuildTemplateDataAsync(
        CarouselBidTemplateType templateType,
        SubmitCarouselBidRequestDto request,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        switch (templateType)
        {
            case CarouselBidTemplateType.ShopAd:
                if (request.ShopImageFile is null)
                {
                    return TemplateBuildResult.Fail("Shop image is required for ShopAd.");
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return TemplateBuildResult.Fail("Message is required for ShopAd.");
                }

                var shopImageUrl = await _fileStorageService.SaveBiddingImageAsync(request.ShopImageFile, cancellationToken);
                return TemplateBuildResult.Ok(BiddingTemplateSerializer.Serialize(new ShopAdTemplateData
                {
                    ShopImage = shopImageUrl,
                    Message = request.Message.Trim()
                }));

            case CarouselBidTemplateType.DiscountAd:
                if (request.ProductImageFile is null)
                {
                    return TemplateBuildResult.Fail("Product image is required for DiscountAd.");
                }

                if (!request.OriginalPrice.HasValue || request.OriginalPrice.Value <= 0)
                {
                    return TemplateBuildResult.Fail("Original price is required for DiscountAd.");
                }

                if (!request.DiscountPrice.HasValue || request.DiscountPrice.Value <= 0)
                {
                    return TemplateBuildResult.Fail("Discount price is required for DiscountAd.");
                }

                if (request.DiscountPrice.Value >= request.OriginalPrice.Value)
                {
                    return TemplateBuildResult.Fail("Discount price must be lower than the original price.");
                }

                var productImageUrl = await _fileStorageService.SaveBiddingImageAsync(request.ProductImageFile, cancellationToken);
                return TemplateBuildResult.Ok(BiddingTemplateSerializer.Serialize(new DiscountAdTemplateData
                {
                    ProductImage = productImageUrl,
                    OriginalPrice = decimal.Round(request.OriginalPrice.Value, 2, MidpointRounding.AwayFromZero),
                    DiscountPrice = decimal.Round(request.DiscountPrice.Value, 2, MidpointRounding.AwayFromZero)
                }));

            case CarouselBidTemplateType.EventAd:
                if (request.EventImageFile is null)
                {
                    return TemplateBuildResult.Fail("Event image is required for EventAd.");
                }

                if (!request.StartDate.HasValue)
                {
                    return TemplateBuildResult.Fail("Start date is required for EventAd.");
                }

                if (string.IsNullOrWhiteSpace(request.StartTime))
                {
                    return TemplateBuildResult.Fail("Start time is required for EventAd.");
                }

                if (!TimeOnly.TryParse(request.StartTime, out var parsedStartTime))
                {
                    return TemplateBuildResult.Fail("Start time is invalid.");
                }

                var eventStartLocal = request.StartDate.Value.Date.Add(parsedStartTime.ToTimeSpan());
                var minimumStartLocal = BiddingBusinessClock.GetLocalNow(nowUtc).AddDays(3);
                if (eventStartLocal < minimumStartLocal)
                {
                    return TemplateBuildResult.Fail("Event start must be at least 3 days from now.");
                }

                var eventImageUrl = await _fileStorageService.SaveBiddingImageAsync(request.EventImageFile, cancellationToken);
                return TemplateBuildResult.Ok(BiddingTemplateSerializer.Serialize(new EventAdTemplateData
                {
                    EventImage = eventImageUrl,
                    StartDate = request.StartDate.Value.Date,
                    StartTime = parsedStartTime.ToString("HH:mm")
                }));

            default:
                return TemplateBuildResult.Fail("Unsupported template type.");
        }
    }

    private sealed record TemplateBuildResult(bool Succeeded, string? TemplateData, string? Error)
    {
        public static TemplateBuildResult Ok(string templateData) => new(true, templateData, null);

        public static TemplateBuildResult Fail(string error) => new(false, null, error);
    }
}
