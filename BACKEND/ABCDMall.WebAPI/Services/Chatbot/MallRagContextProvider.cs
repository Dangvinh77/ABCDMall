using System.Text;
using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.Movies.Application.Services.Movies;
using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Application.Services.Maps;

namespace ABCDMall.WebAPI.Services.Chatbot;

public sealed class MallRagContextProvider : IMallRagContextProvider
{
    private const int MaxTotalCharacters = 120_000;
    private const int MaxShopDescriptionChars = 800;

    private readonly IPublicShopCatalogReadRepository _catalogReadRepository;
    private readonly IEventQueryService _eventQueryService;
    private readonly IFoodQueryService _foodQueryService;
    private readonly IMovieQueryService _movieQueryService;
    private readonly IMapQueryService _mapQueryService;

    public MallRagContextProvider(
        IPublicShopCatalogReadRepository catalogReadRepository,
        IEventQueryService eventQueryService,
        IFoodQueryService foodQueryService,
        IMovieQueryService movieQueryService,
        IMapQueryService mapQueryService)
    {
        _catalogReadRepository = catalogReadRepository;
        _eventQueryService = eventQueryService;
        _foodQueryService = foodQueryService;
        _movieQueryService = movieQueryService;
        _mapQueryService = mapQueryService;
    }

    public async Task<string> GetContextTextAsync(CancellationToken cancellationToken = default)
    {
        var shops = (await _catalogReadRepository.GetShopInfosAsync(cancellationToken))
            .Where(s => s.IsPublicVisible && !string.IsNullOrWhiteSpace(s.Id))
            .OrderBy(s => s.ShopName)
            .ToList();

        var shopIds = shops
            .Select(s => s.Id!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        List<PublicShopProduct> products;
        List<PublicShopVoucher> vouchers;
        if (shopIds.Length == 0)
        {
            products = [];
            vouchers = [];
        }
        else
        {
            products = (await _catalogReadRepository.GetProductsAsync(shopIds, cancellationToken)).ToList();
            vouchers = (await _catalogReadRepository.GetVouchersAsync(shopIds, cancellationToken)).ToList();
        }

        var productsByShop = products.GroupBy(p => p.ShopId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);
        var vouchersByShop = vouchers.GroupBy(v => v.ShopId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var events = await _eventQueryService.GetListAsync(new EventListQueryDto(), cancellationToken);

        // Lấy dữ liệu FoodCourt
        var foods = await _foodQueryService.GetListAsync(null, cancellationToken);

        // Lấy dữ liệu Movies
        var movies = await _movieQueryService.GetListAsync(null, cancellationToken);

        // Lấy dữ liệu UtilityMap (bản đồ tiện ích)
        var floors = await _mapQueryService.GetAllFloorsAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("=== SHOPS (public, use slug for deep links) ===");
        foreach (var shop in shops)
        {
            var desc = Truncate(shop.Description, MaxShopDescriptionChars);
            sb.AppendLine(
                $"- Name: {shop.ShopName} | Slug: {shop.Slug} | Category: {shop.Category} | Floor: {shop.Floor} | Hours: {shop.OpenHours} | Summary: {shop.Summary}");
            sb.AppendLine($"  Description: {desc}");

            if (productsByShop.TryGetValue(shop.Id!, out var plist))
            {
                foreach (var p in plist.Take(40))
                {
                    sb.AppendLine(
                        $"  Product: {p.Name} | Price: {p.Price} | Featured: {p.IsFeatured} | Discounted: {p.IsDiscounted}");
                }

                if (plist.Count > 40)
                {
                    sb.AppendLine($"  ... {plist.Count - 40} more products omitted");
                }
            }

            if (vouchersByShop.TryGetValue(shop.Id!, out var vlist))
            {
                foreach (var v in vlist.Where(x => x.IsActive).Take(20))
                {
                    sb.AppendLine($"  Voucher: {v.Title} | Code: {v.Code} | Until: {v.ValidUntil}");
                }
            }
        }

        sb.AppendLine();
        sb.AppendLine("=== EVENTS ===");
        foreach (var e in events.OrderByDescending(x => x.StartDate))
        {
            sb.AppendLine(
                $"- Id: {e.Id} | Title: {e.Title} | Type: {e.EventType} | Status: {e.Status} | Start: {e.StartDate:u} | End: {e.EndDate:u} | Location: {e.Location} | Hot: {e.IsHot}");
            var ed = Truncate(e.Description, 500);
            sb.AppendLine($"  Description: {ed}");
        }

        // FoodCourt - Nhà hàng, quán ăn trong mall
        sb.AppendLine();
        sb.AppendLine("=== FOOD COURT (Nhà hàng, quán ăn) ===");
        foreach (var food in foods.Take(50))
        {
            var foodDesc = Truncate(food.Description ?? "", 200);
            sb.AppendLine(
                $"- Name: {food.Name} | Slug: {food.Slug} | Image: {food.ImageUrl}");
            if (!string.IsNullOrEmpty(foodDesc))
            {
                sb.AppendLine($"  Description: {foodDesc}");
            }
        }
        if (foods.Count > 50)
        {
            sb.AppendLine($"... {foods.Count - 50} more food items omitted");
        }

        // Movies - Rạp chiếu phim
        sb.AppendLine();
        sb.AppendLine("=== MOVIES (Rạp chiếu phim) ===");
        foreach (var movie in movies.Take(30))
        {
            sb.AppendLine(
                $"- Title: {movie.Title} | Slug: {movie.Slug} | Duration: {movie.DurationMinutes}min | Rating: {movie.RatingLabel} | Status: {movie.Status} | Genres: {string.Join(", ", movie.Genres)}");
        }
        if (movies.Count > 30)
        {
            sb.AppendLine($"... {movies.Count - 30} more movies omitted");
        }

        // UtilityMap - Bản đồ tiện ích (toilet, ATM, thang máy, thang cuốn...)
        sb.AppendLine();
        sb.AppendLine("=== UTILITY MAP (Bản đồ tiện ích) ===");
        sb.AppendLine("Các tầng trong mall:");
        foreach (var floor in floors)
        {
            sb.AppendLine($"- Floor: {floor.FloorLevel} | Description: {floor.Description}");
            foreach (var loc in floor.Locations.Take(20))
            {
                sb.AppendLine($"  - Location: {loc.LocationSlot} | Name: {loc.ShopName}");
            }
            if (floor.Locations.Count > 20)
            {
                sb.AppendLine($"  ... {floor.Locations.Count - 20} more locations on this floor");
            }
        }

        var text = sb.ToString();
        if (text.Length > MaxTotalCharacters)
        {
            return text[..MaxTotalCharacters] + "\n... [context truncated for size]";
        }

        return text;
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= max)
        {
            return value;
        }

        return value[..max] + "...";
    }
}
