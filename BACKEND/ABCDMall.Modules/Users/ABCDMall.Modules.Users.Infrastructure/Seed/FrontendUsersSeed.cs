using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure.Seed;

public static class FrontendUsersSeed
{
    public static async Task SeedAsync(MallDbContext db, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await SeedUsersAndShopsAsync(db, now, ct);
        await BiddingSeed.SeedAsync(db, ct);
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

        var seedIds    = SeedData.Users.Select(s => s.Id).ToArray();
        var seedEmails = SeedData.Users.Select(s => s.Email).ToArray();
        var existingUsers = await db.Users
            .Where(x => seedIds.Contains(x.Id) || seedEmails.Contains(x.Email))
            .ToListAsync(ct);

        foreach (var seed in SeedData.Users)
        {
            // Prefer ID match so that email changes don't duplicate rows
            var user = existingUsers.FirstOrDefault(x => string.Equals(x.Id, seed.Id, StringComparison.OrdinalIgnoreCase))
                    ?? existingUsers.FirstOrDefault(x => string.Equals(x.Email, seed.Email, StringComparison.OrdinalIgnoreCase));
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
        public static readonly UserSeed[] Users =
        [
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
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-001",
                Email: "manager1@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Nguyen Minh Quan",
                ShopId: "shop-001",
                Image: "/images/profiles/manager-1.png",
                Address: "District 1, Ho Chi Minh City",
                Cccd: "079203000111",
                CreatedAt: new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-002",
                Email: "manager2@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Tran Hoang Long",
                ShopId: "shop-002",
                Image: "/images/profiles/manager-2.png",
                Address: "Thu Duc, Ho Chi Minh City",
                Cccd: "079203000222",
                CreatedAt: new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-003",
                Email: "manager3@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Pham Bao Trung",
                ShopId: "shop-003",
                Image: "/images/profiles/manager-3.png",
                Address: "Go Vap, Ho Chi Minh City",
                Cccd: "079203000333",
                CreatedAt: new DateTime(2026, 4, 4, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-004",
                Email: "manager4@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Le Thi Phuong",
                ShopId: "shop-004",
                Image: "/images/profiles/manager-4.png",
                Address: "Binh Thanh, Ho Chi Minh City",
                Cccd: "079203000444",
                CreatedAt: new DateTime(2026, 4, 5, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-005",
                Email: "manager5@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Vo Ngoc Linh",
                ShopId: "shop-005",
                Image: "/images/profiles/manager-5.png",
                Address: "District 7, Ho Chi Minh City",
                Cccd: "079203000555",
                CreatedAt: new DateTime(2026, 4, 6, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-006",
                Email: "manager6@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Hoang Duc Nam",
                ShopId: "shop-006",
                Image: "/images/profiles/manager-6.png",
                Address: "District 3, Ho Chi Minh City",
                Cccd: "079203000666",
                CreatedAt: new DateTime(2026, 4, 7, 0, 0, 0, DateTimeKind.Utc)),
            new(
                Id: "users-manager-007",
                Email: "manager7@abcdmall.local",
                Password: "Manager@123",
                Role: "Manager",
                FullName: "Nguyen Thi Mai",
                ShopId: "shop-007",
                Image: "/images/profiles/manager-7.png",
                Address: "District 5, Ho Chi Minh City",
                Cccd: "079203000777",
                CreatedAt: new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc)),
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
        ];

        public static readonly ShopSeed[] Shops =
        [
            new(
                Id: "shop-001",
                ShopName: "Nike",
                ManagerName: "Nguyen Minh Quan",
                Cccd: "079203000111",
                RentalLocation: "1-07",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "165 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "14 m3",
                WaterFee: 15000m,
                ServiceFee: 900000m,
                LeaseTermDays: 365,
                TotalDue: 1702500m,
                ContractImage: "/img/nike/logo.webp",
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                Slug: "nike",
                Category: "Sportswear",
                Floor: "Level 1",
                LogoUrl: "/img/nike/logo.webp",
                CoverImageUrl: "/img/nike/out.jpg",
                OpenHours: "09:30 - 22:00"),
            new(
                Id: "shop-002",
                ShopName: "Adidas",
                ManagerName: "Tran Hoang Long",
                Cccd: "079203000222",
                RentalLocation: "1-11",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "148 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "12 m3",
                WaterFee: 15000m,
                ServiceFee: 850000m,
                LeaseTermDays: 365,
                TotalDue: 1643000m,
                ContractImage: "/img/adidas/logo.png",
                CreatedAt: new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                Slug: "adidas",
                Category: "Sportswear",
                Floor: "Level 1",
                LogoUrl: "/img/adidas/logo.png",
                CoverImageUrl: "/img/adidas/out.webp",
                OpenHours: "09:30 - 22:00"),
            new(
                Id: "shop-003",
                ShopName: "Uniqlo",
                ManagerName: "Pham Bao Trung",
                Cccd: "079203000333",
                RentalLocation: "1-01",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "182 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "18 m3",
                WaterFee: 15000m,
                ServiceFee: 1100000m,
                LeaseTermDays: 365,
                TotalDue: 1922000m,
                ContractImage: "/img/uniqlo/logo.png",
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                Slug: "uniqlo",
                Category: "Fashion",
                Floor: "Level 1",
                LogoUrl: "/img/uniqlo/logo.png",
                CoverImageUrl: "/img/uniqlo/out.jpg",
                OpenHours: "09:30 - 22:00"),
            new(
                Id: "shop-004",
                ShopName: "Charles & Keith",
                ManagerName: "Le Thi Phuong",
                Cccd: "079203000444",
                RentalLocation: "1-06",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "96 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "10 m3",
                WaterFee: 15000m,
                ServiceFee: 750000m,
                LeaseTermDays: 365,
                TotalDue: 1221000m,
                ContractImage: "/img/C&K/logo.jpg",
                CreatedAt: new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                Slug: "charles-keith",
                Category: "Bags & Accessories",
                Floor: "Level 1",
                LogoUrl: "/img/C&K/logo.jpg",
                CoverImageUrl: "/img/C&K/out.jpg",
                OpenHours: "09:30 - 22:00"),
            new(
                Id: "shop-005",
                ShopName: "Miniso",
                ManagerName: "Vo Ngoc Linh",
                Cccd: "079203000555",
                RentalLocation: "3-03A",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "120 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "12 m3",
                WaterFee: 15000m,
                ServiceFee: 680000m,
                LeaseTermDays: 365,
                TotalDue: 1355000m,
                ContractImage: "/img/miniso/logo.jpg",
                CreatedAt: new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc),
                Slug: "miniso",
                Category: "Lifestyle Store",
                Floor: "Level 2",
                LogoUrl: "/img/miniso/logo.jpg",
                CoverImageUrl: "/img/miniso/out.jpg",
                OpenHours: "09:30 - 22:00"),
            new(
                Id: "shop-006",
                ShopName: "Pop Mart",
                ManagerName: "Hoang Duc Nam",
                Cccd: "079203000666",
                RentalLocation: "3-15",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 7, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "88 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "8 m3",
                WaterFee: 15000m,
                ServiceFee: 620000m,
                LeaseTermDays: 180,
                TotalDue: 1043000m,
                ContractImage: "/img/popmart/logo.jpg",
                CreatedAt: new DateTime(2026, 4, 7, 0, 0, 0, DateTimeKind.Utc),
                Slug: "pop-mart",
                Category: "Collectibles & Toys",
                Floor: "Level 2",
                LogoUrl: "/img/popmart/logo.jpg",
                CoverImageUrl: "/img/popmart/out.webp",
                OpenHours: "09:30 - 22:00"),
            new(
                Id: "shop-007",
                ShopName: "Levents",
                ManagerName: "Nguyen Thi Mai",
                Cccd: "079203000777",
                RentalLocation: "3-07",
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "75 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "9 m3",
                WaterFee: 15000m,
                ServiceFee: 580000m,
                LeaseTermDays: 180,
                TotalDue: 877500m,
                ContractImage: "/img/levents/logo.png",
                CreatedAt: new DateTime(2026, 4, 8, 0, 0, 0, DateTimeKind.Utc),
                Slug: "levents",
                Category: "Streetwear",
                Floor: "Level 2",
                LogoUrl: "/img/levents/logo.png",
                CoverImageUrl: "/img/levents/OUT.webp",
                OpenHours: "09:30 - 22:00")
        ];

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
        [
            new("rental-area-001", "A-101", "1", "Sportswear Zone A", "42m2", 25000000m, "Rented", "Nike", "shop-001", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-002", "A-102", "1", "Accessory Hub", "30m2", 18000000m, "Available", null, null, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-003", "B-201", "1", "Sportswear Zone B", "35m2", 22000000m, "Rented", "Adidas", "shop-002", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-004", "C-301", "1", "Fashion Flagship", "28m2", 16000000m, "Rented", "Uniqlo", "shop-003", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-005", "D-102", "1", "Accessories Lane", "26m2", 17500000m, "Rented", "Charles & Keith", "shop-004", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-006", "D-103", "2", "Lifestyle Corner", "24m2", 15000000m, "Rented", "Miniso", "shop-005", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-007", "E-210", "2", "Collectibles Hub", "40m2", 24000000m, "Rented", "Pop Mart", "shop-006", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-008", "F-305", "2", "Streetwear Point", "33m2", 21000000m, "Rented", "Levents", "shop-007", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-009", "G-115", "1", "Toy House", "32m2", 19500000m, "Available", null, null, new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc)),
            new("rental-area-010", "H-410", "4", "Premium Lounge", "55m2", 32000000m, "Available", null, null, new DateTime(2026, 4, 3, 0, 0, 0, DateTimeKind.Utc))
        ];

        public static readonly MonthlyBillSeed[] MonthlyBills =
        [
            new(
                Id: "monthly-bill-001",
                ShopInfoId: "shop-001",
                BillKey: "shop-001:2026-04:2026-03:seed",
                ShopName: "Nike",
                ManagerName: "Nguyen Minh Quan",
                Cccd: "079203000111",
                RentalLocation: "1-07",
                Month: "April 2026",
                UsageMonth: "March 2026",
                BillingMonthKey: "2026-04",
                UsageMonthKey: "2026-03",
                LeaseStartDate: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                ElectricityUsage: "165 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "14 m3",
                WaterFee: 15000m,
                ServiceFee: 900000m,
                LeaseTermDays: 365,
                TotalDue: 1702500m,
                ContractImage: "/img/nike/logo.webp",
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-002", "shop-001", "shop-001:2026-05:2026-04:seed", "Nike", "Nguyen Minh Quan", "079203000111",
                "1-07", "May 2026", "April 2026", "2026-05", "2026-04", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "172 kWh", 3500m, "15 m3", 15000m, 900000m, 365, 1727000m, "/img/nike/logo.webp", new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-003", "shop-001", "shop-001:2026-06:2026-05:seed", "Nike", "Nguyen Minh Quan", "079203000111",
                "1-07", "June 2026", "May 2026", "2026-06", "2026-05", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "168 kWh", 3500m, "13 m3", 15000m, 900000m, 365, 1693000m, "/img/nike/logo.webp", new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-004", "shop-002", "shop-002:2026-05:2026-04:seed", "Adidas", "Tran Hoang Long", "079203000222",
                "1-11", "May 2026", "April 2026", "2026-05", "2026-04", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "148 kWh", 3500m, "12 m3", 15000m, 850000m, 365, 1643000m, "/img/adidas/logo.png", new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-005", "shop-002", "shop-002:2026-06:2026-05:seed", "Adidas", "Tran Hoang Long", "079203000222",
                "1-11", "June 2026", "May 2026", "2026-06", "2026-05", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "155 kWh", 3500m, "13 m3", 15000m, 850000m, 365, 1677500m, "/img/adidas/logo.png", new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-006", "shop-002", "shop-002:2026-07:2026-06:seed", "Adidas", "Tran Hoang Long", "079203000222",
                "1-11", "July 2026", "June 2026", "2026-07", "2026-06", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "151 kWh", 3500m, "11 m3", 15000m, 850000m, 365, 1653500m, "/img/adidas/logo.png", new DateTime(2026, 7, 2, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-007", "shop-003", "shop-003:2026-06:2026-05:seed", "Uniqlo", "Pham Bao Trung", "079203000333",
                "1-01", "June 2026", "May 2026", "2026-06", "2026-05", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "182 kWh", 3500m, "18 m3", 15000m, 1100000m, 365, 1922000m, "/img/uniqlo/logo.png", new DateTime(2026, 6, 3, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-008", "shop-003", "shop-003:2026-07:2026-06:seed", "Uniqlo", "Pham Bao Trung", "079203000333",
                "1-01", "July 2026", "June 2026", "2026-07", "2026-06", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "190 kWh", 3500m, "20 m3", 15000m, 1100000m, 365, 1965000m, "/img/uniqlo/logo.png", new DateTime(2026, 7, 3, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-009", "shop-003", "shop-003:2026-08:2026-07:seed", "Uniqlo", "Pham Bao Trung", "079203000333",
                "1-01", "August 2026", "July 2026", "2026-08", "2026-07", new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                "185 kWh", 3500m, "19 m3", 15000m, 1100000m, 365, 1932500m, "/img/uniqlo/logo.png", new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-010", "shop-004", "shop-004:2026-07:2026-06:seed", "Charles & Keith", "Le Thi Phuong", "079203000444",
                "1-06", "July 2026", "June 2026", "2026-07", "2026-06", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                "96 kWh", 3500m, "10 m3", 15000m, 750000m, 365, 1221000m, "/img/C&K/logo.jpg", new DateTime(2026, 7, 4, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-011", "shop-004", "shop-004:2026-08:2026-07:seed", "Charles & Keith", "Le Thi Phuong", "079203000444",
                "1-06", "August 2026", "July 2026", "2026-08", "2026-07", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                "101 kWh", 3500m, "11 m3", 15000m, 750000m, 365, 1253500m, "/img/C&K/logo.jpg", new DateTime(2026, 8, 4, 0, 0, 0, DateTimeKind.Utc)),
            new(
                "monthly-bill-012", "shop-004", "shop-004:2026-09:2026-08:seed", "Charles & Keith", "Le Thi Phuong", "079203000444",
                "1-06", "September 2026", "August 2026", "2026-09", "2026-08", new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc),
                "98 kWh", 3500m, "10 m3", 15000m, 750000m, 365, 1228000m, "/img/C&K/logo.jpg", new DateTime(2026, 9, 4, 0, 0, 0, DateTimeKind.Utc))
        ];

        public static readonly ProfileUpdateHistorySeed[] ProfileUpdateHistories =
        [
            new(
                "profile-history-001", "users-manager-001", "manager1@abcdmall.local",
                "Nguyen Quan", "District 3, Ho Chi Minh City", "/images/profiles/manager-1-old.png", "079203000101",
                "Nguyen Minh Quan", "District 1, Ho Chi Minh City", "/images/profiles/manager-1.png", "079203000111",
                new DateTime(2026, 4, 10, 8, 0, 0, DateTimeKind.Utc)),
            new(
                "profile-history-002", "users-manager-002", "manager2@abcdmall.local",
                "Tran Long", "Binh Thanh, Ho Chi Minh City", null, "079203000202",
                "Tran Hoang Long", "Thu Duc, Ho Chi Minh City", "/images/profiles/manager-2.png", "079203000222",
                new DateTime(2026, 4, 12, 9, 30, 0, DateTimeKind.Utc)),
            new(
                "profile-history-003", "users-manager-003", "manager3@abcdmall.local",
                "Pham Trung", "Tan Binh, Ho Chi Minh City", null, "079203000303",
                "Pham Bao Trung", "Go Vap, Ho Chi Minh City", "/images/profiles/manager-3.png", "079203000333",
                new DateTime(2026, 4, 15, 10, 15, 0, DateTimeKind.Utc))
        ];
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
}
