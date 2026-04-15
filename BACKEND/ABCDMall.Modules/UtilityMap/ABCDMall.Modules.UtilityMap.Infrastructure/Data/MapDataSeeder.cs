using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.WebAPI;

public static class MapDataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Xóa sạch data cũ để nạp toạ độ chuẩn theo ảnh gốc
        if (await context.FloorPlans.AnyAsync())
        {
            context.MapLocations.RemoveRange(context.MapLocations);
            context.FloorPlans.RemoveRange(context.FloorPlans);
            await context.SaveChangesAsync();
            Console.WriteLine("🗑️  Đã xóa toạ độ cũ.");
        }

        var floors = new List<FloorPlan>
        {
            // ===== TẦNG 1 — Mặt tiền, Trang sức, Thời trang cao cấp =====
            new FloorPlan
            {
                FloorLevel = "Tầng 1",
                Description = "Tầng 1 - Thời trang cao cấp & Trang sức / Mỹ phẩm",
                BlueprintImageUrl = "/images/maps/floor-tang1.png",
                Locations = new List<MapLocation>
                {
                    new() { ShopName = "Uniqlo",             LocationSlot = "1-01",  X= 49.62, Y= 18.39, ShopUrl = "/shops/uniqlo",              StorefrontImageUrl = "" },
                    new() { ShopName = "Adidas",             LocationSlot = "1-11",  X= 22.46, Y= 27.85, ShopUrl = "/shops/adidas",              StorefrontImageUrl = "" },
                    new() { ShopName = "Levi's",             LocationSlot = "1-02",  X = 75.50, Y = 37.50, ShopUrl = "/shops/levis",               StorefrontImageUrl = "" },
                    new() { ShopName = "Chuk",               LocationSlot = "1-03",  X= 71.91, Y= 42.67, ShopUrl = "/shops/chuk",                StorefrontImageUrl = "" },
                    new() { ShopName = "Beauty Box",         LocationSlot = "SB-01", X = 62.00, Y = 31.00, ShopUrl = "/shops/beauty-box",          StorefrontImageUrl = "" },
                    new() { ShopName = "PNJ",                LocationSlot = "1-27",  X = 75.50, Y = 33.00, ShopUrl = "/shops/pnj",                 StorefrontImageUrl = "" },
                    new() { ShopName = "Lộc Phúc",          LocationSlot = "1-26",  X = 77.00, Y = 30.00, ShopUrl = "/shops/loc-phuc",            StorefrontImageUrl = "" },
                    new() { ShopName = "Đông Hải",          LocationSlot = "1-23",  X= 81.22, Y= 23.96, ShopUrl = "/shops/dong-hai",            StorefrontImageUrl = "" },
                    new() { ShopName = "Thế Giới Kim Cương", LocationSlot = "K1-04", X= 69.29, Y= 32.91, ShopUrl = "/shops/the-gioi-kim-cuong",  StorefrontImageUrl = "" },
                    new() { ShopName = "Starbucks Coffee",  LocationSlot = "1-09A", X= 21.48, Y= 38.11, ShopUrl = "/shops/starbucks",           StorefrontImageUrl = "" },
                    new() { ShopName = "Highlands Coffee",  LocationSlot = "1-03A", X= 72.6, Y= 47.98, ShopUrl = "/shops/highlands-coffee",    StorefrontImageUrl = "" },
                    new() { ShopName = "Public Bank",       LocationSlot = "1-10",  X = 12.50, Y = 38.00, ShopUrl = "/shops/public-bank",         StorefrontImageUrl = "" },
                    new() { ShopName = "Pedro",             LocationSlot = "1-09",  X = 33.49, Y = 44.46, ShopUrl = "/shops/pedro",               StorefrontImageUrl = "" },
                    new() { ShopName = "Ecco",              LocationSlot = "1-08A", X= 40.26, Y= 42.15, ShopUrl = "/shops/ecco",                StorefrontImageUrl = "" },
                    new() { ShopName = "Futureworld",       LocationSlot = "1-08B", X= 43.73, Y= 42.44, ShopUrl = "/shops/futureworld",         StorefrontImageUrl = "" },
                    new() { ShopName = "Nike",              LocationSlot = "1-07",  X= 47.81, Y= 42.79, ShopUrl = "/shops/nike",                StorefrontImageUrl = "" },
                    new() { ShopName = "Charles & Keith",  LocationSlot = "1-06",  X= 53.89, Y= 44, ShopUrl = "/shops/charles-keith",       StorefrontImageUrl = "" },
                    new() { ShopName = "Aldo",             LocationSlot = "1-03B", X= 65.82, Y= 45.44, ShopUrl = "/shops/aldo",                StorefrontImageUrl = "" },
                    new() { ShopName = "Vascara",          LocationSlot = "1-05",  X= 72.29, Y= 27.83, ShopUrl = "/shops/vascara",             StorefrontImageUrl = "" },
                    new() { ShopName = "Mujosh",           LocationSlot = "1-09B", X= 37.49, Y= 42.15, ShopUrl = "/shops/mujosh",              StorefrontImageUrl = "" },
                    new() { ShopName = "Chagee",           LocationSlot = "1-20",  X= 69.06, Y= 18.13, ShopUrl = "/shops/chagee",                StorefrontImageUrl = "" },
                    new() { ShopName = "Elise",            LocationSlot = "1-09",  X= 75.6, Y= 19, ShopUrl = "/shops/elise",     StorefrontImageUrl = "" },
                    new() { ShopName = "Casio",          LocationSlot = "K1-11",  X= 70.44, Y= 25.18, ShopUrl = "/shops/casio",             StorefrontImageUrl = "" },
                }
            },

            // ===== TẦNG 2 — Mua sắm & Nhà sách =====
            new FloorPlan
            {
                FloorLevel = "Tầng 2",
                Description = "Tầng 2 - Nhà sách & Giáo dục / Thời trang / Đồ chơi",
                BlueprintImageUrl = "/images/maps/floor-tang2.png",
                Locations = new List<MapLocation>
                {
                    new() { ShopName = "Phương Nam Book City", LocationSlot = "3-01",  X= 49.5, Y= 37.47, ShopUrl = "/shops/phuong-nam",         StorefrontImageUrl = "" },
                    new() { ShopName = "Trung Nguyên Coffee", LocationSlot = "3-01B", X= 63.59, Y= 26.21, ShopUrl = "/shops/trung-nguyen",        StorefrontImageUrl = "" },
                    new() { ShopName = "Ninomaxx",            LocationSlot = "3-31",  X= 80.53, Y= 35.11, ShopUrl = "/shops/ninomaxx",            StorefrontImageUrl = "" },
                    new() { ShopName = "Miniso",              LocationSlot = "3-03A", X= 70.98, Y= 55.26, ShopUrl = "/shops/miniso",              StorefrontImageUrl = "" },
                    new() { ShopName = "Rabity",              LocationSlot = "3-03",  X= 76.91, Y= 51.45, ShopUrl = "/shops/rabity",              StorefrontImageUrl = "" },
                    new() { ShopName = "BalaBala",            LocationSlot = "3-30",  X = 80.00, Y = 28.00, ShopUrl = "/shops/balabala",            StorefrontImageUrl = "" },
                    new() { ShopName = "Baa Baby",            LocationSlot = "K3-03", X= 72.52, Y= 38.17, ShopUrl = "/shops/baa-baby",            StorefrontImageUrl = "" },
                    new() { ShopName = "Pop Mart",            LocationSlot = "3-15",  X= 29.02, Y= 38.4, ShopUrl = "/shops/pop-mart",            StorefrontImageUrl = "" },
                    new() { ShopName = "Mochi Sweet",         LocationSlot = "3-16",  X= 35.72, Y= 37.94, ShopUrl = "/shops/mochi-sweet",         StorefrontImageUrl = "" },
                    new() { ShopName = "ILA",                 LocationSlot = "3-11",  X= 16.48, Y= 50.81, ShopUrl = "/shops/ila",                 StorefrontImageUrl = "" },
                    new() { ShopName = "Lug.vn",              LocationSlot = "3-12",  X = 12.00, Y = 44.50, ShopUrl = "/shops/lugvn",               StorefrontImageUrl = "" },
                    new() { ShopName = "Vitimex",             LocationSlot = "3-10A", X= 25.18, Y= 55.66, ShopUrl = "/shops/vitimex",             StorefrontImageUrl = "" },
                    new() { ShopName = "Belluni",             LocationSlot = "3-10",  X= 29.02, Y= 55.95, ShopUrl = "/shops/belluni",             StorefrontImageUrl = "" },
                    new() { ShopName = "John Henry",          LocationSlot = "3-09",  X= 33.64, Y= 56.41, ShopUrl = "/shops/john-henry",          StorefrontImageUrl = "" },
                    new() { ShopName = "Boo",                 LocationSlot = "3-08",  X= 38.42, Y= 56.53, ShopUrl = "/shops/boo",                 StorefrontImageUrl = "" },
                    new() { ShopName = "Levents",             LocationSlot = "3-07",  X= 46.04, Y= 57.11, ShopUrl = "/shops/levents",             StorefrontImageUrl = "" },
                    new() { ShopName = "V-Sixty Four",        LocationSlot = "3-02",   X= 69.29, Y= 47.29, ShopUrl = "/shops/v-sixty-four",        StorefrontImageUrl = "" },
                    new() { ShopName = "Insidemen",           LocationSlot = "3-02",  X= 68.44, Y= 49.89, ShopUrl = "/shops/insidemen",           StorefrontImageUrl = "" },
                }
            },

            // ===== TẦNG 3 — Ẩm thực / Food Court =====
            new FloorPlan
            {
                FloorLevel = "Tầng 3",
                Description = "Tầng 3 - Ẩm thực & Food Court / Khu vui chơi giải trí",
                BlueprintImageUrl = "/images/maps/floor-tang3.png",
                Locations = new List<MapLocation>
                {
                    new() { ShopName = "Powerbowl 388",       LocationSlot = "5-01",  X = 45.00, Y = 24.00, ShopUrl = "/shops/powerbowl",         StorefrontImageUrl = "" },
                    new() { ShopName = "Kichi Kichi",         LocationSlot = "5-02",  X = 79.00, Y = 28.50, ShopUrl = "/food/kichi-kichi",         StorefrontImageUrl = "" },
                    new() { ShopName = "Gogi House",          LocationSlot = "5-02",  X = 78.00, Y = 34.50, ShopUrl = "/food/gogi-house",          StorefrontImageUrl = "" },
                    new() { ShopName = "Wow Yakiniku",        LocationSlot = "5-02",  X = 81.00, Y = 23.00, ShopUrl = "/food/wow-yakiniku",        StorefrontImageUrl = "" },
                    new() { ShopName = "Cloud Pot",           LocationSlot = "5-02",  X = 68.00, Y = 23.00, ShopUrl = "/food/cloud-pot",           StorefrontImageUrl = "" },
                    new() { ShopName = "Sushi X",             LocationSlot = "5-02",  X= 71.37, Y= 21.6, ShopUrl = "/food/sushi-x",             StorefrontImageUrl = "" },
                    new() { ShopName = "Dao Niu Guo",         LocationSlot = "5-02",  X = 63.00, Y = 33.00, ShopUrl = "/food/dao-niu-guo",         StorefrontImageUrl = "" },
                    new() { ShopName = "Texas Chicken",       LocationSlot = "5-16",  X = 23.50, Y = 32.00, ShopUrl = "/food/texas-chicken",       StorefrontImageUrl = "" },
                    new() { ShopName = "Kohaku Sushi",        LocationSlot = "5-15",  X = 26.50, Y = 38.50, ShopUrl = "/food/kohaku-sushi",        StorefrontImageUrl = "" },
                    new() { ShopName = "Buffet Hoàng Yến",  LocationSlot = "5-13",  X = 14.50, Y = 46.50, ShopUrl = "/food/buffet-hoang-yen",    StorefrontImageUrl = "" },
                    new() { ShopName = "Thai Express",        LocationSlot = "5-12",  X= 28.79, Y= 50.99, ShopUrl = "/food/thai-express",        StorefrontImageUrl = "" },
                    new() { ShopName = "Tanxin",              LocationSlot = "5-11",  X= 35.26, Y= 51.62, ShopUrl = "/food/tanxin",              StorefrontImageUrl = "" },
                    new() { ShopName = "Taipan",               LocationSlot = "5-10",  X= 42.34, Y= 51.85, ShopUrl = "/food/tapan",               StorefrontImageUrl = "" },
                    new() { ShopName = "Khao Lao",            LocationSlot = "5-09",  X= 47.81, Y= 52.14, ShopUrl = "/food/khao-lao",            StorefrontImageUrl = "" },
                    new() { ShopName = "Chang Modern Thai",   LocationSlot = "5-08", X= 54.05, Y= 52.66, ShopUrl = "/food/chang",               StorefrontImageUrl = "" },
                    new() { ShopName = "Bonchon",             LocationSlot = "5-07",  X= 59.28, Y= 53.58, ShopUrl = "/food/bonchon",             StorefrontImageUrl = "" },
                    new() { ShopName = "Crystal Jade",        LocationSlot = "5-06",  X= 71.6, Y= 54.62, ShopUrl = "/food/crystal-jade",        StorefrontImageUrl = "" },
                    new() { ShopName = "Sushi Kei",           LocationSlot = "5-03A", X = 78.00, Y = 44.00, ShopUrl = "/food/sushi-kei",           StorefrontImageUrl = "" },
                }
            },

            // ===== TẦNG 4 — Rạp chiếu phim & Ẩm thực (MAPPING CHUẨN CỦA USER) =====
            new FloorPlan
            {
                FloorLevel = "Tầng 4",
                Description = "Tầng 4 - Rạp chiếu phim & Ẩm thực",
                BlueprintImageUrl = "/images/maps/floor-tang4.png",
                Locations = new List<MapLocation>
                {
                    new() { ShopName = "ABCD Cinemas",        LocationSlot = "6-01",  X= 70.06, Y= 20.18, ShopUrl = "/movies",                    StorefrontImageUrl = "" },
                    new() { ShopName = "Shabu Ya",            LocationSlot = "6-19",  X= 17.4, Y= 33.98, ShopUrl = "/food/shabu-ya",             StorefrontImageUrl = "" },
                    new() { ShopName = "Mikado Sushi",        LocationSlot = "6-20",  X= 34.64, Y= 32.02, ShopUrl = "/food/mikado-sushi",         StorefrontImageUrl = "" },
                    new() { ShopName = "H BBQ Buffet",        LocationSlot = "6-03",  X= 41.88, Y= 35.48, ShopUrl = "/food/h-bbq-buffet",         StorefrontImageUrl = "" },
                    new() { ShopName = "Dookki",              LocationSlot = "6-03A", X= 47.66, Y= 36.69, ShopUrl = "/food/dookki",               StorefrontImageUrl = "" },
                    new() { ShopName = "The Pizza Company",   LocationSlot = "6-04",  X= 57.43, Y= 34.27, ShopUrl = "/food/pizza-company",        StorefrontImageUrl = "" },
                    new() { ShopName = "Dairy Queen",         LocationSlot = "6-05",  X= 60.9, Y= 37.39, ShopUrl = "/food/dairy-queen",          StorefrontImageUrl = "" },
                    new() { ShopName = "Lok Lok Hotpot",      LocationSlot = "6-17",  X = 13.16, Y = 39.78, ShopUrl = "/food/lok-lok-hotpot",       StorefrontImageUrl = "" },
                    new() { ShopName = "Yamazaki Bakery",     LocationSlot = "6-18",  X= 21.63, Y= 40.77, ShopUrl = "/food/yamazaki-bakery",      StorefrontImageUrl = "" },
                    new() { ShopName = "Bobapop",             LocationSlot = "6-18A1",X= 25.64, Y= 41.34, ShopUrl = "/food/bobapop",              StorefrontImageUrl = "" },
                    new() { ShopName = "Marukame Udon",       LocationSlot = "6-07A", X= 68.37, Y= 41.6, ShopUrl = "/food/marukame-udon",        StorefrontImageUrl = "" },
                    new() { ShopName = "King BBQ",            LocationSlot = "6-16B", X= 14.32, Y= 46.42, ShopUrl = "/food/king-bbq",             StorefrontImageUrl = "" },
                    new() { ShopName = "Mei Wei",             LocationSlot = "6-16A", X= 19.71, Y= 46.39, ShopUrl = "/food/mei-wei",              StorefrontImageUrl = "" },
                    new() { ShopName = "Chang Kang Kung",     LocationSlot = "6-15",  X= 27.48, Y= 48.59, ShopUrl = "/food/chang-kang-kung",      StorefrontImageUrl = "" },
                    new() { ShopName = "Joopii",              LocationSlot = "6-13A", X= 34.26, Y= 49.1, ShopUrl = "/food/joopii",               StorefrontImageUrl = "" },
                    new() { ShopName = "Hot Pot Story",       LocationSlot = "6-13",  X= 40.03, Y= 48.91, ShopUrl = "/food/hotpot-story",         StorefrontImageUrl = "" },
                    new() { ShopName = "Tasaki BBQ",          LocationSlot = "6-12",  X= 46.35, Y= 49.51, ShopUrl = "/food/tasaki-bbq",           StorefrontImageUrl = "" },
                    new() { ShopName = "Al Fresco's",         LocationSlot = "6-11",  X= 51.43, Y= 50.78, ShopUrl = "/food/al-frescos",           StorefrontImageUrl = "" },
                    new() { ShopName = "Lẩu Nướng 88",       LocationSlot = "6-10",  X= 57.28, Y= 54.51, ShopUrl = "/food/lau-nuong-88",         StorefrontImageUrl = "" },
                    new() { ShopName = "Chilli Thai",         LocationSlot = "6-09",  X= 69.29, Y= 51.01, ShopUrl = "/food/chilli-thai",          StorefrontImageUrl = "" },
                    new() { ShopName = "Marukame Udon",       LocationSlot = "6-08",  X= 76.99, Y= 43.62, ShopUrl = "/food/Marukame-Udon",          StorefrontImageUrl = "" },
                    new() { ShopName = "Vocuppa Caffe",       LocationSlot = "6-07C",  X= 68.06, Y= 45.99, ShopUrl = "/food/Vocuppa-Caffe",          StorefrontImageUrl = "" },
                }
            }
        };

        await context.FloorPlans.AddRangeAsync(floors);
        await context.SaveChangesAsync();

        int total = floors.Sum(f => f.Locations.Count);
        Console.WriteLine($"✅ Đã seed lại chuẩn xác: 4 tầng, {total} shops.");
    }
}