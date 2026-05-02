using ABCDMall.Modules.Users.Application.Services.Bidding;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Seed;

public static class BiddingSeed
{
    // ──────────────────────────────────────────────────────────────────
    //  Tuần hiện tại: 7 shop tham gia đấu giá — top 5 cao nhất THẮNG
    //  (shop-001→shop-005: thắng; shop-006→shop-007: không thắng)
    // ──────────────────────────────────────────────────────────────────
    private static readonly BidSlot[] CurrentWeekSlots =
    [
        new("bid-current-01", "shop-001", "Nike",           580m, CarouselBidTemplateType.ShopAd),
        new("bid-current-02", "shop-002", "Adidas",         540m, CarouselBidTemplateType.DiscountAd),
        new("bid-current-03", "shop-003", "Uniqlo",         500m, CarouselBidTemplateType.ShopAd),
        new("bid-current-04", "shop-004", "Charles & Keith",460m, CarouselBidTemplateType.EventAd),
        new("bid-current-05", "shop-005", "Miniso",         420m, CarouselBidTemplateType.ShopAd),
        new("bid-current-06", "shop-006", "Pop Mart",       260m, CarouselBidTemplateType.DiscountAd),
        new("bid-current-07", "shop-007", "Levents",        230m, CarouselBidTemplateType.ShopAd),
    ];

    // ──────────────────────────────────────────────────────────────────
    //  Tuần sau: 6 shop tham gia đấu giá — top 5 cao nhất THẮNG
    //  (Pop Mart & Levents dẫn đầu tuần sau; Miniso không thắng)
    // ──────────────────────────────────────────────────────────────────
    private static readonly BidSlot[] NextWeekSlots =
    [
        new("bid-next-01", "shop-006", "Pop Mart",       620m, CarouselBidTemplateType.ShopAd),
        new("bid-next-02", "shop-007", "Levents",        590m, CarouselBidTemplateType.DiscountAd),
        new("bid-next-03", "shop-001", "Nike",           560m, CarouselBidTemplateType.EventAd),
        new("bid-next-04", "shop-002", "Adidas",         530m, CarouselBidTemplateType.ShopAd),
        new("bid-next-05", "shop-003", "Uniqlo",         500m, CarouselBidTemplateType.DiscountAd),
        new("bid-next-06", "shop-005", "Miniso",         310m, CarouselBidTemplateType.ShopAd),
    ];

    // ──────────────────────────────────────────────────────────────────
    //  Tuần trước: 5 shop (expired — dữ liệu lịch sử)
    // ──────────────────────────────────────────────────────────────────
    private static readonly BidSlot[] PastWeekSlots =
    [
        new("bid-past-01", "shop-001", "Nike",           520m, CarouselBidTemplateType.ShopAd),
        new("bid-past-02", "shop-002", "Adidas",         480m, CarouselBidTemplateType.ShopAd),
        new("bid-past-03", "shop-003", "Uniqlo",         450m, CarouselBidTemplateType.DiscountAd),
        new("bid-past-04", "shop-004", "Charles & Keith",400m, CarouselBidTemplateType.EventAd),
        new("bid-past-05", "shop-005", "Miniso",         350m, CarouselBidTemplateType.ShopAd),
    ];

    // Map shop → ảnh cover thực tế từ thư mục /img của mall
    private static readonly Dictionary<string, string> ShopImages = new(StringComparer.OrdinalIgnoreCase)
    {
        ["shop-001"] = "/img/nike/out.jpg",
        ["shop-002"] = "/img/adidas/out.webp",
        ["shop-003"] = "/img/uniqlo/out.jpg",
        ["shop-004"] = "/img/C&K/out.jpg",
        ["shop-005"] = "/img/miniso/out.jpg",
        ["shop-006"] = "/img/popmart/out.webp",
        ["shop-007"] = "/img/levents/OUT.webp",
    };

