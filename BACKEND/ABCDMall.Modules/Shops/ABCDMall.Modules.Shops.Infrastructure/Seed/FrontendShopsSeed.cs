using ABCDMall.Modules.Shops.Domain.Entities;
using ABCDMall.Modules.Shops.Infrastructure.Persistence.Shops;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Shops.Infrastructure.Seed;

public static class FrontendShopsSeed
{
    public static async Task SeedAsync(ShopsDbContext db, CancellationToken ct = default)
    {
        foreach (var seed in ShopSeeds)
        {
            var shop = await db.Shops
                .Include(x => x.Tags)
                .Include(x => x.Products)
                .Include(x => x.Vouchers)
                .FirstOrDefaultAsync(x => x.Id == seed.Id, ct);

            if (shop is null)
            {
                shop = new Shop { Id = seed.Id };
                await db.Shops.AddAsync(shop, ct);
            }

            shop.Name = seed.Name;
            shop.Slug = seed.Slug;
            shop.Category = seed.Category;
            shop.Floor = seed.Floor;
            shop.LocationSlot = seed.LocationSlot;
            shop.Summary = seed.Summary;
            shop.Description = seed.Description;
            shop.LogoUrl = seed.LogoUrl;
            shop.CoverImageUrl = seed.CoverImageUrl;
            shop.OpenHours = seed.OpenHours;
            shop.Badge = seed.Badge;
            shop.Offer = seed.Offer;
            shop.OwnerShopId = seed.Id;

            shop.Tags.Clear();
            foreach (var tag in seed.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                shop.Tags.Add(new ShopTag
                {
                    Id = $"{seed.Id}-tag-{NormalizeToken(tag)}",
                    ShopId = seed.Id,
                    Value = tag
                });
            }

            shop.Products.Clear();
            foreach (var product in seed.Products)
            {
                shop.Products.Add(new ShopProduct
                {
                    Id = product.Id,
                    ShopId = seed.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    OldPrice = product.OldPrice,
                    DiscountPercent = product.DiscountPercent,
                    IsFeatured = product.IsFeatured,
                    IsDiscounted = product.IsDiscounted
                });
            }

            shop.Vouchers.Clear();
            foreach (var voucher in seed.Vouchers)
            {
                shop.Vouchers.Add(new ShopVoucher
                {
                    Id = voucher.Id,
                    ShopId = seed.Id,
                    Code = voucher.Code,
                    Title = voucher.Title,
                    Description = voucher.Description,
                    ValidUntil = voucher.ValidUntil,
                    IsActive = voucher.IsActive
                });
            }
        }
        var baseShops = ShopSeeds.ToList();
        var random = new Random();
        int totalDemoShops = 47; // Số lượng shop ảo bạn muốn tạo thêm

        for (int i = 1; i <= totalDemoShops; i++)
        {
            string demoShopId = $"shop-demo-{i}";

            // Lấy ngẫu nhiên 1 shop trong 23 shop gốc để "mượn" data
            var cloneFrom = baseShops[random.Next(baseShops.Count)];

            var shop = await db.Shops
                .Include(x => x.Tags)
                .Include(x => x.Products)
                .Include(x => x.Vouchers)
                .FirstOrDefaultAsync(x => x.Id == demoShopId, ct);

            if (shop is null)
            {
                shop = new Shop { Id = demoShopId };
                await db.Shops.AddAsync(shop, ct);
            }

            // Clone thông tin cơ bản
            shop.Name = $"Gian Hàng Demo {i} ({cloneFrom.Name})";
            shop.Slug = $"gian-hang-demo-{i}";
            shop.Category = cloneFrom.Category;
            shop.Floor = "Tầng 2";
            shop.LocationSlot = $"L2-{i:D2}";

            if (i % 5 == 0)
            {
                shop.OpeningDate = DateTime.UtcNow.AddDays(10);
                shop.Summary = "🌟 SẮP KHAI TRƯƠNG - " + cloneFrom.Summary;
            }
            else
            {
                shop.OpeningDate = DateTime.UtcNow.AddDays(-1); // Đã khai trương hôm qua
                shop.Summary = cloneFrom.Summary;
            }

            shop.Description = "Đây là gian hàng demo được tự động tạo data để lấp đầy Mall.";
            shop.LogoUrl = cloneFrom.LogoUrl;
            shop.CoverImageUrl = cloneFrom.CoverImageUrl;
            shop.OpenHours = "09:30 - 22:00";
            shop.OwnerShopId = demoShopId;

            // Clone Tags
            shop.Tags.Clear();
            foreach (var tag in cloneFrom.Tags)
            {
                shop.Tags.Add(new ShopTag
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = tag
                });
            }

            // Clone Products (Phải tạo Object mới để EF Core không báo lỗi tracking)
            shop.Products.Clear();
            foreach (var p in cloneFrom.Products)
            {
                shop.Products.Add(new ShopProduct
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    DiscountPercent = p.DiscountPercent,
                    IsFeatured = p.IsFeatured,
                    IsDiscounted = p.IsDiscounted
                });
            }

