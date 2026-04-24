using ABCDMall.Modules.Users.Application.Services.Bidding;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Seed;

public static class BiddingSeed
{
    public static async Task SeedAsync(MallDbContext db, CancellationToken ct = default)
    {
        var managerShopIds = await db.Users
            .Where(x => x.Role == "Manager" && !string.IsNullOrWhiteSpace(x.ShopId))
            .OrderBy(x => x.Email)
            .Select(x => x.ShopId!)
            .Distinct()
            .Take(5)
            .ToArrayAsync(ct);

        if (managerShopIds.Length == 0)
        {
            return;
        }

        var currentWeekMonday = BiddingBusinessClock.GetCurrentWeekMonday(DateTime.UtcNow);
        var pastWeekMonday = currentWeekMonday.AddDays(-7);
        var nextWeekMonday = currentWeekMonday.AddDays(7);

        await SeedPastWeekAsync(db, managerShopIds, pastWeekMonday, ct);
        await SeedCurrentWeekAsync(db, managerShopIds, currentWeekMonday, ct);
        await SeedNextWeekAsync(db, managerShopIds, nextWeekMonday, ct);

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedPastWeekAsync(MallDbContext db, IReadOnlyList<string> shopIds, DateTime monday, CancellationToken ct)
    {
        var bids = CreateWeeklyBids("past", shopIds, monday, CarouselBidStatus.Expired);
        await UpsertBidsAsync(db, bids, ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-past", monday, isActive: false), ct);
    }

    private static async Task SeedCurrentWeekAsync(MallDbContext db, IReadOnlyList<string> shopIds, DateTime monday, CancellationToken ct)
    {
        var bids = CreateWeeklyBids("current", shopIds, monday, CarouselBidStatus.Active);
        await UpsertBidsAsync(db, bids, ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-current", monday, isActive: true), ct);
    }

    private static async Task SeedNextWeekAsync(MallDbContext db, IReadOnlyList<string> shopIds, DateTime monday, CancellationToken ct)
    {
        var bidAmounts = new[] { 190m, 275m, 225m, 310m, 255m, 205m, 340m, 295m };
        var bids = new List<CarouselBid>(8);

        for (var index = 0; index < 8; index++)
        {
            var shopId = shopIds[index % shopIds.Count];
            var templateType = (CarouselBidTemplateType)(index % 3);
            bids.Add(new CarouselBid
            {
                Id = $"bid-next-{index + 1:00}",
                ShopId = shopId,
                BidAmount = bidAmounts[index],
                TemplateType = templateType,
                TemplateData = BuildTemplateData(templateType, index, monday),
                Status = CarouselBidStatus.Pending,
                TargetMondayDate = monday,
                CreatedAt = monday.AddDays(-2).AddHours(index)
            });
        }

        await UpsertBidsAsync(db, bids, ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-next", monday, isActive: false), ct);
    }

    private static CarouselBid[] CreateWeeklyBids(
        string prefix,
        IReadOnlyList<string> shopIds,
        DateTime monday,
        CarouselBidStatus status)
    {
        return shopIds
            .Select((shopId, index) =>
            {
                var templateType = (CarouselBidTemplateType)(index % 3);
                return new CarouselBid
                {
                    Id = $"bid-{prefix}-{index + 1:00}",
                    ShopId = shopId,
                    BidAmount = 180m + (index * 25m),
                    TemplateType = templateType,
                    TemplateData = BuildTemplateData(templateType, index, monday),
                    Status = status,
                    TargetMondayDate = monday,
                    CreatedAt = monday.AddDays(-3).AddHours(index)
                };
            })
            .ToArray();
    }

    private static async Task UpsertBidsAsync(MallDbContext db, IEnumerable<CarouselBid> seeds, CancellationToken ct)
    {
        var ids = seeds.Select(x => x.Id!).ToArray();
        var existing = await db.CarouselBids
            .Where(x => ids.Contains(x.Id!))
            .ToDictionaryAsync(x => x.Id!, StringComparer.OrdinalIgnoreCase, ct);

        foreach (var seed in seeds)
        {
            if (!existing.TryGetValue(seed.Id!, out var bid))
            {
                bid = new CarouselBid { Id = seed.Id };
                await db.CarouselBids.AddAsync(bid, ct);
            }

            bid.ShopId = seed.ShopId;
            bid.BidAmount = seed.BidAmount;
            bid.TemplateType = seed.TemplateType;
            bid.TemplateData = seed.TemplateData;
            bid.Status = seed.Status;
            bid.TargetMondayDate = seed.TargetMondayDate;
            bid.CreatedAt = seed.CreatedAt;
        }
    }

    private static async Task UpsertMovieAdAsync(MallDbContext db, MovieCarouselAd seed, CancellationToken ct)
    {
        var movieAd = await db.MovieCarouselAds.FirstOrDefaultAsync(x => x.Id == seed.Id, ct);
        if (movieAd is null)
        {
            movieAd = new MovieCarouselAd { Id = seed.Id };
            await db.MovieCarouselAds.AddAsync(movieAd, ct);
        }

        movieAd.ImageUrl = seed.ImageUrl;
        movieAd.Description = seed.Description;
        movieAd.TargetMondayDate = seed.TargetMondayDate;
        movieAd.IsActive = seed.IsActive;
    }

    private static string BuildTemplateData(CarouselBidTemplateType templateType, int index, DateTime monday)
    {
        return templateType switch
        {
            CarouselBidTemplateType.ShopAd => BiddingTemplateSerializer.Serialize(new ShopAdTemplateData
            {
                ShopImage = $"https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=1400&q=80&sig={index + 10}",
                Message = $"Featured collection drop #{index + 1} at ABCD Mall."
            }),
            CarouselBidTemplateType.DiscountAd => BiddingTemplateSerializer.Serialize(new DiscountAdTemplateData
            {
                ProductImage = $"https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1400&q=80&sig={index + 20}",
                OriginalPrice = 1200m + (index * 50m),
                DiscountPrice = 850m + (index * 25m)
            }),
            CarouselBidTemplateType.EventAd => BiddingTemplateSerializer.Serialize(new EventAdTemplateData
            {
                EventImage = $"https://images.unsplash.com/photo-1511578314322-379afb476865?auto=format&fit=crop&w=1400&q=80&sig={index + 30}",
                StartDate = monday.AddDays(4),
                StartTime = "19:30"
            }),
            _ => string.Empty
        };
    }

    private static MovieCarouselAd CreateMovieAd(string id, DateTime monday, bool isActive)
        => new()
        {
            Id = id,
            ImageUrl = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?auto=format&fit=crop&w=1600&q=80",
            Description = isActive
                ? "Now showing at ABCD Cinema: exclusive premiere experiences on the big screen."
                : $"Weekly movie spotlight for {monday:dd/MM/yyyy}.",
            TargetMondayDate = monday,
            IsActive = isActive
        };
}
