using ABCDMall.Modules.Shops.Application.DTOs;
using ABCDMall.Modules.Shops.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.Shops.Application.Services.Catalog;

public sealed class ShopCatalogQueryService : IShopCatalogQueryService
{
    private readonly IShopCatalogRepository _repository;
    private readonly ILogger<ShopCatalogQueryService> _logger;

    public ShopCatalogQueryService(
        IShopCatalogRepository repository,
        ILogger<ShopCatalogQueryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ShopCatalogItemDto>> GetShopsAsync(CancellationToken cancellationToken = default)
    {
        var shops = await _repository.GetShopsAsync(cancellationToken);
        _logger.LogInformation("Fetched {ShopCount} shops from the catalog module.", shops.Count);
        return shops.Select(MapCatalogItem).ToList();
    }

    public async Task<ShopDetailDto?> GetShopBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var shop = await _repository.GetShopBySlugAsync(slug, cancellationToken);
        if (shop is null)
        {
            _logger.LogWarning("Shop with slug {ShopSlug} was not found.", slug);
            return null;
        }

        return MapDetail(shop);
    }

    private static ShopCatalogItemDto MapCatalogItem(Shop shop)
    {
        return new ShopCatalogItemDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Slug = shop.Slug,
            Category = shop.Category,
            Location = $"{shop.Floor}, {shop.LocationSlot}",
            Summary = shop.Summary,
            Description = shop.Description,
            ImageUrl = shop.CoverImageUrl,
            Badge = shop.Badge,
            Offer = shop.Offer,
            OpenHours = shop.OpenHours,
            Tags = shop.Tags.Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
        };
    }

    private static ShopDetailDto MapDetail(Shop shop)
    {
        return new ShopDetailDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Slug = shop.Slug,
            Category = shop.Category,
            Location = $"{shop.Floor}, {shop.LocationSlot}",
            Summary = shop.Summary,
            Description = shop.Description,
            ImageUrl = shop.CoverImageUrl,
            Badge = shop.Badge,
            Offer = shop.Offer,
            OpenHours = shop.OpenHours,
            Tags = shop.Tags.Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            LogoUrl = shop.LogoUrl,
            CoverImageUrl = shop.CoverImageUrl,
            Floor = shop.Floor,
            LocationSlot = shop.LocationSlot,
            Products = shop.Products
                .OrderByDescending(x => x.IsFeatured)
                .ThenBy(x => x.Name)
                .Select(x => new ShopProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    OldPrice = x.OldPrice,
                    DiscountPercent = x.DiscountPercent,
                    IsFeatured = x.IsFeatured,
                    IsDiscounted = x.IsDiscounted
                })
                .ToList(),
            Vouchers = shop.Vouchers
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Title)
                .Select(x => new ShopVoucherDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Title = x.Title,
                    Description = x.Description,
                    ValidUntil = x.ValidUntil,
                    IsActive = x.IsActive
                })
                .ToList()
        };
    }
}
