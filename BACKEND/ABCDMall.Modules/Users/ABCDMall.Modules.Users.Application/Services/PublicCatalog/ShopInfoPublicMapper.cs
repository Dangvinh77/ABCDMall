using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;
using System.Text.RegularExpressions;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

internal static class ShopInfoPublicMapper
{
    private const string DefaultImage =
        "https://images.unsplash.com/photo-1483985988355-763728e1935b?q=80&w=1600&auto=format&fit=crop";

    public static PublicShopDto Map(
        ShopInfo shopInfo,
        RentalArea? rentalArea = null,
        IEnumerable<PublicShopProduct>? products = null,
        IEnumerable<PublicShopVoucher>? vouchers = null)
    {
        var locationSlot = FirstNotEmpty(shopInfo.LocationSlot, shopInfo.RentalLocation, "ABCD Mall");
        var floor = FirstNotEmpty(shopInfo.Floor, rentalArea?.Floor is null ? null : $"Floor {rentalArea.Floor}", "Mall floor");
        var category = FirstNotEmpty(shopInfo.Category, rentalArea?.AreaName, "Retail Store");
        var summary = FirstNotEmpty(
            shopInfo.Summary,
            $"{shopInfo.ShopName} is operating at {locationSlot} in the {category.ToLowerInvariant()} zone.");
        var description = FirstNotEmpty(
            shopInfo.Description,
            $"{shopInfo.ShopName} is managed by {shopInfo.ManagerName ?? "the ABCD Mall team"} and positioned at {locationSlot}.");
        var coverImageUrl = FirstNotEmpty(shopInfo.CoverImageUrl, DefaultImage);

        return new PublicShopDto
        {
            Id = shopInfo.Id ?? string.Empty,
            Name = shopInfo.ShopName,
            Slug = FirstNotEmpty(shopInfo.Slug, GenerateSlug(shopInfo.ShopName)),
            Category = category,
            Location = $"{floor}, {locationSlot}",
            Summary = summary,
            Description = description,
            ImageUrl = coverImageUrl,
            Badge = string.IsNullOrWhiteSpace(shopInfo.Badge) ? null : shopInfo.Badge,
            Offer = string.IsNullOrWhiteSpace(shopInfo.Offer) ? null : shopInfo.Offer,
            OpenHours = FirstNotEmpty(shopInfo.OpenHours, "09:30 - 22:00"),
            Tags = BuildTags(shopInfo, rentalArea),
            LogoUrl = shopInfo.LogoUrl,
            CoverImageUrl = coverImageUrl,
            Floor = floor,
            LocationSlot = locationSlot,
            ShopStatus = DeriveShopStatus(shopInfo.OpeningDate),
            OpeningDate = shopInfo.OpeningDate,
            Products = (products ?? [])
                .OrderByDescending(x => x.IsFeatured)
                .ThenBy(x => x.Name)
                .Select(MapProduct)
                .ToList(),
            Vouchers = (vouchers ?? [])
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Title)
                .Select(MapVoucher)
                .ToList()
        };
    }

    public static PublicShopDto Map(
        PublicShop shop,
        IEnumerable<PublicShopProduct>? products = null,
        IEnumerable<PublicShopVoucher>? vouchers = null)
    {
        var coverImageUrl = FirstNotEmpty(shop.CoverImageUrl, DefaultImage);

        return new PublicShopDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Slug = FirstNotEmpty(shop.Slug, GenerateSlug(shop.Name)),
            Category = FirstNotEmpty(shop.Category, "Retail Store"),
            Location = $"{FirstNotEmpty(shop.Floor, "Mall floor")}, {FirstNotEmpty(shop.LocationSlot, "ABCD Mall")}",
            Summary = FirstNotEmpty(shop.Summary, $"{shop.Name} is operating at ABCD Mall."),
            Description = FirstNotEmpty(shop.Description, $"{shop.Name} is managed by the ABCD Mall team."),
            ImageUrl = coverImageUrl,
            Badge = string.IsNullOrWhiteSpace(shop.Badge) ? null : shop.Badge,
            Offer = string.IsNullOrWhiteSpace(shop.Offer) ? null : shop.Offer,
            OpenHours = FirstNotEmpty(shop.OpenHours, "09:30 - 22:00"),
            Tags = BuildTags(shop),
            LogoUrl = shop.LogoUrl,
            CoverImageUrl = coverImageUrl,
            Floor = FirstNotEmpty(shop.Floor, "Mall floor"),
            LocationSlot = FirstNotEmpty(shop.LocationSlot, "ABCD Mall"),
            ShopStatus = FirstNotEmpty(shop.ShopStatus, "Active"),
            OpeningDate = shop.OpeningDate,
            Products = (products ?? [])
                .OrderByDescending(x => x.IsFeatured)
                .ThenBy(x => x.Name)
                .Select(MapProduct)
                .ToList(),
            Vouchers = (vouchers ?? [])
                .OrderByDescending(x => x.IsActive)
                .ThenBy(x => x.Title)
                .Select(MapVoucher)
                .ToList()
        };
    }

    public static string GenerateSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        return Regex.Replace(slug, "-+", "-").Trim('-');
    }

    public static string DeriveShopStatus(DateTime? openingDate)
    {
        if (!openingDate.HasValue)
        {
            return "Active";
        }

        return openingDate.Value.Date > DateTime.UtcNow.Date
            ? "ComingSoon"
            : "Active";
    }

    private static string[] BuildTags(ShopInfo shopInfo, RentalArea? rentalArea)
    {
        var tags = shopInfo.Tags
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (!string.IsNullOrWhiteSpace(rentalArea?.AreaName))
        {
            tags.Add(rentalArea.AreaName);
        }

        if (!string.IsNullOrWhiteSpace(shopInfo.Floor))
        {
            tags.Add(shopInfo.Floor);
        }

        if (!string.IsNullOrWhiteSpace(shopInfo.LocationSlot))
        {
            tags.Add(shopInfo.LocationSlot);
        }

        if (!tags.Any())
        {
            tags.Add("ABCD Mall");
        }

        return tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string[] BuildTags(PublicShop shop)
    {
        var tags = new List<string>();

        if (!string.IsNullOrWhiteSpace(shop.Category))
        {
            tags.Add(shop.Category);
        }

        if (!string.IsNullOrWhiteSpace(shop.Floor))
        {
            tags.Add(shop.Floor);
        }

        if (!string.IsNullOrWhiteSpace(shop.LocationSlot))
        {
            tags.Add(shop.LocationSlot);
        }

        if (!tags.Any())
        {
            tags.Add("ABCD Mall");
        }

        return tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string FirstNotEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

    private static PublicShopProductDto MapProduct(PublicShopProduct product)
        => new()
        {
            Id = product.Id,
            Name = product.Name,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            OldPrice = product.OldPrice,
            DiscountPercent = product.DiscountPercent,
            IsFeatured = product.IsFeatured,
            IsDiscounted = product.IsDiscounted
        };

    private static PublicShopVoucherDto MapVoucher(PublicShopVoucher voucher)
        => new()
        {
            Id = voucher.Id,
            Code = voucher.Code,
            Title = voucher.Title,
            Description = voucher.Description,
            ValidUntil = voucher.ValidUntil,
            IsActive = voucher.IsActive
        };
}
