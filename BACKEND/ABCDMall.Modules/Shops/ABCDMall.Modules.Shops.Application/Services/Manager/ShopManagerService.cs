using ABCDMall.Modules.Shops.Application.DTOs;
using ABCDMall.Modules.Shops.Domain.Entities;

namespace ABCDMall.Modules.Shops.Application.Services.Manager;

public sealed class ShopManagerService : IShopManagerService
{
    private readonly IShopManagerRepository _repository;

    public ShopManagerService(IShopManagerRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ShopDetailDto>> GetMyShopsAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        var shops = await _repository.GetShopsByOwnerAsync(ownerShopId, cancellationToken);
        return shops.Select(MapDetail).ToList();
    }

    public async Task<ShopDetailDto> CreateMyShopAsync(string ownerShopId, UpsertManagedShopRequestDto request, CancellationToken cancellationToken = default)
    {
        NormalizeRequest(request);

        if (await _repository.ExistsSlugAsync(request.Slug, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Shop slug already exists.");
        }

        var shop = new Shop
        {
            Id = Guid.NewGuid().ToString("N"),
            OwnerShopId = ownerShopId
        };

        ApplyRequest(shop, request);
        await _repository.AddShopAsync(shop, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapDetail(shop);
    }

    public async Task<ShopDetailDto?> UpdateMyShopAsync(string ownerShopId, string shopId, UpsertManagedShopRequestDto request, CancellationToken cancellationToken = default)
    {
        NormalizeRequest(request);

        var shop = await _repository.GetShopByIdAndOwnerAsync(shopId, ownerShopId, cancellationToken);
        if (shop is null)
        {
            return null;
        }

        if (await _repository.ExistsSlugAsync(request.Slug, shop.Id, cancellationToken))
        {
            throw new InvalidOperationException("Shop slug already exists.");
        }

        ApplyRequest(shop, request);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapDetail(shop);
    }

    public async Task<bool> DeleteMyShopAsync(string ownerShopId, string shopId, CancellationToken cancellationToken = default)
    {
        var shop = await _repository.GetShopByIdAndOwnerAsync(shopId, ownerShopId, cancellationToken);
        if (shop is null)
        {
            return false;
        }

        _repository.RemoveShop(shop);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static void ApplyRequest(Shop shop, UpsertManagedShopRequestDto request)
    {
        shop.Name = request.Name;
        shop.Slug = request.Slug;
        shop.Category = request.Category;
        shop.Floor = request.Floor;
        shop.LocationSlot = request.LocationSlot;
        shop.Summary = request.Summary;
        shop.Description = request.Description;
        shop.LogoUrl = request.LogoUrl;
        shop.CoverImageUrl = request.CoverImageUrl;
        shop.OpenHours = request.OpenHours;
        shop.Badge = request.Badge;
        shop.Offer = request.Offer;

        shop.Tags.Clear();
        foreach (var tag in request.Tags.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            shop.Tags.Add(new ShopTag
            {
                Id = $"{shop.Id}-tag-{NormalizeToken(tag)}",
                ShopId = shop.Id,
                Value = tag.Trim()
            });
        }
    }

    private static void NormalizeRequest(UpsertManagedShopRequestDto request)
    {
        request.Name = Require(request.Name, "Shop name");
        request.Slug = NormalizeSlug(Require(request.Slug, "Slug"));
        request.Category = Require(request.Category, "Category");
        request.Floor = Require(request.Floor, "Floor");
        request.LocationSlot = Require(request.LocationSlot, "Location slot");
        request.Summary = Require(request.Summary, "Summary");
        request.Description = Require(request.Description, "Description");
        request.LogoUrl = request.LogoUrl?.Trim() ?? string.Empty;
        request.CoverImageUrl = Require(request.CoverImageUrl, "Cover image URL");
        request.OpenHours = string.IsNullOrWhiteSpace(request.OpenHours) ? "09:30 - 22:00" : request.OpenHours.Trim();
        request.Badge = string.IsNullOrWhiteSpace(request.Badge) ? null : request.Badge.Trim();
        request.Offer = string.IsNullOrWhiteSpace(request.Offer) ? null : request.Offer.Trim();
        request.Tags = request.Tags.Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
    }

    private static string Require(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string NormalizeSlug(string value)
        => value.Trim().ToLowerInvariant().Replace(" ", "-");

    private static string NormalizeToken(string value)
        => NormalizeSlug(value).Replace("&", "and");

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
