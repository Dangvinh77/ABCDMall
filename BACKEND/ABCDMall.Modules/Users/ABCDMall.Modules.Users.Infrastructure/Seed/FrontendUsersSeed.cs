using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ABCDMall.Modules.Users.Infrastructure.Seed;

public static class FrontendUsersSeed
{
    public static async Task SeedAsync(MallDbContext db, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await SeedUsersAndShopsAsync(db, now, ct);
        await SeedRentalAreasAsync(db, now, ct);
        await SeedMonthlyBillsAsync(db, now, ct);
        await SeedProfileUpdateHistoriesAsync(db, ct);
    }

    private static async Task SeedUsersAndShopsAsync(MallDbContext db, DateTime now, CancellationToken ct)
    {
        var shops = SeedData.Shops.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
        var allShopSeeds = SeedData.Shops.Concat(SeedData.PublicCatalogShops).ToArray();
        var existingShops = await db.ShopInfos
            .Where(x => allShopSeeds.Select(seed => seed.Id).Contains(x.Id!))
            .ToListAsync(ct);

        foreach (var seed in allShopSeeds)
        {
            var shop = existingShops.FirstOrDefault(x => x.Id == seed.Id);
            if (shop is null)
            {
                shop = new ShopInfo { Id = seed.Id };
                await db.ShopInfos.AddAsync(shop, ct);
                existingShops.Add(shop);
            }

            shop.ShopName = seed.ShopName;
            shop.Slug = string.IsNullOrWhiteSpace(seed.Slug) ? GenerateSlug(seed.ShopName) : seed.Slug;
            shop.Category = seed.Category ?? "Retail Store";
            shop.Floor = seed.Floor ?? "Mall floor";
            shop.LocationSlot = seed.RentalLocation;
            shop.Summary = seed.Summary ?? BuildShopSummary(seed.ShopName, seed.RentalLocation, seed.Category);
            shop.Description = seed.Description ?? BuildShopDescription(seed.ShopName, seed.ManagerName, seed.RentalLocation, seed.Category);
            shop.LogoUrl = seed.LogoUrl ?? seed.ContractImage ?? string.Empty;
            shop.CoverImageUrl = seed.CoverImageUrl ?? seed.ContractImage ?? string.Empty;
            shop.OpenHours = seed.OpenHours ?? "09:30 - 22:00";
            shop.Badge = seed.Badge ?? "Featured Store";
            shop.Offer = seed.Offer ?? (seed.TotalDue > 0 ? $"Estimated monthly due: {seed.TotalDue:N0}" : null);
            shop.Tags = seed.Tags ?? BuildShopTags(seed.RentalLocation, seed.Category, seed.Floor);
            shop.IsPublicVisible = true;
            shop.ManagerName = seed.ManagerName;
            shop.CCCD = seed.Cccd;
            shop.RentalLocation = seed.RentalLocation;
            shop.Month = seed.Month;
            shop.LeaseStartDate = seed.LeaseStartDate;
            shop.ElectricityUsage = seed.ElectricityUsage;
            shop.ElectricityFee = seed.ElectricityFee;
            shop.WaterUsage = seed.WaterUsage;
            shop.WaterFee = seed.WaterFee;
            shop.ServiceFee = seed.ServiceFee;
            shop.LeaseTermDays = seed.LeaseTermDays;
            shop.TotalDue = seed.TotalDue;
            shop.ContractImage = seed.ContractImage;
            shop.ContractImages = seed.ContractImage;
            shop.CreatedAt = seed.CreatedAt;
        }

        var existingUsers = await db.Users
            .Where(x => SeedData.Users.Select(seed => seed.Email).Contains(x.Email))
            .ToListAsync(ct);

        foreach (var seed in SeedData.Users)
        {
            var user = existingUsers.FirstOrDefault(x => string.Equals(x.Email, seed.Email, StringComparison.OrdinalIgnoreCase));
            if (user is null)
            {
                user = new User { Id = seed.Id };
                await db.Users.AddAsync(user, ct);
                existingUsers.Add(user);
            }

            user.Email = seed.Email;
            if (string.IsNullOrWhiteSpace(user.Password) || !BCrypt.Net.BCrypt.Verify(seed.Password, user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(seed.Password);
            }

            user.Role = seed.Role;
            user.FullName = seed.FullName;
            user.ShopId = seed.ShopId;
            user.Image = seed.Image;
            user.Address = seed.Address;
            user.CCCD = seed.Cccd;
            user.FailedLoginAttempts = 0;
            user.LoginOtpCode = null;
            user.LoginOtpExpiresAt = null;
            user.CreatedAt ??= seed.CreatedAt;
            user.UpdatedAt = now;

            if (!string.IsNullOrWhiteSpace(seed.ShopId) && shops.TryGetValue(seed.ShopId, out var shopSeed))
            {
                var shop = existingShops.First(x => x.Id == shopSeed.Id);
                shop.ManagerName = seed.FullName;
                shop.CCCD = seed.Cccd;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedRentalAreasAsync(MallDbContext db, DateTime now, CancellationToken ct)
    {
        var existingAreas = await db.RentalAreas
            .Where(x => SeedData.RentalAreas.Select(seed => seed.AreaCode).Contains(x.AreaCode))
            .ToListAsync(ct);

        foreach (var seed in SeedData.RentalAreas)
        {
            var rentalArea = existingAreas.FirstOrDefault(x => string.Equals(x.AreaCode, seed.AreaCode, StringComparison.OrdinalIgnoreCase));
            if (rentalArea is null)
            {
                rentalArea = new RentalArea { Id = seed.Id };
                await db.RentalAreas.AddAsync(rentalArea, ct);
                existingAreas.Add(rentalArea);
            }

            rentalArea.AreaCode = seed.AreaCode;
            rentalArea.Floor = seed.Floor;
            rentalArea.AreaName = seed.AreaName;
            rentalArea.Size = seed.Size;
            rentalArea.MonthlyRent = seed.MonthlyRent;
            rentalArea.Status = seed.Status;
            rentalArea.TenantName = seed.TenantName;
            rentalArea.ShopInfoId = seed.ShopInfoId;
            rentalArea.CreatedAt = seed.CreatedAt == default ? now : seed.CreatedAt;
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedMonthlyBillsAsync(MallDbContext db, DateTime now, CancellationToken ct)
    {
        var existingBills = await db.ShopMonthlyBills
            .Where(x => SeedData.MonthlyBills.Select(seed => seed.BillKey).Contains(x.BillKey))
            .ToListAsync(ct);

        foreach (var seed in SeedData.MonthlyBills)
        {
            var bill = existingBills.FirstOrDefault(x => string.Equals(x.BillKey, seed.BillKey, StringComparison.OrdinalIgnoreCase));
            if (bill is null)
            {
                bill = new ShopMonthlyBill { Id = seed.Id };
                await db.ShopMonthlyBills.AddAsync(bill, ct);
                existingBills.Add(bill);
            }

            bill.ShopInfoId = seed.ShopInfoId;
            bill.BillKey = seed.BillKey;
            bill.ShopName = seed.ShopName;
            bill.ManagerName = seed.ManagerName;
            bill.CCCD = seed.Cccd;
            bill.RentalLocation = seed.RentalLocation;
            bill.Month = seed.Month;
            bill.UsageMonth = seed.UsageMonth;
            bill.BillingMonthKey = seed.BillingMonthKey;
            bill.UsageMonthKey = seed.UsageMonthKey;
            bill.LeaseStartDate = seed.LeaseStartDate;
            bill.ElectricityUsage = seed.ElectricityUsage;
            bill.ElectricityFee = seed.ElectricityFee;
            bill.WaterUsage = seed.WaterUsage;
            bill.WaterFee = seed.WaterFee;
            bill.ServiceFee = seed.ServiceFee;
            bill.LeaseTermDays = seed.LeaseTermDays;
            bill.TotalDue = seed.TotalDue;
            bill.ContractImage = seed.ContractImage;
            bill.ContractImages = seed.ContractImage;
            bill.CreatedAt = seed.CreatedAt;
            bill.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedProfileUpdateHistoriesAsync(MallDbContext db, CancellationToken ct)
    {
        var existingHistoryIds = await db.ProfileUpdateHistories
            .Where(x => SeedData.ProfileUpdateHistories.Select(seed => seed.Id).Contains(x.Id!))
            .Select(x => x.Id!)
            .ToListAsync(ct);

        foreach (var seed in SeedData.ProfileUpdateHistories.Where(x => !existingHistoryIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase)))
        {
            await db.ProfileUpdateHistories.AddAsync(new ProfileUpdateHistory
            {
                Id = seed.Id,
                UserId = seed.UserId,
                Email = seed.Email,
                PreviousFullName = seed.PreviousFullName,
                PreviousAddress = seed.PreviousAddress,
                PreviousImage = seed.PreviousImage,
                PreviousCCCD = seed.PreviousCccd,
                UpdatedFullName = seed.UpdatedFullName,
                UpdatedAddress = seed.UpdatedAddress,
                UpdatedImage = seed.UpdatedImage,
                UpdatedCCCD = seed.UpdatedCccd,
                UpdatedAt = seed.UpdatedAt
            }, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static string BuildShopSummary(string shopName, string rentalLocation, string? category)
    {
        var safeCategory = string.IsNullOrWhiteSpace(category) ? "shop" : category;
        return string.IsNullOrWhiteSpace(rentalLocation)
            ? $"{shopName} is one of the featured {safeCategory.ToLowerInvariant()} destinations at ABCD Mall."
            : $"{shopName} is operating at {rentalLocation} in ABCD Mall.";
    }

    private static string BuildShopDescription(string shopName, string? managerName, string rentalLocation, string? category)
    {
        if (!string.IsNullOrWhiteSpace(managerName) && !string.IsNullOrWhiteSpace(rentalLocation))
        {
            return $"{shopName} is managed by {managerName} and located at {rentalLocation}.";
        }

        var safeCategory = string.IsNullOrWhiteSpace(category) ? "shopping" : category.ToLowerInvariant();
        return string.IsNullOrWhiteSpace(rentalLocation)
            ? $"{shopName} is available in the ABCD Mall public catalog for visitors exploring {safeCategory} brands."
            : $"{shopName} is available in the ABCD Mall public catalog and can be found at {rentalLocation}.";
    }

    private static string BuildShopTags(string rentalLocation, string? category, string? floor)
    {
        var tags = new List<string> { "ABCD Mall" };

        if (!string.IsNullOrWhiteSpace(category))
        {
            tags.Add(category);
        }

        if (!string.IsNullOrWhiteSpace(floor))
        {
            tags.Add(floor);
        }

        if (!string.IsNullOrWhiteSpace(rentalLocation))
        {
            tags.Add(rentalLocation);
        }

        return string.Join(", ", tags.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string GenerateSlug(string value)
        => string.Join(
            "-",
            value
                .Trim()
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => new string(part.Where(char.IsLetterOrDigit).ToArray()))
                .Where(part => part.Length > 0));

    private static class SeedData
    {
        private static readonly ManagedRentalAssignment[] ManagedRentalAssignments =
        [
            new(1, "1-01", "/shops/uniqlo"),
            new(2, "1-11", "/shops/adidas"),
            new(3, "1-02", "/shops/levis"),
            new(4, "1-03", "/shops/chuk"),
            new(5, "SB-01", "/shops/beauty-box"),
            new(6, "1-27", "/shops/pnj"),
            new(7, "1-26", "/shops/loc-phuc"),
            new(8, "1-23", "/shops/dong-hai"),
            new(9, "K1-04", "/shops/the-gioi-kim-cuong"),
            new(10, "1-09A", "/shops/starbucks"),
            new(11, "1-03A", "/shops/highlands-coffee"),
            new(12, "1-10", "/shops/public-bank"),
            new(13, "1-09", "/shops/pedro"),
            new(14, "1-08A", "/shops/ecco"),
            new(15, "1-08B", "/shops/futureworld"),
            new(16, "1-07", "/shops/nike"),
            new(17, "1-06", "/shops/charles-keith"),
            new(18, "1-03B", "/shops/aldo"),
            new(19, "1-05", "/shops/vascara"),
            new(20, "1-09B", "/shops/mujosh"),
            new(21, "1-20", "/shops/chagee"),
            new(23, "K1-11", "/shops/casio"),
            new(24, "3-01", "/shops/phuong-nam"),
            new(25, "3-01B", "/shops/trung-nguyen"),
            new(26, "3-31", "/shops/ninomaxx"),
            new(27, "3-03A", "/shops/miniso"),
            new(28, "3-03", "/shops/rabity"),
            new(29, "3-30", "/shops/balabala"),
            new(30, "K3-03", "/shops/baa-baby"),
            new(31, "3-15", "/shops/pop-mart")
        ];

        public static readonly UserSeed[] Users =
            BuildUsers();

        public static readonly ShopSeed[] Shops =
            BuildManagedShops();

        public static readonly ShopSeed[] PublicCatalogShops =
        [
            PublicCatalogShop("public-shop-001", "Uniqlo", "uniqlo", "1-01", "/img/uniqlo/logo.png", "Fashion", "Level 1"),
            PublicCatalogShop("public-shop-002", "Adidas", "adidas", "1-11", "/img/adidas/logo.png", "Sportswear", "Level 1"),
            PublicCatalogShop("public-shop-003", "Levi's", "levis", "1-02", "/img/levi/logo.png", "Denim", "Level 1"),
            PublicCatalogShop("public-shop-004", "Chuk", "chuk", "1-03", "/img/Chuk chuk/logo.jpg", "Tea", "Level 1"),
            PublicCatalogShop("public-shop-005", "Beauty Box", "beauty-box", "SB-01", "/img/beautybox/logo.png", "Beauty", "Level 1"),
            PublicCatalogShop("public-shop-006", "PNJ", "pnj", "1-27", "/img/pnj/logo.jpg", "Jewelry", "Level 1"),
            PublicCatalogShop("public-shop-007", "Loc Phuc", "loc-phuc", "1-26", "/img/locphuc/logo.png", "Jewelry", "Level 1"),
            PublicCatalogShop("public-shop-008", "Dong Hai", "dong-hai", "1-23", "/img/DongHai/logo.webp", "Footwear", "Level 1"),
            PublicCatalogShop("public-shop-009", "The Gioi Kim Cuong", "the-gioi-kim-cuong", "K1-04", "/img/Thế Giới Kim Cương/logo.jpg", "Jewelry", "Level 1"),
            PublicCatalogShop("public-shop-010", "Starbucks Coffee", "starbucks", "1-09A", "/img/starbuck/logo.png", "Coffee", "Level 1"),
            PublicCatalogShop("public-shop-011", "Highlands Coffee", "highlands-coffee", "1-03A", "/img/2lands/logo.png", "Coffee", "Level 1"),
            PublicCatalogShop("public-shop-012", "Public Bank", "public-bank", "1-10", "/img/public bank/logo.png", "Banking", "Level 1"),
            PublicCatalogShop("public-shop-013", "Pedro", "pedro", "1-09", "/img/pedro/logo.png", "Footwear", "Level 1"),
            PublicCatalogShop("public-shop-014", "Ecco", "ecco", "1-08A", "/img/ecco/logo.png", "Footwear", "Level 1"),
            PublicCatalogShop("public-shop-015", "Futureworld", "futureworld", "1-08B", "/img/futureword/logo.jpg", "Technology", "Level 1"),
            PublicCatalogShop("public-shop-016", "Nike", "nike", "1-07", "/img/nike/logo.webp", "Sportswear", "Level 1"),
            PublicCatalogShop("public-shop-017", "Charles and Keith", "charles-keith", "1-06", "/img/C&K/logo.jpg", "Fashion Accessories", "Level 1"),
            PublicCatalogShop("public-shop-018", "Aldo", "aldo", "1-03B", "/img/aldo/logo.jpg", "Footwear", "Level 1"),
            PublicCatalogShop("public-shop-019", "Vascara", "vascara", "1-05", "/img/vascara/logo.png", "Fashion Accessories", "Level 1"),
            PublicCatalogShop("public-shop-020", "Mujosh", "mujosh", "1-09B", "/img/mujosh/logo.jpg", "Eyewear", "Level 1"),
            PublicCatalogShop("public-shop-021", "Chagee", "chagee", "1-20", "/img/Chagee/logo.png", "Tea", "Level 1"),
            PublicCatalogShop("public-shop-022", "Elise", "elise", "1-09", "/img/Elise/logo.png", "Fashion", "Level 1"),
            PublicCatalogShop("public-shop-023", "Casio", "casio", "K1-11", "/img/casio/logo.png", "Watches", "Level 1"),
            PublicCatalogShop("public-shop-024", "Phuong Nam Book City", "phuong-nam", "3-01", "/img/phuongnam/logo.jpg", "Books", "Level 2"),
            PublicCatalogShop("public-shop-025", "Trung Nguyen Coffee", "trung-nguyen", "3-01B", "/img/trungnguyen/logo.jpg", "Coffee", "Level 2"),
            PublicCatalogShop("public-shop-026", "Ninomaxx", "ninomaxx", "3-31", "/img/NINOMAXX/logo.png", "Fashion", "Level 2"),
            PublicCatalogShop("public-shop-027", "Miniso", "miniso", "3-03A", "/img/miniso/logo.jpg", "Lifestyle", "Level 2"),
            PublicCatalogShop("public-shop-028", "Rabity", "rabity", "3-03", "/img/rabity/logo.jpg", "Kids Fashion", "Level 2"),
            PublicCatalogShop("public-shop-029", "BalaBala", "balabala", "3-30", "/img/BalaBala/logo.jpg", "Kids Fashion", "Level 2"),
            PublicCatalogShop("public-shop-030", "Baa Baby", "baa-baby", "K3-03", "/img/Baa Baby/logo.png", "Kids Fashion", "Level 2"),
            PublicCatalogShop("public-shop-031", "Pop Mart", "pop-mart", "3-15", "/img/popmart/logo.jpg", "Collectibles", "Level 2"),
            PublicCatalogShop("public-shop-032", "Mochi Sweet", "mochi-sweet", "3-16", "/img/Mochi Sweet/logo.webp", "Desserts", "Level 2"),
            PublicCatalogShop("public-shop-033", "ILA", "ila", "3-11", "/img/ILA/logo.webp", "Education", "Level 2"),
            PublicCatalogShop("public-shop-034", "Lug.vn", "lugvn", "3-12", "/img/lug/logo.png", "Travel Gear", "Level 2"),
            PublicCatalogShop("public-shop-035", "Vitimex", "vitimex", "3-10A", "/img/Vitimex/logo.png", "Fashion", "Level 2"),
            PublicCatalogShop("public-shop-036", "Belluni", "belluni", "3-10", "/img/Belluni/logo.png", "Fashion", "Level 2"),
            PublicCatalogShop("public-shop-037", "John Henry", "john-henry", "3-09", "/img/johnhenry/logo.png", "Fashion", "Level 2"),
            PublicCatalogShop("public-shop-038", "Boo", "boo", "3-08", "/img/boo/logo.png", "Streetwear", "Level 2"),
            PublicCatalogShop("public-shop-039", "Levents", "levents", "3-07", "/img/levents/logo.png", "Streetwear", "Level 2"),
            PublicCatalogShop("public-shop-040", "V-Sixty Four", "v-sixty-four", "3-02", "/img/V-Sixty Four/logo.png", "Fashion", "Level 2"),
            PublicCatalogShop("public-shop-041", "Insidemen", "insidemen", "3-02", "/img/Insidemen/logo.png", "Fashion", "Level 2"),
            PublicCatalogShop("public-shop-042", "Powerbowl 388", "powerbowl", "5-01", "/img/powerbowl/logo.png", "Recreation", "Level 3")
        ];

        private static ShopSeed PublicCatalogShop(
            string id,
            string shopName,
            string slug,
            string rentalLocation,
            string? imageUrl,
            string category,
            string floor)
            => new(
                Id: id,
                ShopName: shopName,
                ManagerName: null,
                Cccd: null,
                RentalLocation: rentalLocation,
                Month: string.Empty,
                LeaseStartDate: new DateTime(2026, 4, 21, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: string.Empty,
                ElectricityFee: 0m,
                WaterUsage: string.Empty,
                WaterFee: 0m,
                ServiceFee: 0m,
                LeaseTermDays: 0,
                TotalDue: 0m,
                ContractImage: imageUrl,
                CreatedAt: new DateTime(2026, 4, 21, 0, 0, 0, DateTimeKind.Utc),
                Slug: slug,
                Category: category,
                Floor: floor,
                Summary: $"{shopName} is featured in the ABCD Mall public directory at {rentalLocation}.",
                Description: $"{shopName} is listed in the ABCD Mall public shop catalog for visitors exploring {category.ToLowerInvariant()} brands.",
                LogoUrl: imageUrl,
                CoverImageUrl: imageUrl,
                OpenHours: "09:30 - 22:00",
                Badge: "Mall Favorite",
                Offer: null,
                Tags: $"ABCD Mall, {category}, {floor}, {rentalLocation}");

        public static readonly RentalAreaSeed[] RentalAreas =
            BuildRentalAreas();

        public static readonly MonthlyBillSeed[] MonthlyBills =
            BuildMonthlyBills();

        public static readonly ProfileUpdateHistorySeed[] ProfileUpdateHistories =
        [
            new(
                "profile-history-001", "users-manager-001", "manager1@abcdmall.local",
                "Nguyen Minh", "District 3, Ho Chi Minh City", "/images/profiles/manager-1-old.png", "079203000101",
                "Nguyen Van Minh", "District 1, Ho Chi Minh City", "/images/profiles/manager-1.png", "079203000111",
                new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc)),
            new(
                "profile-history-002", "users-manager-002", "manager2@abcdmall.local",
                "Tran Lan", "Binh Thanh, Ho Chi Minh City", null, "079203000202",
                "Tran Thi Lan", "Thu Duc, Ho Chi Minh City", "/images/profiles/manager-2.png", "079203000222",
                new DateTime(2026, 4, 12, 9, 30, 0, DateTimeKind.Utc)),
            new(
                "profile-history-003", "users-manager-003", "manager3@abcdmall.local",
                "Pham Huy", "Tan Binh, Ho Chi Minh City", null, "079203000303",
                "Pham Gia Huy", "Go Vap, Ho Chi Minh City", "/images/profiles/manager-3.png", "079203000333",
                new DateTime(2026, 4, 15, 10, 15, 0, DateTimeKind.Utc))
        ];

        private static UserSeed[] BuildUsers()
        {
            var users = new List<UserSeed>
            {
                new(
                    Id: "users-admin-001",
                    Email: "admin@abcdmall.local",
                    Password: "Admin@123",
                    Role: "Admin",
                    FullName: "Mall Administrator",
                    ShopId: null,
                    Image: "/images/profiles/admin-default.png",
                    Address: "ABCD Mall HQ",
                    Cccd: "000000000001",
                    CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc))
            };

            for (var index = 1; index <= ManagedRentalAssignments.Length; index++)
            {
                var profile = GetManagerProfile(index);
                users.Add(new UserSeed(
                    Id: $"users-manager-{index:000}",
                    Email: $"manager{index}@abcdmall.local",
                    Password: "Manager@123",
                    Role: "Manager",
                    FullName: profile.FullName,
                    ShopId: $"shop-{index:000}",
                    Image: profile.Image,
                    Address: profile.Address,
                    Cccd: profile.Cccd,
                    CreatedAt: new DateTime(2026, 4, Math.Min(index + 1, 28), 0, 0, 0, DateTimeKind.Utc)));
            }

            for (var index = 31; index <= 80; index++)
            {
                var profile = GetUnassignedManagerProfile(index);
                users.Add(new UserSeed(
                    Id: $"users-manager-{index:000}",
                    Email: $"manager{index}@abcdmall.local",
                    Password: "Manager@123",
                    Role: "Manager",
                    FullName: profile.FullName,
                    ShopId: null,
                    Image: profile.Image,
                    Address: profile.Address,
                    Cccd: profile.Cccd,
                    CreatedAt: new DateTime(2026, 5, ((index - 31) % 28) + 1, 0, 0, 0, DateTimeKind.Utc)));
            }

            users.AddRange(
            [
                new(
                    Id: "users-movies-admin-001",
                    Email: "movies.admin1@abcdmall.local",
                    Password: "MoviesAdmin@123",
                    Role: "MoviesAdmin",
                    FullName: "Movies Admin One",
                    ShopId: null,
                    Image: "/images/profiles/admin-default.png",
                    Address: "Cinema Operations Desk",
                    Cccd: "000000100001",
                    CreatedAt: new DateTime(2026, 4, 7, 0, 0, 0, DateTimeKind.Utc)),
                new(
                    Id: "users-movies-admin-002",
                    Email: "movies.admin2@abcdmall.local",
                    Password: "MoviesAdmin@123",
                    Role: "MoviesAdmin",
                    FullName: "Movies Admin Two",
                    ShopId: null,
                    Image: "/images/profiles/admin-default.png",
                    Address: "Cinema Operations Desk",
                    Cccd: "000000100002",
                    CreatedAt: new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc)),
                new(
                    Id: "users-movies-admin-003",
                    Email: "movies.admin3@abcdmall.local",
                    Password: "MoviesAdmin@123",
                    Role: "MoviesAdmin",
                    FullName: "Movies Admin Three",
                    ShopId: null,
                    Image: "/images/profiles/admin-default.png",
                    Address: "Cinema Operations Desk",
                    Cccd: "000000100003",
                    CreatedAt: new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc))
            ]);

            return users.ToArray();
        }

        private static ShopSeed[] BuildManagedShops()
            => ManagedRentalAssignments
                .Select((assignment, zeroBasedIndex) => CreateManagedShopSeed(zeroBasedIndex + 1, assignment))
                .ToArray();

        private static RentalAreaSeed[] BuildRentalAreas()
            => ManagedRentalAssignments
                .Select((assignment, zeroBasedIndex) =>
                {
                    var index = zeroBasedIndex + 1;
                    var profile = GetManagerProfile(index);
                    var leaseStartDate = GetLeaseStartDate(index);

                    return new RentalAreaSeed(
                        Id: assignment.MapLocationId.ToString(CultureInfo.InvariantCulture),
                        AreaCode: assignment.RentalLocation,
                        Floor: GetLegacyFloorLabel(assignment.RentalLocation),
                        AreaName: $"Managed retail area {index:00}",
                        Size: $"{24 + (index % 12)}m2",
                        MonthlyRent: 18000000m + (index * 350000m),
                        Status: "Rented",
                        TenantName: profile.ShopName,
                        ShopInfoId: $"shop-{index:000}",
                        CreatedAt: leaseStartDate);
                })
                .ToArray();

        private static MonthlyBillSeed[] BuildMonthlyBills()
            => ManagedRentalAssignments
                .Select((assignment, zeroBasedIndex) => CreateMonthlyBillSeed(zeroBasedIndex + 1, assignment))
                .ToArray();

        private static ShopSeed CreateManagedShopSeed(int index, ManagedRentalAssignment assignment)
        {
            var profile = GetManagerProfile(index);
            var leaseStartDate = GetLeaseStartDate(index);
            var electricityRate = 3400m + ((index - 1) % 4 * 100m);
            var electricityUnits = 90 + (index * 3);
            var waterUnits = 10 + ((index - 1) % 8);
            var serviceFee = 600000m + (index * 25000m);
            var totalDue = (electricityUnits * electricityRate) + (waterUnits * 15000m) + serviceFee;

            return new ShopSeed(
                Id: $"shop-{index:000}",
                ShopName: profile.ShopName,
                ManagerName: profile.FullName,
                Cccd: profile.Cccd,
                RentalLocation: assignment.RentalLocation,
                Month: leaseStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                LeaseStartDate: leaseStartDate,
                ElectricityUsage: $"{electricityUnits} kWh",
                ElectricityFee: electricityRate,
                WaterUsage: $"{waterUnits} m3",
                WaterFee: 15000m,
                ServiceFee: serviceFee,
                LeaseTermDays: 180 + (((index - 1) % 4) * 90),
                TotalDue: totalDue,
                ContractImage: $"/images/contracts/managed-shop-{index:000}.png",
                CreatedAt: leaseStartDate);
        }

        private static MonthlyBillSeed CreateMonthlyBillSeed(int index, ManagedRentalAssignment assignment)
        {
            var shop = CreateManagedShopSeed(index, assignment);
            var billingMonth = shop.LeaseStartDate.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var usageMonth = shop.LeaseStartDate.AddMonths(-1).ToString("yyyy-MM", CultureInfo.InvariantCulture);

            return new MonthlyBillSeed(
                Id: $"monthly-bill-{index:000}",
                ShopInfoId: shop.Id,
                BillKey: $"{shop.Id}:{billingMonth}:{usageMonth}:seed",
                ShopName: shop.ShopName,
                ManagerName: shop.ManagerName,
                Cccd: shop.Cccd,
                RentalLocation: shop.RentalLocation,
                Month: shop.LeaseStartDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                UsageMonth: shop.LeaseStartDate.AddMonths(-1).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                BillingMonthKey: billingMonth,
                UsageMonthKey: usageMonth,
                LeaseStartDate: shop.LeaseStartDate,
                ElectricityUsage: shop.ElectricityUsage,
                ElectricityFee: shop.ElectricityFee,
                WaterUsage: shop.WaterUsage,
                WaterFee: shop.WaterFee,
                ServiceFee: shop.ServiceFee,
                LeaseTermDays: shop.LeaseTermDays,
                TotalDue: shop.TotalDue,
                ContractImage: shop.ContractImage,
                CreatedAt: shop.LeaseStartDate.AddDays(1));
        }

        private static DateTime GetLeaseStartDate(int index)
            => new(2026, 4 + ((index - 1) % 6), 1, 0, 0, 0, DateTimeKind.Utc);

        private static string GetLegacyFloorLabel(string rentalLocation)
            => rentalLocation.StartsWith("3", StringComparison.OrdinalIgnoreCase) || rentalLocation.StartsWith("K3", StringComparison.OrdinalIgnoreCase)
                ? "2"
                : "1";

        private static ManagerProfile GetManagerProfile(int index)
            => index switch
            {
                1 => new("Nguyen Van Minh", "Minh Fashion", "District 1, Ho Chi Minh City", "079203000111", "/images/profiles/manager-1.png"),
                2 => new("Tran Thi Lan", "Lan Cosmetics", "Thu Duc, Ho Chi Minh City", "079203000222", "/images/profiles/manager-2.png"),
                3 => new("Pham Gia Huy", "Huy Sneakers", "Go Vap, Ho Chi Minh City", "079203000333", "/images/profiles/manager-3.png"),
                4 => new("Le Bao Chau", "Chau Accessories", "Binh Thanh, Ho Chi Minh City", "079203000444", "/images/profiles/manager-4.png"),
                5 => new("Vo Quynh Anh", "Quynh Gifts", "District 7, Ho Chi Minh City", "079203000555", "/images/profiles/manager-5.png"),
                _ => new(
                    $"Seed Manager {index:00}",
                    $"Seed Shop {index:00}",
                    $"District {(index % 12) + 1}, Ho Chi Minh City",
                    $"079203{index:000000}",
                    $"/images/profiles/manager-{index}.png")
            };

        private static ManagerProfile GetUnassignedManagerProfile(int index)
            => new(
                $"Prospect Manager {index:00}",
                string.Empty,
                $"District {(index % 12) + 1}, Ho Chi Minh City",
                $"089204{index:000000}",
                $"/images/profiles/manager-{index}.png");
    }

    private sealed record UserSeed(
        string Id,
        string Email,
        string Password,
        string Role,
        string? FullName,
        string? ShopId,
        string? Image,
        string? Address,
        string? Cccd,
        DateTime CreatedAt);

    private sealed record ShopSeed(
        string Id,
        string ShopName,
        string? ManagerName,
        string? Cccd,
        string RentalLocation,
        string Month,
        DateTime LeaseStartDate,
        string ElectricityUsage,
        decimal ElectricityFee,
        string WaterUsage,
        decimal WaterFee,
        decimal ServiceFee,
        int LeaseTermDays,
        decimal TotalDue,
        string? ContractImage,
        DateTime CreatedAt,
        string? Slug = null,
        string? Category = null,
        string? Floor = null,
        string? Summary = null,
        string? Description = null,
        string? LogoUrl = null,
        string? CoverImageUrl = null,
        string? OpenHours = null,
        string? Badge = null,
        string? Offer = null,
        string? Tags = null);

    private sealed record RentalAreaSeed(
        string Id,
        string AreaCode,
        string Floor,
        string AreaName,
        string Size,
        decimal MonthlyRent,
        string Status,
        string? TenantName,
        string? ShopInfoId,
        DateTime CreatedAt);

    private sealed record MonthlyBillSeed(
        string Id,
        string ShopInfoId,
        string BillKey,
        string ShopName,
        string? ManagerName,
        string? Cccd,
        string RentalLocation,
        string Month,
        string UsageMonth,
        string BillingMonthKey,
        string UsageMonthKey,
        DateTime LeaseStartDate,
        string ElectricityUsage,
        decimal ElectricityFee,
        string WaterUsage,
        decimal WaterFee,
        decimal ServiceFee,
        int LeaseTermDays,
        decimal TotalDue,
        string? ContractImage,
        DateTime CreatedAt);

    private sealed record ProfileUpdateHistorySeed(
        string Id,
        string UserId,
        string Email,
        string? PreviousFullName,
        string? PreviousAddress,
        string? PreviousImage,
        string? PreviousCccd,
        string? UpdatedFullName,
        string? UpdatedAddress,
        string? UpdatedImage,
        string? UpdatedCccd,
        DateTime UpdatedAt);

    private sealed record ManagedRentalAssignment(
        int MapLocationId,
        string RentalLocation,
        string MapShopUrl);

    private sealed record ManagerProfile(
        string FullName,
        string ShopName,
        string Address,
        string Cccd,
        string Image);
}