    public static async Task SeedAsync(MallDbContext db, CancellationToken ct = default)
    {
        var currentWeekMonday = BiddingBusinessClock.GetCurrentWeekMonday(DateTime.UtcNow);
        var pastWeekMonday    = currentWeekMonday.AddDays(-7);
        var nextWeekMonday    = currentWeekMonday.AddDays(7);

        // Past week: top-5 won & expired; all 5 slots are winners so loserStatus is irrelevant
        await SeedWeekAsync(db, PastWeekSlots,    pastWeekMonday,
            winnerStatus: CarouselBidStatus.Expired,
            loserStatus:  CarouselBidStatus.Lost,
            maxWinners: 5, daysBeforeMonday: 3, ct);

        // Current week: top-5 Active (shown on carousel); others Lost
        await SeedWeekAsync(db, CurrentWeekSlots, currentWeekMonday,
            winnerStatus: CarouselBidStatus.Active,
            loserStatus:  CarouselBidStatus.Lost,
            maxWinners: 5, daysBeforeMonday: 2, ct);

        // Next week: all Pending — resolution hasn't happened yet (demo: click "Simulate Saturday")
        await SeedWeekAsync(db, NextWeekSlots,    nextWeekMonday,
            winnerStatus: CarouselBidStatus.Pending,
            loserStatus:  CarouselBidStatus.Pending,
            maxWinners: 5, daysBeforeMonday: 2, ct);

        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-past",    pastWeekMonday,    isActive: false), ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-current", currentWeekMonday, isActive: true),  ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-next",    nextWeekMonday,    isActive: false), ct);

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedWeekAsync(
        MallDbContext db,
        BidSlot[] slots,
        DateTime monday,
        CarouselBidStatus winnerStatus,
        CarouselBidStatus loserStatus,
        int maxWinners,
        int daysBeforeMonday,
        CancellationToken ct)
    {
        // Rank by BidAmount descending; ties broken by array order (earlier = earlier CreatedAt = wins tie)
        var winnerIds = slots
            .OrderByDescending(s => s.BidAmount)
            .Take(maxWinners)
            .Select(s => s.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var bids = slots
            .Select((slot, index) => new CarouselBid
            {
                Id               = slot.Id,
                ShopId           = slot.ShopId,
                BidAmount        = slot.BidAmount,
                TemplateType     = slot.TemplateType,
                TemplateData     = BuildTemplateData(slot, index, monday),
                Status           = winnerIds.Contains(slot.Id) ? winnerStatus : loserStatus,
                TargetMondayDate = monday,
                CreatedAt        = monday.AddDays(-daysBeforeMonday).AddHours(index)
            })
            .ToArray();

        await UpsertBidsAsync(db, bids, ct);
    }

    private static async Task UpsertBidsAsync(MallDbContext db, IEnumerable<CarouselBid> seeds, CancellationToken ct)
    {
        var ids      = seeds.Select(x => x.Id!).ToArray();
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

            bid.ShopId           = seed.ShopId;
            bid.BidAmount        = seed.BidAmount;
            bid.TemplateType     = seed.TemplateType;
            bid.TemplateData     = seed.TemplateData;
            bid.Status           = seed.Status;
            bid.TargetMondayDate = seed.TargetMondayDate;
            bid.CreatedAt        = seed.CreatedAt;
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

        movieAd.ImageUrl         = seed.ImageUrl;
        movieAd.Description      = seed.Description;
        movieAd.TargetMondayDate = seed.TargetMondayDate;
        movieAd.IsActive         = seed.IsActive;
    }

    private static string BuildTemplateData(BidSlot slot, int index, DateTime monday)
    {
        var image = ShopImages.TryGetValue(slot.ShopId, out var img) ? img : ShopImages["shop-001"];

        return slot.TemplateType switch
        {
            CarouselBidTemplateType.ShopAd => BiddingTemplateSerializer.Serialize(new ShopAdTemplateData
            {
                ShopImage = image,
                Message   = $"Khám phá {slot.ShopName} — ưu đãi đặc biệt chỉ có tại ABCD Mall tuần này!"
            }),
            CarouselBidTemplateType.DiscountAd => BiddingTemplateSerializer.Serialize(new DiscountAdTemplateData
            {
                ProductImage   = image,
                OriginalPrice  = 1500m + (index * 100m),
                DiscountPrice  = 990m  + (index * 60m)
            }),
            CarouselBidTemplateType.EventAd => BiddingTemplateSerializer.Serialize(new EventAdTemplateData
            {
                EventImage = image,
                StartDate  = monday.AddDays(4),
                StartTime  = "19:30"
            }),
            _ => string.Empty
        };
    }

    private static MovieCarouselAd CreateMovieAd(string id, DateTime monday, bool isActive)
        => new()
        {
            Id               = id,
            ImageUrl         = "https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?auto=format&fit=crop&w=1600&q=80",
            Description      = isActive
                ? "Now showing at ABCD Cinema: exclusive premiere experiences on the big screen."
                : $"Weekly movie spotlight for {monday:dd/MM/yyyy}.",
            TargetMondayDate = monday,
            IsActive         = isActive
        };

    private sealed record BidSlot(
        string Id,
        string ShopId,
        string ShopName,
        decimal BidAmount,
        CarouselBidTemplateType TemplateType);
}
