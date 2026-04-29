using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class BiddingAdminService : IBiddingAdminService
{
    private readonly IBiddingRepository _repository;
    private readonly IEmailNotificationService _emailNotificationService;

    public BiddingAdminService(
        IBiddingRepository repository,
        IEmailNotificationService emailNotificationService)
    {
        _repository = repository;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<IReadOnlyList<AdminCarouselBidListItemDto>> GetUpcomingWeekBidsAsync(CancellationToken cancellationToken = default)
    {
        var targetMondayDate = BiddingBusinessClock.GetUpcomingWeekMonday(DateTime.UtcNow);
        var bids = await _repository.GetBidsByTargetMondayAsync(targetMondayDate, cancellationToken);
        var shopLookup = await _repository.GetShopCatalogInfoByIdsAsync(
            bids.Select(x => x.ShopId).Distinct(StringComparer.OrdinalIgnoreCase),
            cancellationToken);

        return bids
            .OrderByDescending(x => x.BidAmount)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new AdminCarouselBidListItemDto
            {
                Id = x.Id ?? string.Empty,
                ShopId = x.ShopId,
                ShopName = shopLookup.TryGetValue(x.ShopId, out var shopInfo) ? shopInfo.ShopName : x.ShopId,
                TemplateType = x.TemplateType.ToString(),
                BidAmount = x.BidAmount,
                Status = x.Status.ToString(),
                TargetMondayDate = x.TargetMondayDate,
                CreatedAt = x.CreatedAt
            })
            .ToArray();
    }

    public async Task<ApplicationResult<MovieCarouselAdDto>> UpsertUpcomingWeekMovieAdAsync(
        CreateOrUpdateMovieCarouselAdRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            return ApplicationResult<MovieCarouselAdDto>.BadRequest("Image URL is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return ApplicationResult<MovieCarouselAdDto>.BadRequest("Description is required.");
        }

        var targetMondayDate = BiddingBusinessClock.GetUpcomingWeekMonday(DateTime.UtcNow);
        var movieAd = await _repository.GetMovieAdByTargetMondayDateAsync(targetMondayDate, cancellationToken);
        if (movieAd is null)
        {
            movieAd = new MovieCarouselAd
            {
                TargetMondayDate = targetMondayDate
            };

            await _repository.AddMovieAdAsync(movieAd, cancellationToken);
        }

        movieAd.ImageUrl = request.ImageUrl.Trim();
        movieAd.Description = request.Description.Trim();
        movieAd.IsActive = false;

        await _repository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MovieCarouselAdDto>.Ok(new MovieCarouselAdDto
        {
            Id = movieAd.Id ?? string.Empty,
            ImageUrl = movieAd.ImageUrl,
            Description = movieAd.Description,
            TargetMondayDate = movieAd.TargetMondayDate,
            IsActive = movieAd.IsActive
        });
    }

    public async Task<ApplicationResult<BidResolutionSummaryDto>> ResolveUpcomingWeekBidsAsync(CancellationToken cancellationToken = default)
    {
        var targetMondayDate = BiddingBusinessClock.GetUpcomingWeekMonday(DateTime.UtcNow);
        var pendingBids = (await _repository.GetBidsByTargetMondayAndStatusesAsync(
                targetMondayDate,
                [CarouselBidStatus.Pending],
                cancellationToken))
            .OrderByDescending(x => x.BidAmount)
            .ThenBy(x => x.CreatedAt)
            .ToArray();

        if (pendingBids.Length == 0)
        {
            return ApplicationResult<BidResolutionSummaryDto>.Ok(new BidResolutionSummaryDto
            {
                TargetMondayDate = targetMondayDate,
                TotalPending = 0,
                WonCount = 0,
                LostCount = 0
            });
        }

        var winners = pendingBids.Take(5).ToArray();
        var losers = pendingBids.Skip(5).ToArray();

        foreach (var winner in winners)
        {
            winner.Status = CarouselBidStatus.Won;
        }

        foreach (var loser in losers)
        {
            loser.Status = CarouselBidStatus.Lost;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        foreach (var winner in winners)
        {
            var contact = await _repository.GetManagerContactByShopIdAsync(winner.ShopId, cancellationToken);
            if (contact?.Email is null)
            {
                continue;
            }

            await _emailNotificationService.SendCarouselBidWonEmailAsync(
                contact.Email,
                contact.FullName,
                contact.ShopName,
                winner.BidAmount,
                winner.TargetMondayDate);
        }

        foreach (var loser in losers)
        {
            var contact = await _repository.GetManagerContactByShopIdAsync(loser.ShopId, cancellationToken);
            if (contact?.Email is null)
            {
                continue;
            }

            await _emailNotificationService.SendCarouselBidLostEmailAsync(
                contact.Email,
                contact.FullName,
                contact.ShopName,
                loser.BidAmount,
                loser.TargetMondayDate);
        }

        return ApplicationResult<BidResolutionSummaryDto>.Ok(new BidResolutionSummaryDto
        {
            TargetMondayDate = targetMondayDate,
            TotalPending = pendingBids.Length,
            WonCount = winners.Length,
            LostCount = losers.Length
        });
    }

    public async Task<ApplicationResult<BidPublishSummaryDto>> PublishUpcomingWeekCarouselAsync(CancellationToken cancellationToken = default)
    {
        var targetMondayDate = BiddingBusinessClock.GetUpcomingWeekMonday(DateTime.UtcNow);
        var activeBids = await _repository.GetActiveBidsAsync(cancellationToken);
        var publishableBids = (await _repository.GetBidsByTargetMondayAndStatusesAsync(
                targetMondayDate,
                [CarouselBidStatus.Paid, CarouselBidStatus.Active],
                cancellationToken))
            .OrderByDescending(x => x.BidAmount)
            .ThenBy(x => x.CreatedAt)
            .Take(5)
            .ToArray();

        foreach (var bid in activeBids)
        {
            bid.Status = CarouselBidStatus.Expired;
        }

        foreach (var bid in publishableBids)
        {
            bid.Status = CarouselBidStatus.Active;
        }

        var movieAds = await _repository.GetMovieAdsAsync(cancellationToken);
        var targetMovieAd = movieAds.FirstOrDefault(x => x.TargetMondayDate == targetMondayDate);

        foreach (var movieAd in movieAds)
        {
            movieAd.IsActive = false;
        }

        if (targetMovieAd is not null)
        {
            targetMovieAd.IsActive = true;
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<BidPublishSummaryDto>.Ok(new BidPublishSummaryDto
        {
            TargetMondayDate = targetMondayDate,
            ActiveBidCount = publishableBids.Length,
            MovieAdIncluded = targetMovieAd is not null,
            TotalSlots = publishableBids.Length + (targetMovieAd is null ? 0 : 1)
        });
    }
}
