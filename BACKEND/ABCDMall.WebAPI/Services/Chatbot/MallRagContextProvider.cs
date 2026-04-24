using System.Text;
using ABCDMall.Modules.Events.Application.DTOs;
using ABCDMall.Modules.Events.Application.DTOs.Events;
using ABCDMall.Modules.Events.Application.Services.Events;
using ABCDMall.Modules.Users.Application.Services.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.WebAPI.Services.Chatbot;

public sealed class MallRagContextProvider : IMallRagContextProvider
{
    private const int MaxTotalCharacters = 120_000;
    private const int MaxShopDescriptionChars = 800;

    private readonly IPublicShopCatalogReadRepository _catalogReadRepository;
    private readonly IEventQueryService _eventQueryService;

    public MallRagContextProvider(
        IPublicShopCatalogReadRepository catalogReadRepository,
        IEventQueryService eventQueryService)
    {
        _catalogReadRepository = catalogReadRepository;
        _eventQueryService = eventQueryService;
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
