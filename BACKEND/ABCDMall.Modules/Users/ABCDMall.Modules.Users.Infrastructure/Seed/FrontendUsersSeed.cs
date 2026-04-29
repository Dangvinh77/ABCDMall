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
            bill.PaymentStatus = seed.PaymentStatus;
            bill.PaidAtUtc = seed.PaidAtUtc;
            bill.StripeSessionId = null;
            bill.StripePaymentIntentId = null;
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
            new("shop-uniqlo", "uniqlo", "Uniqlo", "Nguyen Van Minh", "manager1@abcdmall.local", "079203000111", "1-01", "1", "Fashion Corner", "42m2", 25000000m),
            new("shop-miniso", "miniso", "Miniso", "Tran Thi Lan", "manager2@abcdmall.local", "079203000222", "3-03A", "2", "Lifestyle Store", "36m2", 21000000m),
            new("shop-nike", "nike", "Nike", "Pham Gia Huy", "manager3@abcdmall.local", "079203000333", "1-07", "1", "Sneaker Zone", "46m2", 28000000m),
            new("shop-charles-keith", "charles-keith", "Charles & Keith", "Le Bao Chau", "manager4@abcdmall.local", "079203000444", "1-06", "1", "Accessory Gallery", "34m2", 22000000m),
            new("shop-lego", "lego", "LEGO", "Vo Quynh Anh", "manager5@abcdmall.local", "079203000555", "6-02", "4", "Toy Gallery", "40m2", 27000000m),
            new("shop-adidas", "adidas", "Adidas", "Dang Mai Phuong", "manager6@abcdmall.local", "079203000666", "1-11", "1", "Sportswear Lane", "44m2", 26000000m),
            new("shop-levis", "levis", "Levi's", "Do Minh Khang", "manager7@abcdmall.local", "079203000777", "1-02", "1", "Denim Studio", "38m2", 23000000m),
            new("shop-beauty-box", "beauty-box", "Beauty Box", "Hoang Bao Tran", "manager8@abcdmall.local", "079203000888", "SB-01", "1", "Beauty Box", "30m2", 20000000m),
            new("shop-pnj", "pnj", "PNJ", "Bui Quoc Bao", "manager9@abcdmall.local", "079203000999", "1-27", "1", "Jewelry Boutique", "32m2", 24000000m),
            new("shop-pedro", "pedro", "Pedro", "Nguyen Hoai Nam", "manager10@abcdmall.local", "079203001010", "1-09", "1", "Leather Studio", "31m2", 21000000m),
            new("shop-casio", "casio", "Casio", "Tran Minh Thu", "manager11@abcdmall.local", "079203001011", "K1-11", "1", "Watch Counter", "24m2", 16000000m),
            new("shop-phuong-nam", "phuong-nam", "Phuong Nam Book City", "Pham Ngoc Anh", "manager12@abcdmall.local", "079203001012", "3-01", "2", "Book City", "55m2", 30000000m),
            new("shop-pop-mart", "pop-mart", "Pop Mart", "Le Thanh Tung", "manager13@abcdmall.local", "079203001013", "3-15", "2", "Collectible Hub", "28m2", 19000000m),
            new("shop-ninomaxx", "ninomaxx", "Ninomaxx", "Vo Minh Quan", "manager14@abcdmall.local", "079203001014", "3-31", "2", "Casualwear Point", "35m2", 20500000m),
            new("shop-levents", "levents", "Levents", "Dang Thao Nhi", "manager15@abcdmall.local", "079203001015", "3-07", "2", "Streetwear Corner", "34m2", 21000000m),
            new("shop-rabity", "rabity", "Rabity", "Nguyen Duc Anh", "manager16@abcdmall.local", "079203001016", "3-03", "2", "Kids Fashion", "33m2", 19500000m),
            new("shop-boo", "boo", "Boo", "Tran Quoc Viet", "manager17@abcdmall.local", "079203001017", "3-08", "2", "Youth Streetwear", "32m2", 20000000m),
            new("shop-john-henry", "john-henry", "John Henry", "Ho Thi My Linh", "manager18@abcdmall.local", "079203001018", "3-09", "2", "Menswear Studio", "35m2", 22000000m),
            new("shop-lugvn", "lugvn", "Lug.vn", "Phan Hoang Long", "manager19@abcdmall.local", "079203001019", "3-12", "2", "Travel Goods", "30m2", 19000000m),
            new("shop-powerbowl", "powerbowl", "Powerbowl 388", "Nguyen Gia Bao", "manager20@abcdmall.local", "079203001020", "5-01", "3", "Entertainment Zone", "80m2", 42000000m),
            new("shop-vans", "vans", "Vans", "Le Minh Tri", "manager21@abcdmall.local", "079203001021", "5-04", "3", "Skate Street", "34m2", 21000000m),
            new("shop-converse", "converse", "Converse", "Tran Anh Khoa", "manager22@abcdmall.local", "079203001022", "5-05", "3", "Sneaker Street", "34m2", 21000000m),
            new("shop-sony-center", "sony-center", "Sony Center", "Do Bao Ngoc", "manager23@abcdmall.local", "079203001023", "6-06", "4", "Technology Center", "52m2", 36000000m),
            new("shop-chuk", "chuk", "Chuk", "Nguyen Hoang Phuc", "manager24@abcdmall.local", "079203001024", "1-03", "1", "Food & Beverage", "28m2", 18000000m),
            new("shop-loc-phuc", "loc-phuc", "Loc Phuc", "Tran Bao Long", "manager25@abcdmall.local", "079203001025", "1-26", "1", "Jewelry Boutique", "30m2", 24000000m),
            new("shop-dong-hai", "dong-hai", "Dong Hai", "Le Minh Duc", "manager26@abcdmall.local", "079203001026", "1-23", "1", "Footwear Studio", "32m2", 22000000m),
            new("shop-the-gioi-kim-cuong", "the-gioi-kim-cuong", "The Gioi Kim Cuong", "Pham Thanh Dat", "manager27@abcdmall.local", "079203001027", "K1-04", "1", "Jewelry Counter", "24m2", 21000000m),
            new("shop-starbucks", "starbucks", "Starbucks Coffee", "Vo Gia Han", "manager28@abcdmall.local", "079203001028", "1-09A", "1", "Coffee Store", "38m2", 26000000m),
            new("shop-highlands-coffee", "highlands-coffee", "Highlands Coffee", "Dang Minh Anh", "manager29@abcdmall.local", "079203001029", "1-03A", "1", "Coffee Store", "36m2", 25000000m),
            new("shop-public-bank", "public-bank", "Public Bank", "Bui Quang Huy", "manager30@abcdmall.local", "079203001030", "1-10", "1", "Service Counter", "34m2", 23000000m),
            new("shop-ecco", "ecco", "Ecco", "Nguyen Khanh Linh", "manager31@abcdmall.local", "079203001031", "1-08A", "1", "Footwear Studio", "33m2", 22000000m),
            new("shop-futureworld", "futureworld", "Futureworld", "Tran Nhat Nam", "manager32@abcdmall.local", "079203001032", "1-08B", "1", "Technology Store", "35m2", 26000000m),
            new("shop-aldo", "aldo", "Aldo", "Le Mai Chi", "manager33@abcdmall.local", "079203001033", "1-03B", "1", "Accessory Gallery", "31m2", 21000000m),
            new("shop-vascara", "vascara", "Vascara", "Pham Thu Ha", "manager34@abcdmall.local", "079203001034", "1-05", "1", "Accessory Gallery", "32m2", 21000000m),
            new("shop-mujosh", "mujosh", "Mujosh", "Vo Minh Chau", "manager35@abcdmall.local", "079203001035", "1-09B", "1", "Eyewear Counter", "25m2", 17000000m),
            new("shop-chagee", "chagee", "Chagee", "Hoang Thuy Duong", "manager36@abcdmall.local", "079203001036", "1-20", "1", "Tea & Beverage", "28m2", 20000000m),
            new("shop-elise", "elise", "Elise", "Do Thanh Truc", "manager37@abcdmall.local", "079203001037", "1-09", "1", "Fashion Corner", "36m2", 23000000m),
            new("shop-trung-nguyen", "trung-nguyen", "Trung Nguyen Coffee", "Nguyen Quoc Viet", "manager38@abcdmall.local", "079203001038", "3-01B", "2", "Coffee Store", "34m2", 24000000m),
            new("shop-balabala", "balabala", "BalaBala", "Tran Ngoc Mai", "manager39@abcdmall.local", "079203001039", "3-30", "2", "Kids Fashion", "33m2", 20500000m),
            new("shop-baa-baby", "baa-baby", "Baa Baby", "Le Hoang Yen", "manager40@abcdmall.local", "079203001040", "K3-03", "2", "Baby Store", "26m2", 18000000m),
            new("shop-mochi-sweet", "mochi-sweet", "Mochi Sweet", "Pham Anh Tu", "manager41@abcdmall.local", "079203001041", "3-16", "2", "Dessert Counter", "24m2", 17000000m),
            new("shop-ila", "ila", "ILA", "Bui Thanh Tam", "manager42@abcdmall.local", "079203001042", "3-11", "2", "Education Center", "40m2", 26000000m),
            new("shop-vitimex", "vitimex", "Vitimex", "Nguyen Gia Linh", "manager43@abcdmall.local", "079203001043", "3-10A", "2", "Casualwear Point", "31m2", 19000000m),
            new("shop-belluni", "belluni", "Belluni", "Tran Duc Minh", "manager44@abcdmall.local", "079203001044", "3-10", "2", "Menswear Studio", "32m2", 20000000m),
            new("shop-v-sixty-four", "v-sixty-four", "V-Sixty Four", "Le Bao Ngoc", "manager45@abcdmall.local", "079203001045", "3-02", "2", "Fashion Corner", "30m2", 19000000m),
            new("shop-insidemen", "insidemen", "Insidemen", "Pham Hoang Nam", "manager46@abcdmall.local", "079203001046", "3-02", "2", "Menswear Studio", "30m2", 19000000m),
            new("shop-kichi-kichi", "kichi-kichi", "Kichi Kichi", "Manager 47", "manager47@abcdmall.local", "079203001047", "5-02", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-gogi-house", "gogi-house", "Gogi House", "Manager 48", "manager48@abcdmall.local", "079203001048", "5-02", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-wow-yakiniku", "wow-yakiniku", "Wow Yakiniku", "Manager 49", "manager49@abcdmall.local", "079203001049", "5-02", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-cloud-pot", "cloud-pot", "Cloud Pot", "Manager 50", "manager50@abcdmall.local", "079203001050", "5-02", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-sushix", "sushix", "Sushi X", "Manager 51", "manager51@abcdmall.local", "079203001051", "5-02", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-dao-niu-guo", "dao-niu-guo", "Dao Niu Guo", "Manager 52", "manager52@abcdmall.local", "079203001052", "5-02", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-texas-chicken", "texas-chicken", "Texas Chicken", "Manager 53", "manager53@abcdmall.local", "079203001053", "5-16", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-kohaku-sushi", "kohaku-sushi", "Kohaku Sushi", "Manager 54", "manager54@abcdmall.local", "079203001054", "5-15", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-buffet-hoang-yen", "buffet-hoang-yen", "Buffet Hoang Yen", "Manager 55", "manager55@abcdmall.local", "079203001055", "5-13", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-thai-express", "thai-express", "Thai Express", "Manager 56", "manager56@abcdmall.local", "079203001056", "5-12", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-tanxin", "tanxin", "Tanxin", "Manager 57", "manager57@abcdmall.local", "079203001057", "5-11", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-tapan", "tapan", "Taipan", "Manager 58", "manager58@abcdmall.local", "079203001058", "5-10", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-khao-lao", "khao-lao", "Khao Lao", "Manager 59", "manager59@abcdmall.local", "079203001059", "5-09", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-chang", "chang", "Chang Modern Thai", "Manager 60", "manager60@abcdmall.local", "079203001060", "5-08", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-bonchon", "bonchon", "Bonchon", "Manager 61", "manager61@abcdmall.local", "079203001061", "5-07", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-crystal-jade", "crystal-jade", "Crystal Jade", "Manager 62", "manager62@abcdmall.local", "079203001062", "5-06", "3", "Food & Beverage", "42m2", 28000000m),
            new("shop-sushi-kei", "sushi-kei", "Sushi Kei", "Manager 63", "manager63@abcdmall.local", "079203001063", "5-03A", "3", "Food & Beverage", "30m2", 28000000m),
            new("shop-movies", "movies", "ABCD Cinemas", "Manager 64", "manager64@abcdmall.local", "079203001064", "6-01", "4", "Cinema Complex", "120m2", 55000000m),
            new("shop-shabu-ya", "shabu-ya", "Shabu Ya", "Manager 65", "manager65@abcdmall.local", "079203001065", "6-19", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-mikado-sushi", "mikado-sushi", "Mikado Sushi", "Manager 66", "manager66@abcdmall.local", "079203001066", "6-20", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-h-bbq-buffet", "h-bbq-buffet", "H BBQ Buffet", "Manager 67", "manager67@abcdmall.local", "079203001067", "6-03", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-dookki-vietnam", "dookki-vietnam", "Dookki", "Manager 68", "manager68@abcdmall.local", "079203001068", "6-03A", "4", "Food & Beverage", "30m2", 32000000m),
            new("shop-the-pizza-company", "the-pizza-company", "The Pizza Company", "Manager 69", "manager69@abcdmall.local", "079203001069", "6-04", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-dairy-queen", "dairy-queen", "Dairy Queen", "Manager 70", "manager70@abcdmall.local", "079203001070", "6-05", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-lok-lok-hotpot", "lok-lok-hotpot", "Lok Lok Hotpot", "Manager 71", "manager71@abcdmall.local", "079203001071", "6-17", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-yamazaki-bakery", "yamazaki-bakery", "Yamazaki Bakery", "Manager 72", "manager72@abcdmall.local", "079203001072", "6-18", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-bobapop", "bobapop", "Bobapop", "Manager 73", "manager73@abcdmall.local", "079203001073", "6-18A1", "4", "Food & Beverage", "30m2", 32000000m),
            new("shop-marukame-udon", "marukame-udon", "Marukame Udon", "Manager 74", "manager74@abcdmall.local", "079203001074", "6-07A", "4", "Food & Beverage", "30m2", 32000000m),
            new("shop-king-bbq", "king-bbq", "King BBQ", "Manager 75", "manager75@abcdmall.local", "079203001075", "6-16B", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-mei-wei", "mei-wei", "Mei Wei", "Manager 76", "manager76@abcdmall.local", "079203001076", "6-16A", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-chang-kang-kung", "chang-kang-kung", "Chang Kang Kung", "Manager 77", "manager77@abcdmall.local", "079203001077", "6-15", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-joopii", "joopii", "Joopii", "Manager 78", "manager78@abcdmall.local", "079203001078", "6-13A", "4", "Food & Beverage", "30m2", 32000000m),
            new("shop-hot-pot-story", "hot-pot-story", "Hot Pot Story", "Manager 79", "manager79@abcdmall.local", "079203001079", "6-13", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-tasaki-bbq", "tasaki-bbq", "Tasaki BBQ", "Manager 80", "manager80@abcdmall.local", "079203001080", "6-12", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-al-frescos", "al-frescos", "Al Fresco's", "Manager 81", "manager81@abcdmall.local", "079203001081", "6-11", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-lu-nuong-88", "lu-nuong-88", "Lau Nuong 88", "Manager 82", "manager82@abcdmall.local", "079203001082", "6-10", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-chilli-thai", "chilli-thai", "Chilli Thai", "Manager 83", "manager83@abcdmall.local", "079203001083", "6-09", "4", "Food & Beverage", "42m2", 32000000m),
            new("shop-vocuppa-caffe", "vocuppa-caffe", "Vocuppa Caffe", "Manager 84", "manager84@abcdmall.local", "079203001084", "6-07C", "4", "Food & Beverage", "42m2", 32000000m)
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
        {
            (int BillingMonthNumber, string BillingMonthName, string BillingMonthKey, string UsageMonthName, string UsageMonthKey, int ElectricityUsageValue, int WaterUsageValue, string PaymentStatus, DateTime? PaidAtUtc)[] billConfigs =
            {
                (1, "January 2026", "2026-01", "December 2025", "2025-12", 210, 16, "Paid", new DateTime(2026, 1, 10, 9, 0, 0, DateTimeKind.Utc)),
                (2, "February 2026", "2026-02", "January 2026", "2026-01", 225, 18, "Paid", new DateTime(2026, 2, 10, 9, 0, 0, DateTimeKind.Utc)),
                (3, "March 2026", "2026-03", "February 2026", "2026-02", 240, 19, "Paid", new DateTime(2026, 3, 10, 9, 0, 0, DateTimeKind.Utc)),
                (4, "April 2026", "2026-04", "March 2026", "2026-03", 255, 21, "Unpaid", null),
            };

            return CatalogShopOwners
                .SelectMany((shop, shopIndex) => billConfigs.Select((config, monthIndex) =>
                {
                    var electricityUsageValue = config.ElectricityUsageValue + (shopIndex % 11) * 4 + monthIndex * 3;
                    var waterUsageValue = config.WaterUsageValue + (shopIndex % 7);
                    var electricityFee = 3500m;
                    var waterFee = 15000m;
                    var serviceFee = 800000m;
                    var totalDue = electricityUsageValue * electricityFee + waterUsageValue * waterFee + serviceFee;

                    return new MonthlyBillSeed(
                        Id: $"monthly-bill-{shopIndex + 1:000}-{config.BillingMonthNumber:00}",
                        ShopInfoId: shop.ShopId,
                        BillKey: $"{shop.ShopId}:{config.BillingMonthKey}:{config.UsageMonthKey}:seed",
                        ShopName: shop.ShopName,
                        ManagerName: shop.ManagerName,
                        Cccd: shop.Cccd,
                        RentalLocation: shop.RentalLocation,
                        Month: config.BillingMonthName,
                        UsageMonth: config.UsageMonthName,
                        BillingMonthKey: config.BillingMonthKey,
                        UsageMonthKey: config.UsageMonthKey,
                        LeaseStartDate: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(shopIndex),
                        ElectricityUsage: $"{electricityUsageValue} kWh",
                        ElectricityFee: electricityFee,
                        WaterUsage: $"{waterUsageValue} m3",
                        WaterFee: waterFee,
                        ServiceFee: serviceFee,
                        LeaseTermDays: 180,
                        TotalDue: totalDue,
                        PaymentStatus: config.PaymentStatus,
                        PaidAtUtc: config.PaidAtUtc?.AddMinutes(shopIndex),
                        ContractImage: $"/images/contracts/{shop.Slug}-contract.png",
                        CreatedAt: new DateTime(2026, config.BillingMonthNumber, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(shopIndex));
                }))
                .ToArray();
        }

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
                "chuk" => "/img/Chuk chuk/logo.jpg",
                "loc-phuc" => "/img/locphuc/logo.png",
                "dong-hai" => "/img/DongHai/logo.webp",
                "the-gioi-kim-cuong" => "/img/Thế Giới Kim Cương/logo.jpg",
                "starbucks" => "/img/starbuck/logo.png",
                "highlands-coffee" => "/img/2lands/logo.png",
                "public-bank" => "/img/public bank/logo.png",
                "ecco" => "/img/ecco/logo.png",
                "futureworld" => "/img/futureword/logo.jpg",
                "aldo" => "/img/aldo/logo.jpg",
                "vascara" => "/img/vascara/logo.png",
                "mujosh" => "/img/mujosh/logo.jpg",
                "chagee" => "/img/Chagee/logo.png",
                "elise" => "/img/Elise/logo.png",
                "trung-nguyen" => "/img/trungnguyen/logo.jpg",
                "balabala" => "/img/BalaBala/logo.jpg",
                "baa-baby" => "/img/Baa Baby/logo.png",
                "mochi-sweet" => "/img/Mochi Sweet/logo.webp",
                "ila" => "/img/ILA/logo.webp",
                "vitimex" => "/img/Vitimex/logo.png",
                "belluni" => "/img/Belluni/logo.png",
                "v-sixty-four" => "/img/V-Sixty Four/logo.png",
                "insidemen" => "/img/Insidemen/logo.png",
                _ => "/img/ABCDMall/logo.png"
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
                "chuk" => "/img/Chuk chuk/logo.jpg",
                "loc-phuc" => "/img/locphuc/logo.png",
                "dong-hai" => "/img/DongHai/logo.webp",
                "the-gioi-kim-cuong" => "/img/Thế Giới Kim Cương/logo.jpg",
                "starbucks" => "/img/starbuck/logo.png",
                "highlands-coffee" => "/img/2lands/logo.png",
                "public-bank" => "/img/public bank/logo.png",
                "ecco" => "/img/ecco/logo.png",
                "futureworld" => "/img/futureword/logo.jpg",
                "aldo" => "/img/aldo/logo.jpg",
                "vascara" => "/img/vascara/logo.png",
                "mujosh" => "/img/mujosh/logo.jpg",
                "chagee" => "/img/Chagee/logo.png",
                "elise" => "/img/Elise/logo.png",
                "trung-nguyen" => "/img/trungnguyen/logo.jpg",
                "balabala" => "/img/BalaBala/logo.jpg",
                "baa-baby" => "/img/Baa Baby/logo.png",
                "mochi-sweet" => "/img/Mochi Sweet/logo.webp",
                "ila" => "/img/ILA/logo.webp",
                "vitimex" => "/img/Vitimex/logo.png",
                "belluni" => "/img/Belluni/logo.png",
                "v-sixty-four" => "/img/V-Sixty Four/logo.png",
                "insidemen" => "/img/Insidemen/logo.png",
                _ => "/img/ABCDMall/logo.png"
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
        string PaymentStatus,
        DateTime? PaidAtUtc,
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
