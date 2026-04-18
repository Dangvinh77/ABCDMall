using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Domain.Entities;
using System.Text.RegularExpressions;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public sealed class PublicShopCatalogService : IPublicShopCatalogService
{
    private const string DefaultImage =
        "https://images.unsplash.com/photo-1483985988355-763728e1935b?q=80&w=1600&auto=format&fit=crop";

    private readonly IPublicShopCatalogReadRepository _readRepository;

    public PublicShopCatalogService(IPublicShopCatalogReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<IReadOnlyList<PublicShopDto>> GetShopsAsync(CancellationToken cancellationToken = default)
    {
        var shopInfos = await _readRepository.GetShopInfosAsync(cancellationToken);
        var rentalAreas = await _readRepository.GetRentalAreasAsync(cancellationToken);

        return shopInfos
            .Select(shopInfo => MapShop(shopInfo, FindRentalArea(shopInfo, rentalAreas)))
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

    private static PublicShopDto MapShop(ShopInfo shopInfo, RentalArea? rentalArea)
    {
        var location = string.IsNullOrWhiteSpace(shopInfo.RentalLocation)
            ? "ABCD Mall"
            : $"Area {shopInfo.RentalLocation}";
        var category = rentalArea?.AreaName ?? "Retail Store";
        var floor = string.IsNullOrWhiteSpace(rentalArea?.Floor) ? "Mall floor" : $"Floor {rentalArea!.Floor}";
        var leaseInfo = shopInfo.LeaseTermDays > 0 ? $"{shopInfo.LeaseTermDays} day lease term" : "Flexible lease term";
        var utilityInfo =
            shopInfo.ElectricityFee > 0 || shopInfo.WaterFee > 0 || shopInfo.ServiceFee > 0
                ? $"Current monthly service profile includes electricity, water, and service fees totaling {shopInfo.TotalDue:N0}."
                : "This shop is newly added and commercial details are being updated.";

        return new PublicShopDto
        {
            Id = shopInfo.Id ?? string.Empty,
            Name = shopInfo.ShopName,
            Slug = GenerateSlug(shopInfo.ShopName),
            Category = category,
            Location = $"{location}, {floor}",
            Summary = $"{shopInfo.ShopName} is operating at {location} in the {category.ToLowerInvariant()} zone.",
            Description =
                $"{shopInfo.ShopName} is managed by {shopInfo.ManagerName ?? "the ABCD Mall team"} and is positioned at {location}. " +
                $"{leaseInfo}. {utilityInfo}",
            ImageUrl = string.IsNullOrWhiteSpace(shopInfo.ContractImage) ? DefaultImage : shopInfo.ContractImage!,
            Badge = rentalArea?.Status == "Rented" ? "Featured Store" : "New Listing",
            Offer = shopInfo.TotalDue > 0 ? $"Estimated monthly due: {shopInfo.TotalDue:N0}" : "Contact mall support for leasing details.",
            OpenHours = "09:00 - 22:00",
            Tags = BuildTags(shopInfo, rentalArea)
        };
    }

    private static string[] BuildTags(ShopInfo shopInfo, RentalArea? rentalArea)
    {
        var tags = new List<string>();

        if (!string.IsNullOrWhiteSpace(rentalArea?.AreaName))
        {
            tags.Add(rentalArea.AreaName);
        }

        if (!string.IsNullOrWhiteSpace(rentalArea?.Floor))
        {
            tags.Add($"Floor {rentalArea.Floor}");
        }

        if (!string.IsNullOrWhiteSpace(shopInfo.RentalLocation))
        {
            tags.Add(shopInfo.RentalLocation);
        }

        if (shopInfo.LeaseTermDays > 0)
        {
            tags.Add($"{shopInfo.LeaseTermDays} days");
        }

        if (!tags.Any())
        {
            tags.Add("ABCD Mall");
        }

        return tags.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string GenerateSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", string.Empty);
        slug = Regex.Replace(slug, @"\s+", "-");
        return slug;
    }
}

