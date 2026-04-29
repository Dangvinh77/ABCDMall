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

        // ĐÃ SỬA: Neo cứng ngày Demo để dữ liệu luôn chính xác cho buổi bảo vệ
        // Hôm nay: Thứ Tư, 29/04/2026
        var demoDate = new DateTime(2026, 4, 29, 12, 0, 0, DateTimeKind.Utc);
        
        var currentWeekMonday = BiddingBusinessClock.GetCurrentWeekMonday(demoDate); // 27/04/2026
        var pastWeekMonday = currentWeekMonday.AddDays(-7); // 20/04/2026
        var nextWeekMonday = currentWeekMonday.AddDays(7); // 04/05/2026

        await SeedPastWeekAsync(db, managerShopIds, pastWeekMonday, ct);
        await SeedCurrentWeekAsync(db, managerShopIds, currentWeekMonday, ct);
        
        // Truyền thêm currentWeekMonday để fix lỗi "Ngày tạo đơn ở tương lai"
        await SeedNextWeekAsync(db, managerShopIds, nextWeekMonday, currentWeekMonday, ct); 

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedPastWeekAsync(MallDbContext db, IReadOnlyList<string> shopIds, DateTime targetMonday, CancellationToken ct)
    {
        // 1. DỮ LIỆU LỊCH SỬ: Đấu giá hiển thị tuần trước -> Giờ đã hết hạn (Expired)
        var bids = CreateWeeklyBids("past", shopIds, targetMonday, CarouselBidStatus.Expired);
        await UpsertBidsAsync(db, bids, ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-past", targetMonday, isActive: false), ct);
    }

    private static async Task SeedCurrentWeekAsync(MallDbContext db, IReadOnlyList<string> shopIds, DateTime targetMonday, CancellationToken ct)
    {
        // 2. DỮ LIỆU HIỆN TẠI: Đấu giá tuần trước thắng -> Tuần này đang hiển thị trên Web (Active)
        var bids = CreateWeeklyBids("current", shopIds, targetMonday, CarouselBidStatus.Active);
        await UpsertBidsAsync(db, bids, ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-current", targetMonday, isActive: true), ct);
    }

    private static async Task SeedNextWeekAsync(MallDbContext db, IReadOnlyList<string> shopIds, DateTime targetMonday, DateTime currentWeekMonday, CancellationToken ct)
    {
        // 3. DỮ LIỆU DEMO ADMIN: 8 Shop đang đấu giá hôm nay để giành giật 5 slot của tuần sau (Pending)
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
                TemplateData = BuildTemplateData(templateType, index, targetMonday),
                Status = CarouselBidStatus.Pending,
                TargetMondayDate = targetMonday,
                
                // ĐÃ SỬA: Ngày nộp đơn đấu thầu là Thứ Ba (Hôm qua) hoặc sáng nay (Thứ Tư)
                // Đảm bảo không bị ảo ma Canada xuất hiện ngày nộp đơn ở tương lai.
                CreatedAt = currentWeekMonday.AddDays(1).AddHours(index) 
            });
        }

        await UpsertBidsAsync(db, bids, ct);
        await UpsertMovieAdAsync(db, CreateMovieAd("movie-ad-next", targetMonday, isActive: false), ct);
    }

    private static CarouselBid[] CreateWeeklyBids(
        string prefix,
        IReadOnlyList<string> shopIds,
        DateTime targetMonday,
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
                    TemplateData = BuildTemplateData(templateType, index, targetMonday),
                    Status = status,
                    TargetMondayDate = targetMonday,
                    // Thời gian tạo của các đơn đã Active/Expired là từ tuần trước đó nữa
                    CreatedAt = targetMonday.AddDays(-3).AddHours(index)
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