            // Clone Vouchers
            shop.Vouchers.Clear();
            foreach (var v in cloneFrom.Vouchers)
            {
                shop.Vouchers.Add(new ShopVoucher
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = $"{v.Code}-{i}", // Đổi mã voucher một chút
                    Title = v.Title,
                    Description = v.Description,
                    ValidUntil = v.ValidUntil,
                    IsActive = v.IsActive
                });
            }
        }

        // Cuối cùng lưu lại toàn bộ xuống DB
        await db.SaveChangesAsync(ct);
    }

    private static string NormalizeToken(string value)
        => value
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");

    private sealed record ProductSeed(
        string Id,
        string Name,
        string ImageUrl,
        decimal Price,
        decimal? OldPrice = null,
        int? DiscountPercent = null,
        bool IsFeatured = false,
        bool IsDiscounted = false);

    private sealed record VoucherSeed(
        string Id,
        string Code,
        string Title,
        string Description,
        string ValidUntil,
        bool IsActive = true);

    private sealed record ShopSeed(
        string Id,
        string Name,
        string Slug,
        string Category,
        string Floor,
        string LocationSlot,
        string Summary,
        string Description,
        string LogoUrl,
        string CoverImageUrl,
        string OpenHours,
        string? Badge,
        string? Offer,
        string[] Tags,
        ProductSeed[] Products,
        VoucherSeed[] Vouchers);

    private static readonly ShopSeed[] ShopSeeds =
    [
        new(
            "shop-uniqlo",
            "Uniqlo",
            "uniqlo",
            "Fashion & Lifestyle",
            "Floor 1",
            "1-01",
            "Japanese essentials and LifeWear staples for everyday comfort.",
            "Uniqlo brings practical Japanese apparel, layering basics, and seasonal collections into a bright flagship store designed for quick browsing and family shopping.",
            "/img/uniqlo/logo.png",
            "/img/uniqlo/out.jpg",
            "09:30 - 22:00",
            "Featured Store",
            "LifeWear picks updated weekly",
            ["Fashion", "Floor 1", "Japanese Brand"],
            [
                new("shop-uniqlo-product-1", "Uniqlo U Crew Neck T-Shirt", "https://image.uniqlo.com/UQ/ST3/vn/imagesgoods/455359/item/vngoods_00_455359.jpg", 249000m, null, null, true, false),
                new("shop-uniqlo-product-2", "UV Protection Jacket", "https://image.uniqlo.com/UQ/ST3/vn/imagesgoods/453773/item/vngoods_69_453773.jpg", 799000m, null, null, true, false),
                new("shop-uniqlo-product-3", "Smart Ankle Pants", "https://image.uniqlo.com/UQ/ST3/AsianCommon/imagesgoods/450251/item/goods_09_450251.jpg", 799000m, 999000m, 20, false, true)
            ],
            [new("shop-uniqlo-voucher-1", "UQWELCOME", "Save 100K", "Applied on bills from 1,000,000 VND after app signup.", "31/12/2026")]
        ),
        new(
            "shop-adidas",
            "Adidas",
            "adidas",
            "Sportswear",
            "Floor 1",
            "1-11",
            "Performance sneakers and Originals staples in one branded zone.",
            "Adidas combines running, football, and Originals collections with in-store launch drops for sneakers, apparel, and member-exclusive offers.",
            "/img/adidas/logo.png",
            "/img/adidas/out.webp",
            "09:30 - 22:00",
            "Featured Store",
            "Members save 15% on regular-price items",
            ["Sports", "Floor 1", "Sneakers"],
            [
                new("shop-adidas-product-1", "Samba OG", "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/3bbecbdf584e40398446a8bf0117cf62_9366/Giay_Samba_OG_trang_B75806_01_standard.jpg", 2700000m, null, null, true, false),
                new("shop-adidas-product-2", "Ultraboost 1.0", "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/119bc1694f2945d8ab93af6600bf8a96_9366/Giay_Ultraboost_1.0_trang_HQ4199_01_standard.jpg", 5000000m, null, null, true, false),
                new("shop-adidas-product-3", "Stan Smith", "https://assets.adidas.com/images/h_840,f_auto,q_auto,fl_lossy,c_fill,g_auto/68ae7ea7849b43eca70aac1e00f5146d_9366/Giay_Stan_Smith_trang_FX5502_01_standard.jpg", 1750000m, 2500000m, 30, false, true)
            ],
            [new("shop-adidas-voucher-1", "ADICLUB15", "15% off", "Exclusive for adiClub members on regular-price products.", "30/06/2026")]
        ),
        new(
            "shop-nike",
            "Nike",
            "nike",
            "Sportswear",
            "Floor 1",
            "1-07",
            "Global sports icon with top running, lifestyle, and training drops.",
            "Nike anchors the sneaker zone with Air Force, Pegasus, and Dri-FIT apparel, paired with launch-focused displays for best-selling performance gear.",
            "/img/nike/logo.webp",
            "/img/nike/out.jpg",
            "09:30 - 22:00",
            "Hot Drop",
            "New Air Max arrivals this week",
            ["Sports", "Floor 1", "Running"],
            [
                new("shop-nike-product-1", "Nike Air Force 1 '07", "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/b7d9211c-26e7-431a-ac24-b0540fb3c00f/air-force-1-07-mens-shoes-jBrhbr.png", 2939000m, null, null, true, false),
                new("shop-nike-product-2", "Air Zoom Pegasus 40", "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/8cbafcf5-3bf5-40b9-87a1-d87b3b4bc50d/pegasus-40-mens-road-running-shoes-MCVWzK.png", 3519000m, null, null, true, false),
                new("shop-nike-product-3", "Dri-FIT Training Tee", "https://static.nike.com/a/images/t_PDP_1728_v1/f_auto,q_auto:eco/e970a2f1-6a3f-42e1-884c-cb6104e4604e/dri-fit-mens-training-t-shirt-1JvTzR.png", 650000m, 850000m, 23, false, true)
            ],
            []
        ),
        new(
            "shop-levis",
            "Levi's",
            "levis",
            "Fashion & Lifestyle",
            "Floor 1",
            "1-02",
            "Classic denim staples and iconic American casualwear.",
            "Levi's brings signature 501 jeans, denim jackets, and logo tees into a premium denim-focused storefront built around timeless wardrobe essentials.",
            "/img/levi/logo.png",
            "/img/levi/out.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Denim", "Floor 1", "Fashion"],
            [
                new("shop-levis-product-1", "501 Original Jeans", "https://lsco.scene7.com/is/image/lsco/005010115-front-pdp?fmt=jpeg&qlt=70,1&op_sharpen=0&resMode=sharp2&op_usm=0.8,1,10,0&fit=crop,0&wid=450&hei=600", 2299000m, null, null, true, false),
                new("shop-levis-product-2", "Trucker Jacket", "https://lsco.scene7.com/is/image/lsco/723340146-front-pdp?fmt=jpeg&qlt=70,1&op_sharpen=0&resMode=sharp2&op_usm=0.8,1,10,0&fit=crop,0&wid=450&hei=600", 2599000m, null, null, true, false),
                new("shop-levis-product-3", "Logo Tee", "https://lsco.scene7.com/is/image/lsco/224910815-front-pdp?fmt=jpeg&qlt=70,1&op_sharpen=0&resMode=sharp2&op_usm=0.8,1,10,0&fit=crop,0&wid=450&hei=600", 499000m, 799000m, 37, false, true)
            ],
            []
        ),
        new(
            "shop-charles-keith",
            "Charles & Keith",
            "charles-keith",
            "Bags & Accessories",
            "Floor 1",
            "1-06",
            "Polished handbags, heels, and accessories for modern styling.",
            "Charles & Keith offers a curated mix of elevated bags, heels, and small leather goods with trend-led displays aimed at women’s fashion-forward shoppers.",
            "/img/C&K/logo.jpg",
            "/img/C&K/out.jpg",
            "09:30 - 22:00",
            null,
            "Selected accessories on seasonal markdown",
            ["Accessories", "Floor 1", "Women"],
            [
                new("shop-charles-keith-product-1", "Gabine Saddle Bag", "https://www.charleskeith.vn/dw/image/v2/BCWJ_PRD/on/demandware.static/-/Sites-vn-products/default/dw5b078020/images/hi-res/2021-L3-CK2-80781610-1-01-1.jpg?sw=1152&sh=1536", 2150000m, null, null, true, false),
                new("shop-charles-keith-product-2", "Classic Pointed Heels", "https://www.charleskeith.vn/dw/image/v2/BCWJ_PRD/on/demandware.static/-/Sites-vn-products/default/dw47b2c5dc/images/hi-res/2023-L6-CK1-60280385-01-1.jpg?sw=1152&sh=1536", 1390000m, null, null, true, false),
                new("shop-charles-keith-product-3", "Bow Wallet", "https://www.charleskeith.vn/dw/image/v2/BCWJ_PRD/on/demandware.static/-/Sites-vn-products/default/dw9026402f/images/hi-res/2023-L4-CK6-10770578-01-1.jpg?sw=1152&sh=1536", 950000m, 1250000m, 24, false, true)
            ],
            []
        ),
        new(
            "shop-beauty-box",
            "Beauty Box",
            "beauty-box",
            "Beauty & Cosmetics",
            "Floor 1",
            "SB-01",
            "K-beauty, makeup, and skincare brands in one compact beauty stop.",
            "Beauty Box gathers trending Korean, Japanese, and international beauty products in an approachable retail format with gifts and new-customer promotions.",
            "/img/beautybox/logo.png",
            "/img/beautybox/out.jpg",
            "09:30 - 22:00",
            "New Arrival",
            "20% off first purchase",
            ["Beauty", "Floor 1", "Skincare"],
            [
                new("shop-beauty-box-product-1", "CLIO Cushion", "https://product.hstatic.net/200000000133/product/clio_kill_cover_the_new_founwear_cushion_03_linen_1_fd3a928c0c1f4e199990141cc131100b_master.jpg", 850000m, null, null, true, false),
                new("shop-beauty-box-product-2", "MAC Powder Kiss", "https://product.hstatic.net/200000000133/product/son_kem_mac_powder_kiss_liquid_lipcolour_991_devoted_to_chili_1_18bc79fb665d4b85bd8ff8a5d3fdb983_master.jpg", 780000m, null, null, true, false),
                new("shop-beauty-box-product-3", "Romand Juicy Tint", "https://product.hstatic.net/200000000133/product/son_tint_romand_juicy_lasting_tint_25_bare_grape_1_8f17bc3dbb3f46f4b3069b47e5b22108_master.jpg", 209000m, 280000m, 25, false, true)
            ],
            [new("shop-beauty-box-voucher-1", "BBNEW20", "20% off first order", "Maximum 100,000 VND for newly registered customers.", "30/06/2026")]
        ),
        new(
            "shop-pnj",
            "PNJ",
            "pnj",
            "Jewelry",
            "Floor 1",
            "1-27",
            "Vietnamese fine jewelry with gifting and premium collections.",
            "PNJ offers gold, silver, and gemstone jewelry supported by gifting services and premium presentation for milestone purchases.",
            "/img/pnj/logo.jpg",
            "/img/pnj/pnj_out.png",
            "09:30 - 22:00",
            null,
            null,
            ["Jewelry", "Floor 1", "Premium"],
            [
                new("shop-pnj-product-1", "18K CZ Earrings", "https://cdn.pnj.io/images/detailed/141/gbxmxmy001099-bong-tai-vang-18k-dinh-da-cz-pnj.png", 4250000m, null, null, true, false),
                new("shop-pnj-product-2", "Silver Necklace", "https://cdn.pnj.io/images/detailed/146/gnsmmxw000305-day-chuyen-bac-dinh-da-pnjsilver.png", 850000m, null, null, true, false),
                new("shop-pnj-product-3", "14K Diamond Ring", "https://cdn.pnj.io/images/detailed/189/gnddddw004543-nhan-kim-cuong-vang-14k-pnj.png", 18500000m, 19500000m, 5, false, true)
            ],
            []
        ),
        new(
            "shop-pedro",
            "Pedro",
            "pedro",
            "Bags & Accessories",
            "Floor 1",
            "1-09",
            "Dress shoes, leather bags, and polished essentials for men and women.",
            "Pedro balances sharp footwear and accessories with contemporary styling, aimed at premium shoppers seeking versatile formal-casual pieces.",
            "/img/pedro/logo.png",
            "/img/pedro/pedro_outdoor.png",
            "09:30 - 22:00",
            null,
            null,
            ["Accessories", "Floor 1", "Leather"],
            [
                new("shop-pedro-product-1", "Leather Loafers", "https://media.maisononline.vn/sys_master/images/hea/h19/9274577879070/PM1-40940172-1_01.jpg", 2590000m, null, null, true, false),
                new("shop-pedro-product-2", "Messenger Bag", "https://media.maisononline.vn/sys_master/images/h5a/hf0/9248742514718/PM2-26320141_01.jpg", 1890000m, null, null, true, false),
                new("shop-pedro-product-3", "Studio Top Handle Bag", "https://media.maisononline.vn/sys_master/images/h04/h27/9385078554654/PW2-56610098_01.jpg", 1690000m, 2190000m, 22, false, true)
            ],
            []
        ),
        new(
            "shop-casio",
            "Casio",
            "casio",
            "Watches & Accessories",
            "Floor 1",
            "K1-11",
            "Official Casio watches with G-Shock, Vintage, and Edifice staples.",
            "Casio serves shoppers looking for durable daily watches, sporty G-Shock models, and gift-ready timepieces across casual and premium segments.",
            "/img/casio/logo.png",
            "/img/casio/out.png",
            "09:30 - 22:00",
            null,
            null,
            ["Watches", "Floor 1", "Accessories"],
            [
                new("shop-casio-product-1", "G-Shock GA-2100", "https://www.casio.com/content/dam/casio/product-info/locales/vn/vi/timepiece/product/watch/G/GA/GA-2/GA-2100-1A1/assets/GA-2100-1A1_1.png.transform/main-visual-pc/image.png", 3280000m, null, null, true, false),
                new("shop-casio-product-2", "Vintage LA680WA", "https://www.casio.com/content/dam/casio/product-info/locales/vn/vi/timepiece/product/watch/L/LA/LA6/LA680WA-7/assets/LA680WA-7_1.png.transform/main-visual-pc/image.png", 1150000m, null, null, true, false),
                new("shop-casio-product-3", "Edifice EFV-550L", "https://www.casio.com/content/dam/casio/product-info/locales/vn/vi/timepiece/product/watch/E/EF/EFV/EFV-550L-1A/assets/EFV-550L-1AV_1.png.transform/main-visual-pc/image.png", 2500000m, 3100000m, 19, false, true)
            ],
            []
        ),
        new(
            "shop-phuong-nam",
            "Phuong Nam Book City",
            "phuong-nam",
            "Books & Gifts",
            "Floor 2",
            "3-01",
            "Books, stationery, gifts, and educational finds in one cultural stop.",
            "Phuong Nam Book City combines bestseller shelves, premium stationery, and gift corners in a family-friendly bookstore layout for leisurely discovery.",
            "/img/phuongnam/logo.jpg",
            "/img/phuongnam/out.jpg",
            "09:30 - 22:00",
            "Family Favorite",
            "Gift bookmark on selected literature purchases",
            ["Books", "Floor 2", "Stationery"],
            [
                new("shop-phuong-nam-product-1", "Muon Kiep Nhan Sinh", "https://nhasachphuongnam.com/images/detailed/164/muon-kiep-nhan-sinh-b2.jpg", 168000m, null, null, true, false),
                new("shop-phuong-nam-product-2", "Dac Nhan Tam", "https://nhasachphuongnam.com/images/detailed/161/dac-nhan-tam.jpg", 88000m, null, null, true, false),
                new("shop-phuong-nam-product-3", "Lamy Safari Fountain Pen", "https://nhasachphuongnam.com/images/detailed/160/lamy-safari.jpg", 690000m, null, null, false, false)
            ],
            [new("shop-phuong-nam-voucher-1", "PNBCOCT", "Free bookmark", "Receive a premium bookmark with literature bills from 500,000 VND.", "31/12/2026")]
        ),
        new(
            "shop-pop-mart",
            "Pop Mart",
            "pop-mart",
            "Collectibles & Toys",
            "Floor 2",
            "3-15",
            "Blind boxes, art toys, and limited collectible figures.",
            "Pop Mart brings trending IPs like Labubu and Skullpanda into a high-energy collectible space with blind-box walls and premium display shelves.",
            "/img/popmart/logo.jpg",
            "/img/popmart/out.webp",
            "09:30 - 22:00",
            "Collector Hotspot",
            "Limited blind-box drops every weekend",
            ["Collectibles", "Floor 2", "Toys"],
            [
                new("shop-pop-mart-product-1", "Labubu Tasty Macarons", "https://media.karousell.com/media/photos/products/2023/12/3/pop_mart_labubu_macaron_1701584742_91a0c02c_progressive.jpg", 450000m, null, null, true, false),
                new("shop-pop-mart-product-2", "Skullpanda Image of Reality", "https://global.popmart.com/cdn/shop/files/7_7b84a9e5-9f5b-4357-9db0-5e3e14fb5e74.jpg", 280000m, null, null, true, false),
                new("shop-pop-mart-product-3", "Mega Space Molly 400%", "https://global.popmart.com/cdn/shop/files/1_dbca665c-3f9c-4ebc-88e5-eb65ea4e4f8d.jpg", 5500000m, null, null, false, false)
            ],
            []
        ),
        new(
            "shop-miniso",
            "Miniso",
            "miniso",
            "Lifestyle Store",
            "Floor 2",
            "3-03A",
            "Affordable lifestyle accessories, plush toys, and home goods.",
            "Miniso offers fun daily-use products, licensed character accessories, and impulse-friendly gifts for students, families, and office shoppers.",
            "/img/miniso/logo.jpg",
            "/img/miniso/out.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Lifestyle", "Floor 2", "Gifts"],
            [
                new("shop-miniso-product-1", "Capybara Plush", "https://minisovietnam.com.vn/wp-content/uploads/2024/01/6936735398282-1.jpg", 199000m, null, null, true, false),
                new("shop-miniso-product-2", "Lotso Bottle", "https://minisovietnam.com.vn/wp-content/uploads/2023/08/6936735368384-1.jpg", 259000m, null, null, true, false),
                new("shop-miniso-product-3", "Sanrio Bluetooth Earbuds", "https://minisovietnam.com.vn/wp-content/uploads/2023/07/6936735352321-1.jpg", 399000m, 499000m, 20, false, true)
            ],
            []
        ),
        new(
            "shop-ninomaxx",
            "Ninomaxx",
            "ninomaxx",
            "Fashion & Lifestyle",
            "Floor 2",
            "3-31",
            "Vietnamese casualwear with affordable wardrobe basics and outerwear.",
            "Ninomaxx focuses on everyday menswear and casual essentials with approachable pricing, simple silhouettes, and mall-friendly seasonal promotions.",
            "/img/NINOMAXX/logo.png",
            "/img/NINOMAXX/out.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Fashion", "Floor 2", "Casual"],
            [
                new("shop-ninomaxx-product-1", "Polo Shirt", "https://ninomaxxconcept.com/wp-content/uploads/2023/11/2311025-1.jpg", 459000m, null, null, true, false),
                new("shop-ninomaxx-product-2", "Slimfit Khaki Pants", "https://ninomaxxconcept.com/wp-content/uploads/2023/10/2310118-1.jpg", 599000m, null, null, true, false),
                new("shop-ninomaxx-product-3", "Water-Resistant Windbreaker", "https://ninomaxxconcept.com/wp-content/uploads/2023/11/2311054-1.jpg", 489000m, 699000m, 30, false, true)
            ],
            []
        ),
        new(
            "shop-levents",
            "Levents",
            "levents",
            "Streetwear",
            "Floor 2",
            "3-07",
            "Vietnamese streetwear label with oversized fits and graphic staples.",
            "Levents positions itself as a youth-driven local brand with hoodies, tees, and accessories styled for the modern streetwear audience.",
            "/img/levents/logo.png",
            "/img/levents/OUT.webp",
            "09:30 - 22:00",
            "Local Brand",
            "Free shipping on 500K orders",
            ["Streetwear", "Floor 2", "Local Brand"],
            [
                new("shop-levents-product-1", "Classic Zipper Hoodie", "https://product.hstatic.net/1000312752/product/ao_khoac_ni_hoodie_zip_local_brand_levents_classic_zipper_hoodie_black_2_0c3bf310b1a04d2ab9c704f0f089f268_master.jpg", 650000m, null, null, true, false),
                new("shop-levents-product-2", "Basic Tee", "https://product.hstatic.net/1000312752/product/ao_thun_local_brand_levents_basic_tee_white_2_d15f2122607e4d8fb74f260bc34ba0bb_master.jpg", 350000m, null, null, true, false),
                new("shop-levents-product-3", "Mini Logo Backpack", "https://product.hstatic.net/1000312752/product/balo_local_brand_levents_mini_logo_backpack_black_1_bb481878b27c4b4d8ec8891d1eeb2c0b_master.jpg", 490000m, 690000m, 28, false, true)
            ],
            [new("shop-levents-voucher-1", "LVTFREE", "Free shipping", "Applied on orders from 500,000 VND.", "No expiry")]
        ),
        new(
            "shop-rabity",
            "Rabity",
            "rabity",
            "Kids Fashion",
            "Floor 2",
            "3-03",
            "Premium children’s fashion with playful designs and safe fabrics.",
            "Rabity serves family shoppers with comfortable children’s apparel, licensed character prints, and gift-ready seasonal collections for babies and kids.",
            "/img/rabity/logo.jpg",
            "/img/rabity/out.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Kids", "Floor 2", "Family"],
            [
                new("shop-rabity-product-1", "Marvel T-Shirt", "https://rabity.vn/cdn/shop/files/92408_1_540x.png", 159000m, null, null, true, false),
                new("shop-rabity-product-2", "Elsa Dress", "https://rabity.vn/cdn/shop/files/92398_1_540x.png", 329000m, null, null, true, false),
                new("shop-rabity-product-3", "Mickey Set", "https://rabity.vn/cdn/shop/files/92405_1_540x.png", 175000m, 250000m, 30, false, true)
            ],
            []
        ),
        new(
            "shop-boo",
            "Boo",
            "boo",
            "Streetwear",
            "Floor 2",
            "3-08",
            "Vietnamese streetwear pioneer with graphic tees and youth-driven denim.",
            "Boo caters to a young streetwear audience with bold graphics, oversized fits, and promotional drops that sit between local culture and global casual style.",
            "/img/boo/logo.png",
            "/img/boo/out.jpg",
            "09:30 - 22:00",
            "Youth Pick",
            null,
            ["Streetwear", "Floor 2", "Local Brand"],
            [
                new("shop-boo-product-1", "Marvel Graphic Tee", "https://boo.vn/wp-content/uploads/2023/12/1-4-600x800.jpg", 349000m, null, null, true, false),
                new("shop-boo-product-2", "Wide-Leg Jeans", "https://boo.vn/wp-content/uploads/2023/11/1-600x800.jpg", 599000m, null, null, true, false),
                new("shop-boo-product-3", "BOOZilla Hoodie", "https://boo.vn/wp-content/uploads/2023/10/1-1-600x800.jpg", 489000m, 699000m, 30, false, true)
            ],
            []
        ),
        new(
            "shop-john-henry",
            "John Henry",
            "john-henry",
            "Menswear",
            "Floor 2",
            "3-09",
            "Smart menswear inspired by American office and weekend dressing.",
            "John Henry offers polished shirts, trousers, and bamboo polos for shoppers who want dependable office-ready menswear with a classic retail presentation.",
            "/img/johnhenry/logo.png",
            "/img/johnhenry/out.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Menswear", "Floor 2", "Smart Casual"],
            [
                new("shop-john-henry-product-1", "Wrinkle-Resistant White Shirt", "https://johnhenry.vn/cdn/shop/files/SM2301_1_540x.jpg", 750000m, null, null, true, false),
                new("shop-john-henry-product-2", "Regular Fit Trousers", "https://johnhenry.vn/cdn/shop/files/QT2305_1_540x.jpg", 890000m, null, null, true, false),
                new("shop-john-henry-product-3", "Bamboo Polo", "https://johnhenry.vn/cdn/shop/files/PL2310_1_540x.jpg", 450000m, 590000m, 23, false, true)
            ],
            []
        ),
        new(
            "shop-lugvn",
            "Lug.vn",
            "lugvn",
            "Travel & Bags",
            "Floor 2",
            "3-12",
            "Travel bags, luggage, and utility backpacks for daily and long-haul trips.",
            "Lug.vn specializes in luggage and travel accessories, mixing lightweight suitcases with practical backpacks for shoppers planning trips or everyday commuting.",
            "/img/lug/logo.png",
            "/img/lug/out.webp",
            "09:30 - 22:00",
            null,
            "10% off large luggage this week",
            ["Travel", "Floor 2", "Bags"],
            [
                new("shop-lugvn-product-1", "Echolac Luggage", "https://lug.vn/images/products/2023/11/17/large/vali-nhua-echolac-pc185sa-20-do_1700201633.jpg", 2500000m, null, null, true, false),
                new("shop-lugvn-product-2", "Lusetti Water-Resistant Backpack", "https://lug.vn/images/products/2023/08/11/large/balo-laptop-lusetti-lsb1150-den_1691740924.jpg", 499000m, 990000m, 50, false, true)
            ],
            [new("shop-lugvn-voucher-1", "LUGTRIP", "10% off size L luggage", "Not combinable with other promotions.", "This week only")]
        ),
        new(
            "shop-powerbowl",
            "Powerbowl 388",
            "powerbowl",
            "Entertainment",
            "Floor 3",
            "5-01",
            "Bowling, arcade, and casual entertainment for groups and friends.",
            "Powerbowl 388 creates a destination for mall entertainment with bowling lanes, arcade zones, and combo offers for student groups and weekend traffic.",
            "/img/powerbowl/logo.png",
            "/img/powerbowl/out.jpg",
            "09:30 - 23:00",
            "Night Pick",
            "Arcade combo discounted today",
            ["Entertainment", "Floor 3", "Bowling"],
            [
                new("shop-powerbowl-product-1", "Bowling Ticket", "https://powerbowl.vn/wp-content/uploads/2020/12/127116810_1079374095844426_8373307559135062638_o.jpg", 70000m, null, null, true, false),
                new("shop-powerbowl-product-2", "Arcade Coin Combo", "https://media.timeout.com/images/105304123/image.jpg", 250000m, 300000m, 16, false, true)
            ],
            []
        ),
        new(
            "shop-vans",
            "Vans",
            "vans",
            "Streetwear",
            "Floor 3",
            "5-04",
            "Skate heritage footwear and street-ready apparel.",
            "Vans serves the streetwear crowd with classic silhouettes, backpacks, and seasonal apparel tied to skate culture and youth-oriented promotions.",
            "/img/vans/logo.jpg",
            "/img/vans/out.avif",
            "09:30 - 22:00",
            null,
            "10% off new arrivals",
            ["Streetwear", "Floor 3", "Sneakers"],
            [
                new("shop-vans-product-1", "Old Skool", "https://media.maisononline.vn/sys_master/images/h64/h5b/9239846387742/VN000D3HY28_01.jpg", 1750000m, null, null, true, false),
                new("shop-vans-product-2", "Slip-On Checkerboard", "https://media.maisononline.vn/sys_master/images/h81/hf1/9239845765150/VN000EYEBWW_01.jpg", 1450000m, null, null, true, false),
                new("shop-vans-product-3", "Old Skool Backpack", "https://media.maisononline.vn/sys_master/images/hab/hf6/9341499547678/VN0A5KHPY28_01.jpg", 850000m, 1100000m, 22, false, true)
            ],
            [new("shop-vans-voucher-1", "VANS10", "10% off new arrivals", "Applied to selected Knu Skool launches.", "30/06/2026")]
        ),
        new(
            "shop-converse",
            "Converse",
            "converse",
            "Streetwear",
            "Floor 3",
            "5-05",
            "Chuck classics and lifestyle staples for sneaker culture.",
            "Converse mixes timeless Chuck silhouettes with graphic tees and modern lifestyle drops for entry-level and collector sneaker shoppers alike.",
            "/img/converse/logo.webp",
            "/img/converse/out.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Streetwear", "Floor 3", "Sneakers"],
            [
                new("shop-converse-product-1", "Chuck Taylor Classic", "https://media.maisononline.vn/sys_master/images/hdf/hec/9253457199134/M9160C_01.jpg", 1500000m, null, null, true, false),
                new("shop-converse-product-2", "Chuck 70 Vintage Canvas", "https://media.maisononline.vn/sys_master/images/hf5/hc5/9280453308446/162058C_01.jpg", 2200000m, null, null, true, false),
                new("shop-converse-product-3", "Go-To Tee", "https://media.maisononline.vn/sys_master/images/h1d/hce/9385078554654/10023883-A01_01.jpg", 450000m, 650000m, 30, false, true)
            ],
            []
        ),
        new(
            "shop-sony-center",
            "Sony Center",
            "sony-center",
            "Technology",
            "Floor 4",
            "6-06",
            "Premium Sony retail with PlayStation, audio, and camera categories.",
            "Sony Center gives shoppers direct access to official PlayStation, Bravia, Alpha, and premium audio products with guided consultation inside the mall.",
            "/img/sonycenter/logo.jpeg",
            "/img/sonycenter/out.png",
            "09:30 - 22:00",
            "Premium Tech",
            "Buy PS5 and receive a bonus controller",
            ["Technology", "Floor 4", "Gaming"],
            [
                new("shop-sony-center-product-1", "PlayStation 5", "https://sonycenter.sony.com.vn/Data/Sites/1/Product/3729/ps5_01.png", 14990000m, null, null, true, false),
                new("shop-sony-center-product-2", "WH-1000XM5", "https://sonycenter.sony.com.vn/Data/Sites/1/Product/3932/wh-1000xm5_01.png", 7990000m, null, null, true, false),
                new("shop-sony-center-product-3", "SRS-XB13", "https://sonycenter.sony.com.vn/Data/Sites/1/Product/3602/srs-xb13_01.png", 990000m, 1490000m, 33, false, true)
            ],
            [new("shop-sony-center-voucher-1", "SONYPS5", "Bonus DualSense", "Receive an extra DualSense controller when buying PS5 disc version.", "31/05/2026")]
        ),
        new(
            "shop-lego",
            "LEGO",
            "lego",
            "Toys & Collectibles",
            "Floor 4",
            "6-02",
            "Official LEGO sets for kids, families, and collectors.",
            "The LEGO store offers Creator, Technic, and City collections in a colorful retail environment that encourages gift shopping and family discovery.",
            "/img/lego/logo.png",
            "/img/lego/out.webp",
            "09:30 - 22:00",
            null,
            null,
            ["Toys", "Floor 4", "Family"],
            [
                new("shop-lego-product-1", "Technic Bugatti Bolide", "https://product.hstatic.net/1000287106/product/42151_4a8d0b5e9f1947b18fc4f794b68e98a3_master.jpg", 1599000m, null, null, true, false),
                new("shop-lego-product-2", "Creator Orchid", "https://product.hstatic.net/1000287106/product/10311_1_25b04c868eb34421b9cfca784578b871_master.jpg", 1699000m, null, null, true, false),
                new("shop-lego-product-3", "City Prisoner Transport", "https://product.hstatic.net/1000287106/product/60312_1_c8b417e3f84f494fb2a3c75d3121d5a7_master.jpg", 599000m, 759000m, 21, false, true)
            ],
            []
        )

    ];

}
