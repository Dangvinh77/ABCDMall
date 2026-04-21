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
            var products = seed.Products.Length > 0 ? seed.Products : GetDefaultFeaturedProducts(seed);
            foreach (var product in products)
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

    private static ProductSeed[] GetDefaultFeaturedProducts(ShopSeed seed)
        => seed.Slug switch
        {
            "chuk" =>
            [
                Product(seed, 1, "Chuk Chuk Milk Tea", "https://product.hstatic.net/200000459373/product/tra-sua-tran-chau_9e6a7f6047f84413a2a12e875e4c7902_master.jpg", 45000m),
                Product(seed, 2, "Chuk Chuk Fruit Tea", "https://product.hstatic.net/200000459373/product/tra-trai-cay_061b998a73674473bd7ec7d0c0e6132b_master.jpg", 49000m),
                Product(seed, 3, "Chuk Chuk Coffee Combo", "https://product.hstatic.net/200000459373/product/ca-phe-sua-da_2e3b2a0d96534760bdb7d7fdc625c9e8_master.jpg", 39000m, 55000m, 29)
            ],
            "loc-phuc" =>
            [
                Product(seed, 1, "Gold Ring 24K", "https://cdn.pnj.io/images/detailed/191/gnxmxmw060903-nhan-vang-24k-pnj-1.png", 5200000m),
                Product(seed, 2, "Pearl Earrings", "https://cdn.pnj.io/images/detailed/181/gbxmxmy000617-bong-tai-vang-18k-dinh-ngoc-trai-pnj.png", 6800000m),
                Product(seed, 3, "Silver Bracelet", "https://cdn.pnj.io/images/detailed/177/glxmxmw000183-lac-tay-bac-dinh-da-pnjsilver.png", 890000m, 1200000m, 26)
            ],
            "dong-hai" =>
            [
                Product(seed, 1, "Leather Oxford Shoes", "https://product.hstatic.net/1000230642/product/gia_-y_ta_y_nam_da_that_dong_hai_g2515_den_1_7d77d55f340f48d6a26c51979dd3c844_master.jpg", 1890000m),
                Product(seed, 2, "Women Block Heels", "https://product.hstatic.net/1000230642/product/giay_cao_got_nu_dong_hai_g8171_den_1_2710f1763de947ec8895ccfe9dcf2f46_master.jpg", 1290000m),
                Product(seed, 3, "Leather Sandals", "https://product.hstatic.net/1000230642/product/sandal_nam_dong_hai_s1654_nau_1_c68c1393df064a62b8e7bbfcd8ce0367_master.jpg", 790000m, 1090000m, 28)
            ],
            "the-gioi-kim-cuong" =>
            [
                Product(seed, 1, "Diamond Solitaire Ring", "https://cdn.pnj.io/images/detailed/189/gnddddw004543-nhan-kim-cuong-vang-14k-pnj.png", 18500000m),
                Product(seed, 2, "Diamond Pendant Necklace", "https://cdn.pnj.io/images/detailed/180/gmdxddw000375-mat-day-chuyen-kim-cuong-vang-14k-pnj.png", 22500000m),
                Product(seed, 3, "Diamond Stud Earrings", "https://cdn.pnj.io/images/detailed/181/gbdd00w000011-bong-tai-kim-cuong-vang-14k-pnj.png", 12900000m, 15000000m, 14)
            ],
            "starbucks" =>
            [
                Product(seed, 1, "Caramel Macchiato", "https://globalassets.starbucks.com/digitalassets/products/bev/CaramelMacchiato.jpg", 95000m),
                Product(seed, 2, "Caffe Latte", "https://globalassets.starbucks.com/digitalassets/products/bev/CaffeLatte.jpg", 85000m),
                Product(seed, 3, "Mocha Frappuccino", "https://globalassets.starbucks.com/digitalassets/products/bev/MochaFrappuccino.jpg", 89000m, 109000m, 18)
            ],
            "highlands-coffee" =>
            [
                Product(seed, 1, "Phin Sua Da", "https://www.highlandscoffee.com.vn/vnt_upload/product/03_2018/PHIN_SUA_DA.png", 45000m),
                Product(seed, 2, "Freeze Tra Xanh", "https://www.highlandscoffee.com.vn/vnt_upload/product/04_2023/FREEZE_TRA_XANH.png", 65000m),
                Product(seed, 3, "Banh Mi Combo", "https://www.highlandscoffee.com.vn/vnt_upload/product/03_2018/BANH_MI_THIT_NUONG.png", 59000m, 79000m, 25)
            ],
            "public-bank" =>
            [
                Product(seed, 1, "Debit Card Service", "https://www.publicbank.com.vn/images/default-source/default-album/cards.jpg", 50000m),
                Product(seed, 2, "Savings Account Package", "https://www.publicbank.com.vn/images/default-source/default-album/deposit.jpg", 100000m),
                Product(seed, 3, "Online Banking Package", "https://www.publicbank.com.vn/images/default-source/default-album/pbebank.jpg", 80000m, 120000m, 33)
            ],
            "ecco" =>
            [
                Product(seed, 1, "ECCO Soft 7 Sneaker", "https://media.ecco.com/media/catalog/product/cache/2/image/1200x/9df78eab33525d08d6e5fb8d27136e95/4/3/43000301001_1.jpg", 4200000m),
                Product(seed, 2, "ECCO Biom C4 Golf", "https://media.ecco.com/media/catalog/product/cache/2/image/1200x/9df78eab33525d08d6e5fb8d27136e95/1/3/13040401007_1.jpg", 6200000m),
                Product(seed, 3, "ECCO Leather Belt", "https://media.ecco.com/media/catalog/product/cache/2/image/1200x/9df78eab33525d08d6e5fb8d27136e95/9/1/910577290000_1.jpg", 1590000m, 1990000m, 20)
            ],
            "futureworld" =>
            [
                Product(seed, 1, "iPhone 15 Pro", "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/iphone-15-pro-naturaltitanium-select?wid=940&hei=1112&fmt=png-alpha&.v=1692846357018", 28990000m),
                Product(seed, 2, "MacBook Air 13 M3", "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/mba13-m3-midnight-gallery1-202402?wid=4000&hei=3072&fmt=jpeg&qlt=90&.v=1707416810559", 27990000m),
                Product(seed, 3, "AirPods Pro 2", "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/MQD83?wid=1144&hei=1144&fmt=jpeg&qlt=90&.v=1660803972361", 5490000m, 6190000m, 11)
            ],
            "aldo" =>
            [
                Product(seed, 1, "Aldo Leather Loafers", "https://media.maisononline.vn/sys_master/images/h1a/h37/9239847108638/ALDO-MENS-LOAFERS-BLACK-01.jpg", 2590000m),
                Product(seed, 2, "Aldo Crossbody Bag", "https://media.maisononline.vn/sys_master/images/hbf/h10/9239846322206/ALDO-BAGS-BLACK-01.jpg", 1890000m),
                Product(seed, 3, "Aldo High Heels", "https://media.maisononline.vn/sys_master/images/h78/hc2/9239846125598/ALDO-WOMENS-HEELS-BEIGE-01.jpg", 1390000m, 1990000m, 30)
            ],
            "vascara" =>
            [
                Product(seed, 1, "Vascara Tote Bag", "https://product.hstatic.net/1000003969/product/tui_xach_tote_satchel_tot_0088_mau_den__1__2f3d2e67d7a34fc69979ed3f47d2c2db_master.jpg", 895000m),
                Product(seed, 2, "Vascara High Heels", "https://product.hstatic.net/1000003969/product/giay_cao_got_bmn_0523_mau_den__1__fbe38a3b1aa74a0185703ef4ecf76eb7_master.jpg", 745000m),
                Product(seed, 3, "Vascara Wallet", "https://product.hstatic.net/1000003969/product/vi_cam_tay_wal_0230_mau_be__1__1b2a2034ecdc49e1a6dc7a865c52788d_master.jpg", 395000m, 495000m, 20)
            ],
            "mujosh" =>
            [
                Product(seed, 1, "Mujosh Optical Frame", "https://product.hstatic.net/200000456445/product/mujosh_frame_black_1_8de5f0e94505408ca356a17ef1dcfd11_master.jpg", 1290000m),
                Product(seed, 2, "Mujosh Sunglasses", "https://product.hstatic.net/200000456445/product/mujosh_sunglasses_1_4da2e88e85c742e38d7b8a4dcf71239a_master.jpg", 1590000m),
                Product(seed, 3, "Mujosh Blue Light Glasses", "https://product.hstatic.net/200000456445/product/mujosh_bluelight_1_ea760f0fd38c4a0aa82f67d064e6cf31_master.jpg", 890000m, 1190000m, 25)
            ],
            "chagee" =>
            [
                Product(seed, 1, "Da Hong Pao Milk Tea", "https://chagee.com.my/wp-content/uploads/2023/12/Da-Hong-Pao-Milk-Tea.png", 65000m),
                Product(seed, 2, "Jasmine Green Milk Tea", "https://chagee.com.my/wp-content/uploads/2023/12/Jasmine-Green-Milk-Tea.png", 65000m),
                Product(seed, 3, "Peach Oolong Tea", "https://chagee.com.my/wp-content/uploads/2023/12/Peach-Oolong-Tea.png", 55000m, 69000m, 20)
            ],
            "elise" =>
            [
                Product(seed, 1, "Elise Office Dress", "https://product.hstatic.net/200000000133/product/dam_cong_so_elise_1_06e37cc8fc7f47939634243f59cc36f3_master.jpg", 1498000m),
                Product(seed, 2, "Elise Blazer", "https://product.hstatic.net/200000000133/product/ao_blazer_elise_1_ee840d4945d246969d9e3435a53aebf6_master.jpg", 1798000m),
                Product(seed, 3, "Elise Pleated Skirt", "https://product.hstatic.net/200000000133/product/chan_vay_elise_1_46b8e17df71842e0a93200abf0f3bb59_master.jpg", 799000m, 1098000m, 27)
            ],
            "trung-nguyen" =>
            [
                Product(seed, 1, "G7 3in1 Instant Coffee", "https://trungnguyenlegend.com/wp-content/uploads/2021/10/G7-3in1.png", 89000m),
                Product(seed, 2, "Legend Classic Coffee", "https://trungnguyenlegend.com/wp-content/uploads/2021/10/Legend-Classic.png", 159000m),
                Product(seed, 3, "Creative 5 Ground Coffee", "https://trungnguyenlegend.com/wp-content/uploads/2021/10/Sang-Tao-5.png", 119000m, 149000m, 20)
            ],
            "balabala" =>
            [
                Product(seed, 1, "Balabala Kids Hoodie", "https://cdn.shopify.com/s/files/1/0566/0839/1368/products/kids-hoodie.jpg", 699000m),
                Product(seed, 2, "Balabala Girls Dress", "https://cdn.shopify.com/s/files/1/0566/0839/1368/products/girls-dress.jpg", 799000m),
                Product(seed, 3, "Balabala Kids T-Shirt", "https://cdn.shopify.com/s/files/1/0566/0839/1368/products/kids-tshirt.jpg", 299000m, 399000m, 25)
            ],
            "baa-baby" =>
            [
                Product(seed, 1, "Baby Cotton Bodysuit", "https://product.hstatic.net/1000288768/product/body_so_sinh_1_b0d71c7f22a44fb093e8889ea2dfc963_master.jpg", 249000m),
                Product(seed, 2, "Baby Gift Set", "https://product.hstatic.net/1000288768/product/set_qua_tang_so_sinh_1_249d2e1cf34449f9a14f6c6aee0e0f1b_master.jpg", 699000m),
                Product(seed, 3, "Baby Blanket", "https://product.hstatic.net/1000288768/product/khan_chan_so_sinh_1_f47eaeeb35b64f389647196a88f8f87d_master.jpg", 349000m, 449000m, 22)
            ],
            "mochi-sweet" =>
            [
                Product(seed, 1, "Strawberry Mochi", "https://product.hstatic.net/200000420363/product/strawberry_mochi_1_518af4c3750446b7bf1ed9fb2ef66dcf_master.jpg", 39000m),
                Product(seed, 2, "Matcha Mochi Box", "https://product.hstatic.net/200000420363/product/matcha_mochi_1_3fdb9fa5ad0e4f90968d821f438ce57f_master.jpg", 159000m),
                Product(seed, 3, "Assorted Mochi Set", "https://product.hstatic.net/200000420363/product/assorted_mochi_1_f5042d3c9b194a0494b34306c2a4453f_master.jpg", 199000m, 249000m, 20)
            ],
            "ila" =>
            [
                Product(seed, 1, "English for Kids Course", "https://ila.edu.vn/wp-content/uploads/2023/08/ila-jumpstart.jpg", 3900000m),
                Product(seed, 2, "IELTS Foundation Course", "https://ila.edu.vn/wp-content/uploads/2023/08/ila-ielts.jpg", 6900000m),
                Product(seed, 3, "Trial Class Package", "https://ila.edu.vn/wp-content/uploads/2023/08/ila-smart-teens.jpg", 990000m, 1500000m, 34)
            ],
            "vitimex" =>
            [
                Product(seed, 1, "Vitimex Polo Shirt", "https://product.hstatic.net/1000284478/product/ao_polo_nam_1_1ee998cb336c466b9a2c8a1f894ec5d9_master.jpg", 459000m),
                Product(seed, 2, "Vitimex Casual Pants", "https://product.hstatic.net/1000284478/product/quan_kaki_nam_1_9c629f8803fc4a7bb09707d7585f25bf_master.jpg", 599000m),
                Product(seed, 3, "Vitimex Basic Tee", "https://product.hstatic.net/1000284478/product/ao_thun_nam_1_786e51cb39a6424c9b78e8d457c9cb08_master.jpg", 249000m, 349000m, 29)
            ],
            "belluni" =>
            [
                Product(seed, 1, "Belluni Oxford Shirt", "https://product.hstatic.net/1000284478/product/ao_so_mi_nam_belluni_1_160393e144b144cd85a52effccbc70ea_master.jpg", 790000m),
                Product(seed, 2, "Belluni Chino Pants", "https://product.hstatic.net/1000284478/product/quan_tay_nam_belluni_1_0b1f3d4a59dc436fb7e7d5f527e7b327_master.jpg", 890000m),
                Product(seed, 3, "Belluni Polo Shirt", "https://product.hstatic.net/1000284478/product/ao_polo_nam_belluni_1_d36e6cf4d4844a0597a2044b403e3c02_master.jpg", 490000m, 690000m, 29)
            ],
            "v-sixty-four" =>
            [
                Product(seed, 1, "V-Sixty Four Shirt", "https://product.hstatic.net/1000344185/product/ao_so_mi_v64_1_2e83fdb023a54272a7e8d582d4f74b26_master.jpg", 599000m),
                Product(seed, 2, "V-Sixty Four Jeans", "https://product.hstatic.net/1000344185/product/quan_jean_v64_1_d65f470349864706b43ea23ef7ed08f8_master.jpg", 799000m),
                Product(seed, 3, "V-Sixty Four T-Shirt", "https://product.hstatic.net/1000344185/product/ao_thun_v64_1_9330c7c97b124ed18f898aa9e6b1d717_master.jpg", 299000m, 399000m, 25)
            ],
            "insidemen" =>
            [
                Product(seed, 1, "Insidemen Linen Shirt", "https://product.hstatic.net/1000360022/product/ao_so_mi_linen_1_5a78cbff09bc430a9f7e292a417c5922_master.jpg", 690000m),
                Product(seed, 2, "Insidemen Slim Trousers", "https://product.hstatic.net/1000360022/product/quan_tay_nam_1_2a7a3547d15a40d6a12b93a7198de55e_master.jpg", 790000m),
                Product(seed, 3, "Insidemen Basic Polo", "https://product.hstatic.net/1000360022/product/ao_polo_nam_1_7632f4d347b845c2b3c841780ff6bf36_master.jpg", 390000m, 520000m, 25)
            ],
            _ => []
        };

    private static ProductSeed Product(
        ShopSeed seed,
        int index,
        string name,
        string imageUrl,
        decimal price,
        decimal? oldPrice = null,
        int? discountPercent = null)
        => new(
            $"{seed.Id}-product-{index}",
            name,
            imageUrl,
            price,
            oldPrice,
            discountPercent,
            IsFeatured: index <= 2,
            IsDiscounted: oldPrice.HasValue);

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
        ),
        new(
            "shop-chuk",
            "Chuk",
            "chuk",
            "Food & Beverage",
            "Floor 1",
            "1-03",
            "Chuk serves quick drinks and light bites for shoppers on the first floor.",
            "Chuk is a compact beverage stop inside ABCD Mall, designed for fast takeaway orders and casual breaks between shopping visits.",
            "/img/Chuk chuk/logo.jpg",
            "/img/Chuk chuk/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Food & Beverage", "Floor 1", "Drinks"],
            [],
            []
        ),
        new(
            "shop-loc-phuc",
            "Loc Phuc",
            "loc-phuc",
            "Jewelry",
            "Floor 1",
            "1-26",
            "Fine jewelry and gifting pieces near the premium retail zone.",
            "Loc Phuc offers jewelry, watches, and gift-ready accessories for shoppers looking for elegant pieces and milestone presents.",
            "/img/locphuc/logo.png",
            "/img/locphuc/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Jewelry", "Floor 1", "Premium"],
            [],
            []
        ),
        new(
            "shop-dong-hai",
            "Dong Hai",
            "dong-hai",
            "Footwear",
            "Floor 1",
            "1-23",
            "Vietnamese footwear with office, casual, and formal styles.",
            "Dong Hai brings leather shoes, sandals, and daily footwear collections for shoppers who need polished and reliable styles.",
            "/img/DongHai/logo.webp",
            "/img/DongHai/logo.webp",
            "09:30 - 22:00",
            null,
            null,
            ["Footwear", "Floor 1", "Leather"],
            [],
            []
        ),
        new(
            "shop-the-gioi-kim-cuong",
            "The Gioi Kim Cuong",
            "the-gioi-kim-cuong",
            "Jewelry",
            "Floor 1",
            "K1-04",
            "Diamond jewelry and premium gifting in a compact counter format.",
            "The Gioi Kim Cuong focuses on diamond rings, earrings, and fine jewelry consultation for special occasions.",
            "/img/Thế Giới Kim Cương/logo.jpg",
            "/img/Thế Giới Kim Cương/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Jewelry", "Floor 1", "Diamonds"],
            [],
            []
        ),
        new(
            "shop-starbucks",
            "Starbucks Coffee",
            "starbucks",
            "Cafe",
            "Floor 1",
            "1-09A",
            "Global coffeehouse serving handcrafted drinks and quick snacks.",
            "Starbucks Coffee offers espresso drinks, seasonal beverages, pastries, and a comfortable stop for shoppers inside ABCD Mall.",
            "/img/starbuck/logo.png",
            "/img/starbuck/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Cafe", "Floor 1", "Coffee"],
            [],
            []
        ),
        new(
            "shop-highlands-coffee",
            "Highlands Coffee",
            "highlands-coffee",
            "Cafe",
            "Floor 1",
            "1-03A",
            "Vietnamese coffee favorites and quick refreshments.",
            "Highlands Coffee serves signature phin coffee, freeze drinks, teas, and light snacks for shoppers looking for a familiar local cafe stop.",
            "/img/2lands/logo.png",
            "/img/2lands/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Cafe", "Floor 1", "Coffee"],
            [],
            []
        ),
        new(
            "shop-public-bank",
            "Public Bank",
            "public-bank",
            "Banking",
            "Floor 1",
            "1-10",
            "Banking support and financial services for mall visitors.",
            "Public Bank provides convenient in-mall banking services for customers and tenants who need quick financial assistance.",
            "/img/public bank/logo.png",
            "/img/public bank/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Banking", "Floor 1", "Services"],
            [],
            []
        ),
        new(
            "shop-ecco",
            "Ecco",
            "ecco",
            "Footwear",
            "Floor 1",
            "1-08A",
            "Comfort-focused shoes and leather goods for everyday wear.",
            "Ecco offers premium footwear and accessories with a focus on durable materials, comfort, and clean Scandinavian styling.",
            "/img/ecco/logo.png",
            "/img/ecco/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Footwear", "Floor 1", "Leather"],
            [],
            []
        ),
        new(
            "shop-futureworld",
            "Futureworld",
            "futureworld",
            "Technology",
            "Floor 1",
            "1-08B",
            "Apple-focused technology retail and accessories.",
            "Futureworld brings premium devices, accessories, and consultation for shoppers looking for technology products inside the mall.",
            "/img/futureword/logo.jpg",
            "/img/futureword/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Technology", "Floor 1", "Devices"],
            [],
            []
        ),
        new(
            "shop-aldo",
            "Aldo",
            "aldo",
            "Footwear & Accessories",
            "Floor 1",
            "1-03B",
            "Fashion footwear, bags, and accessories for modern shoppers.",
            "Aldo offers trend-led shoes, handbags, and accessories across formal, casual, and occasion-focused collections.",
            "/img/aldo/logo.jpg",
            "/img/aldo/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Footwear", "Floor 1", "Accessories"],
            [],
            []
        ),
        new(
            "shop-vascara",
            "Vascara",
            "vascara",
            "Bags & Accessories",
            "Floor 1",
            "1-05",
            "Women's shoes, handbags, and fashion accessories.",
            "Vascara provides handbags, heels, sandals, and accessories with accessible pricing and seasonal collections.",
            "/img/vascara/logo.png",
            "/img/vascara/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Accessories", "Floor 1", "Women"],
            [],
            []
        ),
        new(
            "shop-mujosh",
            "Mujosh",
            "mujosh",
            "Eyewear",
            "Floor 1",
            "1-09B",
            "Fashion eyewear, optical frames, and sunglasses.",
            "Mujosh offers stylish optical frames and sunglasses for shoppers seeking expressive eyewear and eye-care accessories.",
            "/img/mujosh/logo.jpg",
            "/img/mujosh/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Eyewear", "Floor 1", "Accessories"],
            [],
            []
        ),
        new(
            "shop-chagee",
            "Chagee",
            "chagee",
            "Tea & Beverage",
            "Floor 1",
            "1-20",
            "Modern tea drinks with a premium takeaway experience.",
            "Chagee serves brewed tea drinks, milk tea, and refreshing beverages for shoppers who want a quick premium drink stop.",
            "/img/Chagee/logo.png",
            "/img/Chagee/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Tea", "Floor 1", "Drinks"],
            [],
            []
        ),
        new(
            "shop-elise",
            "Elise",
            "elise",
            "Fashion & Lifestyle",
            "Floor 1",
            "1-09",
            "Women's fashion with polished office and occasion collections.",
            "Elise offers dresses, workwear, and seasonal fashion pieces for shoppers looking for refined feminine styling.",
            "/img/Elise/logo.png",
            "/img/Elise/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Fashion", "Floor 1", "Women"],
            [],
            []
        ),
        new(
            "shop-trung-nguyen",
            "Trung Nguyen Coffee",
            "trung-nguyen",
            "Cafe",
            "Floor 2",
            "3-01B",
            "Vietnamese coffee and cafe products for mall visitors.",
            "Trung Nguyen Coffee offers roasted coffee, signature drinks, and cafe seating for shoppers exploring the bookstore and lifestyle zone.",
            "/img/trungnguyen/logo.jpg",
            "/img/trungnguyen/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Cafe", "Floor 2", "Coffee"],
            [],
            []
        ),
        new(
            "shop-balabala",
            "BalaBala",
            "balabala",
            "Kids Fashion",
            "Floor 2",
            "3-30",
            "Children's fashion with playful everyday collections.",
            "BalaBala provides apparel and accessories for children, focusing on comfortable fabrics and colorful seasonal designs.",
            "/img/BalaBala/logo.jpg",
            "/img/BalaBala/logo.jpg",
            "09:30 - 22:00",
            null,
            null,
            ["Kids", "Floor 2", "Family"],
            [],
            []
        ),
        new(
            "shop-baa-baby",
            "Baa Baby",
            "baa-baby",
            "Baby & Kids",
            "Floor 2",
            "K3-03",
            "Baby products, gifts, and essentials for young families.",
            "Baa Baby serves parents with baby essentials, children's accessories, and family-friendly gift items.",
            "/img/Baa Baby/logo.png",
            "/img/Baa Baby/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Baby", "Floor 2", "Family"],
            [],
            []
        ),
        new(
            "shop-mochi-sweet",
            "Mochi Sweet",
            "mochi-sweet",
            "Dessert",
            "Floor 2",
            "3-16",
            "Japanese-inspired mochi desserts and sweet treats.",
            "Mochi Sweet offers soft mochi, dessert boxes, and quick sweet treats for shoppers passing through the second floor.",
            "/img/Mochi Sweet/logo.webp",
            "/img/Mochi Sweet/logo.webp",
            "09:30 - 22:00",
            null,
            null,
            ["Dessert", "Floor 2", "Sweets"],
            [],
            []
        ),
        new(
            "shop-ila",
            "ILA",
            "ila",
            "Education",
            "Floor 2",
            "3-11",
            "English learning and education consultation for families.",
            "ILA provides language-learning consultation and education services for parents and students inside the mall.",
            "/img/ILA/logo.webp",
            "/img/ILA/logo.webp",
            "09:30 - 22:00",
            null,
            null,
            ["Education", "Floor 2", "Learning"],
            [],
            []
        ),
        new(
            "shop-vitimex",
            "Vitimex",
            "vitimex",
            "Fashion & Lifestyle",
            "Floor 2",
            "3-10A",
            "Casual fashion and wardrobe essentials.",
            "Vitimex offers practical apparel and accessories for shoppers looking for simple everyday pieces.",
            "/img/Vitimex/logo.png",
            "/img/Vitimex/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Fashion", "Floor 2", "Casual"],
            [],
            []
        ),
        new(
            "shop-belluni",
            "Belluni",
            "belluni",
            "Menswear",
            "Floor 2",
            "3-10",
            "Menswear staples for office and daily dressing.",
            "Belluni focuses on shirts, trousers, and smart casual items for male shoppers who prefer practical and polished outfits.",
            "/img/Belluni/logo.png",
            "/img/Belluni/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Menswear", "Floor 2", "Smart Casual"],
            [],
            []
        ),
        new(
            "shop-v-sixty-four",
            "V-Sixty Four",
            "v-sixty-four",
            "Fashion & Lifestyle",
            "Floor 2",
            "3-02",
            "Contemporary fashion for young shoppers.",
            "V-Sixty Four brings casual apparel and trend-led fashion pieces into the second-floor lifestyle zone.",
            "/img/V-Sixty Four/logo.png",
            "/img/V-Sixty Four/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Fashion", "Floor 2", "Casual"],
            [],
            []
        ),
        new(
            "shop-insidemen",
            "Insidemen",
            "insidemen",
            "Menswear",
            "Floor 2",
            "3-02",
            "Modern menswear with casual and smart styling.",
            "Insidemen offers shirts, tees, trousers, and accessories for male shoppers looking for approachable modern outfits.",
            "/img/Insidemen/logo.png",
            "/img/Insidemen/logo.png",
            "09:30 - 22:00",
            null,
            null,
            ["Menswear", "Floor 2", "Fashion"],
            [],
            []
        )

    ];

}
