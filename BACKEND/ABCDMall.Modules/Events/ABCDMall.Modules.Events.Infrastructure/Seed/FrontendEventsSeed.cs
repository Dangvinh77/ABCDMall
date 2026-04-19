using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Domain.Enums;
using ABCDMall.Modules.Events.Infrastructure.Persistence.Events;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Events.Infrastructure.Seed;

public static class FrontendEventsSeed
{
    public static async Task SeedAsync(EventsDbContext db, CancellationToken ct = default)
    {
        var seedIds = EventSeeds.Select(s => s.Id).ToList();

        var existing = await db.Events
            .Where(x => seedIds.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var seed in EventSeeds)
        {
            var item = existing.FirstOrDefault(x => x.Id == seed.Id);
            if (item is null)
            {
                item = new Event { Id = seed.Id };
                await db.Events.AddAsync(item, ct);
                existing.Add(item);
            }

            item.Title         = seed.Title;
            item.Description   = seed.Description;
            item.CoverImageUrl = seed.CoverImageUrl;
            item.StartDate     = seed.StartDate;
            item.EndDate       = seed.EndDate;
            item.Location      = seed.Location;
            item.EventType     = seed.EventType;
            item.ShopId        = seed.ShopId;
            item.ShopName      = seed.ShopName;
            item.IsHot         = seed.IsHot;
            item.CreatedAt     = seed.CreatedAt;
        }

        await db.SaveChangesAsync(ct);
    }

    private sealed record EventSeed(
        Guid Id,
        string Title,
        string Description,
        string CoverImageUrl,
        DateTime StartDate,
        DateTime EndDate,
        string Location,
        EventType EventType,
        string? ShopId,
        string? ShopName,
        bool IsHot,
        DateTime CreatedAt);

