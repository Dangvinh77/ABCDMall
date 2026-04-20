using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
        var existingShops = await db.ShopInfos
            .Where(x => SeedData.Shops.Select(seed => seed.Id).Contains(x.Id!))
            .ToListAsync(ct);

        foreach (var seed in SeedData.Shops)
        {
            var shop = existingShops.FirstOrDefault(x => x.Id == seed.Id);
            if (shop is null)
            {
                shop = new ShopInfo { Id = seed.Id };
                await db.ShopInfos.AddAsync(shop, ct);
                existingShops.Add(shop);
            }

            shop.ShopName = seed.ShopName;
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
            shop.Tags = seed.Tags;
            shop.IsPublicVisible = seed.IsPublicVisible;
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

    private static class SeedData
    {
        private static readonly CatalogShopOwnerSeed[] CatalogShopOwners =
        [
            new("shop-001", "uniqlo", "Uniqlo", "Nguyen Van Minh", "manager1@abcdmall.local", "079203000111", "1-01", "1", "Fashion Corner", "42m2", 25000000m),
            new("shop-002", "miniso", "Miniso", "Tran Thi Lan", "manager2@abcdmall.local", "079203000222", "3-03A", "2", "Lifestyle Store", "36m2", 21000000m),
            new("shop-003", "nike", "Nike", "Pham Gia Huy", "manager3@abcdmall.local", "079203000333", "1-07", "1", "Sneaker Zone", "46m2", 28000000m),
            new("shop-004", "charles-keith", "Charles & Keith", "Le Bao Chau", "manager4@abcdmall.local", "079203000444", "1-06", "1", "Accessory Gallery", "34m2", 22000000m),
            new("shop-005", "lego", "LEGO", "Vo Quynh Anh", "manager5@abcdmall.local", "079203000555", "6-02", "4", "Toy Gallery", "40m2", 27000000m),
            new("shop-006", "adidas", "Adidas", "Dang Mai Phuong", "manager6@abcdmall.local", "079203000666", "1-11", "1", "Sportswear Lane", "44m2", 26000000m),
            new("shop-007", "levis", "Levi's", "Do Minh Khang", "manager7@abcdmall.local", "079203000777", "1-02", "1", "Denim Studio", "38m2", 23000000m),
            new("shop-008", "beauty-box", "Beauty Box", "Hoang Bao Tran", "manager8@abcdmall.local", "079203000888", "SB-01", "1", "Beauty Box", "30m2", 20000000m),
            new("shop-009", "pnj", "PNJ", "Bui Quoc Bao", "manager9@abcdmall.local", "079203000999", "1-27", "1", "Jewelry Boutique", "32m2", 24000000m),
            new("shop-010", "pedro", "Pedro", "Nguyen Hoai Nam", "manager10@abcdmall.local", "079203001010", "1-09", "1", "Leather Studio", "31m2", 21000000m),
            new("shop-011", "casio", "Casio", "Tran Minh Thu", "manager11@abcdmall.local", "079203001011", "K1-11", "1", "Watch Counter", "24m2", 16000000m),
            new("shop-012", "phuong-nam", "Phuong Nam Book City", "Pham Ngoc Anh", "manager12@abcdmall.local", "079203001012", "3-01", "2", "Book City", "55m2", 30000000m),
            new("shop-013", "pop-mart", "Pop Mart", "Le Thanh Tung", "manager13@abcdmall.local", "079203001013", "3-15", "2", "Collectible Hub", "28m2", 19000000m),
            new("shop-014", "ninomaxx", "Ninomaxx", "Vo Minh Quan", "manager14@abcdmall.local", "079203001014", "3-31", "2", "Casualwear Point", "35m2", 20500000m),
            new("shop-015", "levents", "Levents", "Dang Thao Nhi", "manager15@abcdmall.local", "079203001015", "3-07", "2", "Streetwear Corner", "34m2", 21000000m),
            new("shop-016", "rabity", "Rabity", "Nguyen Duc Anh", "manager16@abcdmall.local", "079203001016", "3-03", "2", "Kids Fashion", "33m2", 19500000m),
            new("shop-017", "boo", "Boo", "Tran Quoc Viet", "manager17@abcdmall.local", "079203001017", "3-08", "2", "Youth Streetwear", "32m2", 20000000m),
            new("shop-018", "john-henry", "John Henry", "Ho Thi My Linh", "manager18@abcdmall.local", "079203001018", "3-09", "2", "Menswear Studio", "35m2", 22000000m),
            new("shop-019", "lugvn", "Lug.vn", "Phan Hoang Long", "manager19@abcdmall.local", "079203001019", "3-12", "2", "Travel Goods", "30m2", 19000000m),
            new("shop-020", "powerbowl", "Powerbowl 388", "Nguyen Gia Bao", "manager20@abcdmall.local", "079203001020", "5-01", "3", "Entertainment Zone", "80m2", 42000000m),
            new("shop-021", "vans", "Vans", "Le Minh Tri", "manager21@abcdmall.local", "079203001021", "5-04", "3", "Skate Street", "34m2", 21000000m),
            new("shop-022", "converse", "Converse", "Tran Anh Khoa", "manager22@abcdmall.local", "079203001022", "5-05", "3", "Sneaker Street", "34m2", 21000000m),
            new("shop-023", "sony-center", "Sony Center", "Do Bao Ngoc", "manager23@abcdmall.local", "079203001023", "6-06", "4", "Technology Center", "52m2", 36000000m)
        ];

        public static readonly UserSeed[] Users = BuildUsers();

        public static readonly ShopSeed[] Shops = BuildShops();

        public static readonly RentalAreaSeed[] RentalAreas = BuildRentalAreas();

        public static readonly MonthlyBillSeed[] MonthlyBills = BuildMonthlyBills();

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

            users.AddRange(CatalogShopOwners.Select((shop, index) => new UserSeed(
                Id: $"users-manager-{index + 1:000}",
                Email: shop.Email,
                Password: "Manager@123",
                Role: "Manager",
                FullName: shop.ManagerName,
                ShopId: shop.ShopId,
                Image: $"/images/profiles/manager-{index + 1}.png",
                Address: "Ho Chi Minh City",
                Cccd: shop.Cccd,
                CreatedAt: new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc).AddDays(index))));

            return users.ToArray();
        }

        private static ShopSeed[] BuildShops()
            => CatalogShopOwners.Select((shop, index) => new ShopSeed(
                Id: shop.ShopId,
                ShopName: shop.ShopName,
                Slug: shop.Slug,
                Category: shop.AreaName,
                Floor: $"Floor {shop.Floor}",
                LocationSlot: shop.RentalLocation,
                Summary: $"{shop.ShopName} is available at {shop.RentalLocation} inside ABCD Mall.",
                Description: $"{shop.ShopName} is managed by {shop.ManagerName} and operates from {shop.RentalLocation}.",
                LogoUrl: GetLogoUrl(shop.Slug),
                CoverImageUrl: GetCoverImageUrl(shop.Slug),
                OpenHours: "09:30 - 22:00",
                Badge: "Featured Store",
                Offer: null,
                Tags: $"{shop.AreaName}, Floor {shop.Floor}, {shop.RentalLocation}",
                IsPublicVisible: true,
                ManagerName: shop.ManagerName,
                Cccd: shop.Cccd,
                RentalLocation: shop.RentalLocation,
                Month: "April 2026",
                LeaseStartDate: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(index),
                ElectricityUsage: "0 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "0 m3",
                WaterFee: 15000m,
                ServiceFee: 800000m,
                LeaseTermDays: 180,
                TotalDue: 800000m,
                ContractImage: $"/images/contracts/{shop.Slug}-contract.png",
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(index)))
            .ToArray();

        private static RentalAreaSeed[] BuildRentalAreas()
            => CatalogShopOwners.Select((shop, index) => new RentalAreaSeed(
                Id: $"rental-area-{index + 1:000}",
                AreaCode: shop.RentalLocation,
                Floor: shop.Floor,
                AreaName: shop.AreaName,
                Size: shop.Size,
                MonthlyRent: shop.MonthlyRent,
                Status: "Rented",
                TenantName: shop.ShopName,
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(index)))
            .ToArray();

        private static MonthlyBillSeed[] BuildMonthlyBills()
            => CatalogShopOwners.Select((shop, index) => new MonthlyBillSeed(
                Id: $"monthly-bill-{index + 1:000}",
                ShopInfoId: shop.ShopId,
                BillKey: $"{shop.ShopId}:2026-04:2026-03:seed",
                ShopName: shop.ShopName,
                ManagerName: shop.ManagerName,
                Cccd: shop.Cccd,
                RentalLocation: shop.RentalLocation,
                Month: "April 2026",
                UsageMonth: "March 2026",
                BillingMonthKey: "2026-04",
                UsageMonthKey: "2026-03",
                LeaseStartDate: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(index),
                ElectricityUsage: "0 kWh",
                ElectricityFee: 3500m,
                WaterUsage: "0 m3",
                WaterFee: 15000m,
                ServiceFee: 800000m,
                LeaseTermDays: 180,
                TotalDue: 800000m,
                ContractImage: $"/images/contracts/{shop.Slug}-contract.png",
                CreatedAt: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(index)))
            .ToArray();

        private static string GetLogoUrl(string slug)
            => slug switch
            {
                "uniqlo" => "/img/uniqlo/logo.png",
                "miniso" => "/img/miniso/logo.jpg",
                "nike" => "/img/nike/logo.webp",
                "charles-keith" => "/img/C&K/logo.jpg",
                "lego" => "/img/lego/logo.png",
                "adidas" => "/img/adidas/logo.png",
                "levis" => "/img/levi/logo.png",
                "beauty-box" => "/img/beautybox/logo.png",
                "pnj" => "/img/pnj/logo.jpg",
                "pedro" => "/img/pedro/logo.png",
                "casio" => "/img/casio/logo.png",
                "phuong-nam" => "/img/phuongnam/logo.jpg",
                "pop-mart" => "/img/popmart/logo.jpg",
                "ninomaxx" => "/img/NINOMAXX/logo.png",
                "levents" => "/img/levents/logo.png",
                "rabity" => "/img/rabity/logo.jpg",
                "boo" => "/img/boo/logo.png",
                "john-henry" => "/img/johnhenry/logo.png",
                "lugvn" => "/img/lug/logo.png",
                "powerbowl" => "/img/powerbowl/logo.png",
                "vans" => "/img/vans/logo.jpg",
                "converse" => "/img/converse/logo.webp",
                "sony-center" => "/img/sonycenter/logo.jpeg",
                _ => string.Empty
            };

        private static string GetCoverImageUrl(string slug)
            => slug switch
            {
                "uniqlo" => "/img/uniqlo/out.jpg",
                "miniso" => "/img/miniso/out.jpg",
                "nike" => "/img/nike/out.jpg",
                "charles-keith" => "/img/C&K/out.jpg",
                "lego" => "/img/lego/out.webp",
                "adidas" => "/img/adidas/out.webp",
                "levis" => "/img/levi/out.jpg",
                "beauty-box" => "/img/beautybox/out.jpg",
                "pnj" => "/img/pnj/pnj_out.png",
                "pedro" => "/img/pedro/pedro_outdoor.png",
                "casio" => "/img/casio/out.png",
                "phuong-nam" => "/img/phuongnam/out.jpg",
                "pop-mart" => "/img/popmart/out.webp",
                "ninomaxx" => "/img/NINOMAXX/out.jpg",
                "levents" => "/img/levents/OUT.webp",
                "rabity" => "/img/rabity/out.jpg",
                "boo" => "/img/boo/out.jpg",
                "john-henry" => "/img/johnhenry/out.jpg",
                "lugvn" => "/img/lug/out.webp",
                "powerbowl" => "/img/powerbowl/out.jpg",
                "vans" => "/img/vans/out.avif",
                "converse" => "/img/converse/out.jpg",
                "sony-center" => "/img/sonycenter/out.png",
                _ => string.Empty
            };

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
    }

    private sealed record CatalogShopOwnerSeed(
        string ShopId,
        string Slug,
        string ShopName,
        string ManagerName,
        string Email,
        string Cccd,
        string RentalLocation,
        string Floor,
        string AreaName,
        string Size,
        decimal MonthlyRent);

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
            string Tags,
            bool IsPublicVisible,
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
        DateTime CreatedAt);

    private sealed record RentalAreaSeed(
        string Id,
        string AreaCode,
        string Floor,
        string AreaName,
        string Size,
        decimal MonthlyRent,
        string Status,
        string? TenantName,
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
