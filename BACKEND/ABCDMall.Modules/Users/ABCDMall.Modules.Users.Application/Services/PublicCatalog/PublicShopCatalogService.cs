using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public sealed class PublicShopCatalogService : IPublicShopCatalogService
{
    private readonly IPublicShopCatalogReadRepository _readRepository;

    public PublicShopCatalogService(IPublicShopCatalogReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<IReadOnlyList<PublicShopDto>> GetShopsAsync(CancellationToken cancellationToken = default)
    {
        var shopInfos = await _readRepository.GetShopInfosAsync(cancellationToken);
        var rentalAreas = await _readRepository.GetRentalAreasAsync(cancellationToken);
        var visibleShopInfos = shopInfos
            .Where(shopInfo => shopInfo.IsPublicVisible)
            .ToArray();
        var visibleShopIds = visibleShopInfos
            .SelectMany(GetCatalogLookupIds)
            .Where(id => id.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var productsByShopId = (await _readRepository.GetProductsAsync(visibleShopIds, cancellationToken))
            .GroupBy(product => product.ShopId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.AsEnumerable(), StringComparer.OrdinalIgnoreCase);
        var vouchersByShopId = (await _readRepository.GetVouchersAsync(visibleShopIds, cancellationToken))
            .GroupBy(voucher => voucher.ShopId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

        return visibleShopInfos
            .Select(shopInfo =>
            {
                var catalogLookupIds = GetCatalogLookupIds(shopInfo);
                var products = catalogLookupIds
                    .Where(productsByShopId.ContainsKey)
                    .SelectMany(id => productsByShopId[id]);
                var vouchers = catalogLookupIds
                    .Where(vouchersByShopId.ContainsKey)
                    .SelectMany(id => vouchersByShopId[id]);

                return ShopInfoPublicMapper.Map(shopInfo, FindRentalArea(shopInfo, rentalAreas), products, vouchers);
            })
            .OrderBy(shop => shop.Name)
            .ToList();
    }

    public async Task<PublicShopDto?> GetShopBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var shops = await GetShopsAsync(cancellationToken);
        return shops.FirstOrDefault(shop => string.Equals(shop.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    private static RentalArea? FindRentalArea(ShopInfo shopInfo, IReadOnlyList<RentalArea> rentalAreas)
    {
        return rentalAreas.FirstOrDefault(area =>
            (!string.IsNullOrWhiteSpace(shopInfo.RentalLocation) &&
             string.Equals(area.AreaCode, shopInfo.RentalLocation, StringComparison.OrdinalIgnoreCase)) ||
            (!string.IsNullOrWhiteSpace(shopInfo.ShopName) &&
             string.Equals(area.TenantName, shopInfo.ShopName, StringComparison.OrdinalIgnoreCase)));
    }

    private static string[] GetCatalogLookupIds(ShopInfo shopInfo)
    {
        var ids = new List<string>();

        if (!string.IsNullOrWhiteSpace(shopInfo.Id))
        {
            ids.Add(shopInfo.Id);
        }

        if (!string.IsNullOrWhiteSpace(shopInfo.Slug))
        {
            ids.Add($"shop-{shopInfo.Slug}");
        }

        return ids.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}