    private static DateTime Utc(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        => new(year, month, day, hour, minute, second, DateTimeKind.Utc);

    private static readonly EventSeed[] EventSeeds =
    [
        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000101"),
            Title:         "ABCD Summer Kick-Off Fair 2026",
            Description:   "Sự kiện mở màn mùa mua sắm tháng 4 với cụm gian hàng ưu đãi, quà check-in, coupon liên tầng và khung giờ flash deal tại khu atrium trung tâm.",
            CoverImageUrl: "https://images.unsplash.com/photo-1521334884684-d80222895322?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 4, 10, 9, 30, 0),
            EndDate:       Utc(2026, 4, 28, 22, 0, 0),
            Location:      "Sảnh Trung Tâm - Floor 1",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         true,
            CreatedAt:     Utc(2026, 3, 28, 8, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000102"),
            Title:         "Beauty & Self-Care Week",
            Description:   "Tuần lễ trải nghiệm beauty với soi da nhanh, mini workshop makeup, quầy thử sản phẩm và ưu đãi theo hóa đơn từ các gian hàng mỹ phẩm, phụ kiện và wellness.",
            CoverImageUrl: "https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 4, 14, 10, 0, 0),
            EndDate:       Utc(2026, 4, 22, 21, 30, 0),
            Location:      "Khu Sự Kiện Cánh Đông - Floor 1",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 1, 9, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000103"),
            Title:         "Kids Creative Playground",
            Description:   "Khu vui chơi sáng tạo cho gia đình với tô tượng, lắp ghép mô hình mini, góc đọc truyện và hoạt động tích điểm nhận quà cuối tuần cho khách có hóa đơn trong mall.",
            CoverImageUrl: "https://images.unsplash.com/photo-1516627145497-ae6968895b74?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 4, 18, 9, 30, 0),
            EndDate:       Utc(2026, 4, 27, 21, 30, 0),
            Location:      "Khu Gia Đình - Floor 2",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 5, 10, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000104"),
            Title:         "Streetwear & Sneaker District",
            Description:   "Không gian trưng bày outfit, sneaker wall, khu phối đồ nhanh và mini game cho cộng đồng streetwear, lấy cảm hứng từ các dòng lifestyle và skate đang được quan tâm.",
            CoverImageUrl: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 4, 17, 10, 0, 0),
            EndDate:       Utc(2026, 4, 24, 22, 0, 0),
            Location:      "Atrium Tây - Floor 3",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 3, 11, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000105"),
            Title:         "Gift & Lifestyle Bazaar",
            Description:   "Sự kiện quà tặng và đồ dùng phong cách sống với các booth phụ kiện, văn phòng phẩm, collectibles, đồ gia dụng nhỏ và khu gói quà nhanh theo hóa đơn.",
            CoverImageUrl: "https://images.unsplash.com/photo-1512436991641-6745cdb1723f?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 4, 12, 9, 30, 0),
            EndDate:       Utc(2026, 4, 21, 21, 30, 0),
            Location:      "Hành Lang Trung Tâm - Floor 2",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 1, 8, 30, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000106"),
            Title:         "Mother's Day Gift Festival",
            Description:   "Sự kiện quà tặng dịp Mother's Day với khu tư vấn chọn quà, booth khắc thiệp, gift wrapping miễn phí và combo quà dành cho gia đình, beauty và jewelry.",
            CoverImageUrl: "https://images.unsplash.com/photo-1524504388940-b1c1722653e1?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 4, 29, 10, 0, 0),
            EndDate:       Utc(2026, 5, 10, 22, 0, 0),
            Location:      "Sảnh Trung Tâm - Floor 1",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 10, 9, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000107"),
            Title:         "Mid-Year Mega Sale Preview",
            Description:   "Chương trình preview sale giữa năm với coupon sớm, tích dấu mua sắm, khung giờ quà tặng và khu săn deal dành cho khách ghé mall trong tuần đầu tháng 5.",
            CoverImageUrl: "https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 5, 6, 9, 30, 0),
            EndDate:       Utc(2026, 5, 17, 22, 0, 0),
            Location:      "Sảnh Khuyến Mãi - Floor 1",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 18, 10, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000108"),
            Title:         "Book & Toy Discovery Days",
            Description:   "Chuỗi hoạt động đọc sách, thử boardgame, lắp mô hình mini và khu ảnh check-in dành cho gia đình có trẻ nhỏ, học sinh và nhóm bạn cuối tuần.",
            CoverImageUrl: "https://images.unsplash.com/photo-1512820790803-83ca734da794?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 5, 13, 10, 0, 0),
            EndDate:       Utc(2026, 5, 24, 21, 30, 0),
            Location:      "Không Gian Cộng Đồng - Floor 2",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 24, 9, 30, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000109"),
            Title:         "Tech Experience Carnival",
            Description:   "Ngày hội trải nghiệm công nghệ với demo tai nghe, TV, gaming console, phụ kiện thông minh và quà redeem cho khách hoàn thành trạm trải nghiệm.",
            CoverImageUrl: "https://images.unsplash.com/photo-1519389950473-47ba0277781c?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 5, 20, 10, 0, 0),
            EndDate:       Utc(2026, 5, 31, 22, 0, 0),
            Location:      "Khu Công Nghệ - Floor 4",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         false,
            CreatedAt:     Utc(2026, 5, 1, 8, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000110"),
            Title:         "Weekend Food & Music Garden",
            Description:   "Không gian cuối tuần với acoustic session, food combo, góc chill ngoài trời và chương trình tích stamp theo hóa đơn F&B trong toàn mall.",
            CoverImageUrl: "https://images.unsplash.com/photo-1504674900247-0877df9cc836?q=80&w=1600&auto=format&fit=crop",
            StartDate:     Utc(2026, 6, 5, 17, 0, 0),
            EndDate:       Utc(2026, 6, 14, 22, 0, 0),
            Location:      "Quảng Trường Ngoài Trời",
            EventType:     EventType.MallEvent,
            ShopId:        null,
            ShopName:      null,
            IsHot:         true,
            CreatedAt:     Utc(2026, 5, 10, 9, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000201"),
            Title:         "Uniqlo UT Culture Week",
            Description:   "Lấy cảm hứng từ dòng UT Collection và các capsule đang xuất hiện trên UNIQLO VN như Studio Ghibli, MAGIC FOR ALL và character UT; khách được thử phối đồ, in look card và nhận quà cho hóa đơn UT.",
            CoverImageUrl: "/img/uniqlo/out.jpg",
            StartDate:     Utc(2026, 4, 12, 9, 30, 0),
            EndDate:       Utc(2026, 4, 26, 22, 0, 0),
            Location:      "Cửa hàng Uniqlo - Lô 1-01, Floor 1",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-uniqlo",
            ShopName:      "Uniqlo",
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 2, 8, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000202"),
            Title:         "Adidas Samba Spotlight Weekend",
            Description:   "Sự kiện trưng bày và thử phối outfit xoay quanh dòng adidas Samba đang được nhấn mạnh trên adidas VN, kết hợp khu check-in, tư vấn size nhanh và voucher cho hóa đơn Originals.",
            CoverImageUrl: "/img/adidas/out.webp",
            StartDate:     Utc(2026, 4, 18, 10, 0, 0),
            EndDate:       Utc(2026, 4, 28, 22, 0, 0),
            Location:      "Cửa hàng Adidas - Lô 1-11, Floor 1",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-adidas",
            ShopName:      "Adidas",
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 4, 10, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000203"),
            Title:         "PNJ Style Bloomie Styling Days",
            Description:   "Workshop mini dựa trên tinh thần STYLE by PNJ và các BST như Bloomie, Friendship; khách được tư vấn phối trang sức theo outfit, quà tặng theo hóa đơn và khu chụp ảnh gift-ready.",
            CoverImageUrl: "/img/pnj/pnj_out.png",
            StartDate:     Utc(2026, 4, 16, 10, 0, 0),
            EndDate:       Utc(2026, 4, 24, 21, 30, 0),
            Location:      "Cửa hàng PNJ - Lô 1-27, Floor 1",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-pnj",
            ShopName:      "PNJ",
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 3, 9, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000204"),
            Title:         "Casio G-SHOCK Tough Lab",
            Description:   "Không gian trải nghiệm lấy cảm hứng từ các dòng G-SHOCK đang nổi bật trên CASIO Vietnam như G-STEEL, dòng 2100 và FULL METAL, kèm demo độ bền, phối strap và ưu đãi quà tặng.",
            CoverImageUrl: "/img/casio/out.png",
            StartDate:     Utc(2026, 4, 10, 10, 0, 0),
            EndDate:       Utc(2026, 4, 30, 22, 0, 0),
            Location:      "Cửa hàng Casio - Lô K1-11, Floor 1",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-casio",
            ShopName:      "Casio",
            IsHot:         false,
            CreatedAt:     Utc(2026, 3, 31, 14, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000205"),
            Title:         "Pop Mart Collector Weekend",
            Description:   "Weekend cho cộng đồng sưu tầm với khu blind-box opening, display shelf cho Labubu và Skullpanda, mini lucky draw và khu trao đổi checklist figure cho khách đã mua tại quầy.",
            CoverImageUrl: "/img/popmart/out.webp",
            StartDate:     Utc(2026, 4, 19, 9, 30, 0),
            EndDate:       Utc(2026, 4, 27, 21, 30, 0),
            Location:      "Cửa hàng Pop Mart - Lô 3-15, Floor 2",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-pop-mart",
            ShopName:      "Pop Mart",
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 6, 11, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000206"),
            Title:         "Levents x Hello Kitty Fantasy Drop",
            Description:   "Sự kiện pre-launch theo tinh thần collection Levents x Hello Kitty đang được giới thiệu trên kênh chính thức của Levents, kết hợp styling booth, quà check-in và ưu đãi cho local brand lovers.",
            CoverImageUrl: "/img/levents/OUT.webp",
            StartDate:     Utc(2026, 4, 25, 10, 0, 0),
            EndDate:       Utc(2026, 5, 3, 22, 0, 0),
            Location:      "Cửa hàng Levents - Lô 3-07, Floor 2",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-levents",
            ShopName:      "Levents",
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 12, 9, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000207"),
            Title:         "Vans Old Skool Music Collection",
            Description:   "Lấy cảm hứng từ câu chuyện Old Skool và Music Collection trên Vans, sự kiện cho khách thử mix giày, balo, phụ kiện và nhận photocard khi mua combo streetwear tại cửa hàng.",
            CoverImageUrl: "/img/vans/out.avif",
            StartDate:     Utc(2026, 4, 26, 10, 0, 0),
            EndDate:       Utc(2026, 5, 2, 22, 0, 0),
            Location:      "Cửa hàng Vans - Lô 5-04, Floor 3",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-vans",
            ShopName:      "Vans",
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 14, 8, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000208"),
            Title:         "Converse Chuck 70 Style Lab",
            Description:   "Workshop styling xoay quanh Chuck 70, dòng classic đang được Converse nhấn mạnh, với khu tư vấn phối đồ, thử giày nhanh và góc custom lace-tag cho khách mua trong ngày.",
            CoverImageUrl: "/img/converse/out.jpg",
            StartDate:     Utc(2026, 5, 1, 10, 0, 0),
            EndDate:       Utc(2026, 5, 10, 22, 0, 0),
            Location:      "Cửa hàng Converse - Lô 5-05, Floor 3",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-converse",
            ShopName:      "Converse",
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 18, 9, 0, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000209"),
            Title:         "LEGO Botanicals Build Studio",
            Description:   "Hoạt động build-and-display lấy cảm hứng từ LEGO Botanicals, đặc biệt các set Orchid và Mini Orchid; khách được thử lắp mẫu, nhận postcard hướng dẫn và ưu đãi cho set trưng bày.",
            CoverImageUrl: "/img/lego/out.webp",
            StartDate:     Utc(2026, 5, 8, 10, 0, 0),
            EndDate:       Utc(2026, 5, 17, 21, 30, 0),
            Location:      "Cửa hàng LEGO - Lô 6-02, Floor 4",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-lego",
            ShopName:      "LEGO",
            IsHot:         true,
            CreatedAt:     Utc(2026, 4, 22, 10, 30, 0)),

        new(
            Id:            new Guid("00000000-0000-0000-0000-000000000210"),
            Title:         "Sony Center PS5 & XM5 Experience Days",
            Description:   "Sự kiện trải nghiệm nhanh PlayStation 5, WH-1000XM5 và nhóm audio nổi bật của Sony Center, có tư vấn sản phẩm, demo gaming station và quà tặng theo hóa đơn công nghệ.",
            CoverImageUrl: "/img/sonycenter/out.png",
            StartDate:     Utc(2026, 5, 15, 10, 0, 0),
            EndDate:       Utc(2026, 5, 24, 22, 0, 0),
            Location:      "Cửa hàng Sony Center - Lô 6-06, Floor 4",
            EventType:     EventType.BrandEvent,
            ShopId:        "shop-sony-center",
            ShopName:      "Sony Center",
            IsHot:         false,
            CreatedAt:     Utc(2026, 4, 28, 8, 30, 0))
    ];
}
