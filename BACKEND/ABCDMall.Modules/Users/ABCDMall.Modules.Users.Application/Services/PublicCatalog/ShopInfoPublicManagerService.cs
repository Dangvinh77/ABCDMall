using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Domain.Entities;
using System.Text.Json;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public sealed class ShopInfoPublicManagerService : IShopInfoPublicManagerService
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IShopInfoPublicManagerRepository _repository;
    private readonly IFileStorageService _fileStorageService;

    public ShopInfoPublicManagerService(
        IShopInfoPublicManagerRepository repository,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<PublicShopDto>> GetMyShopsAsync(string shopId, CancellationToken cancellationToken = default)
    {
        var shopInfos = await _repository.GetManagedShopInfosAsync(shopId, cancellationToken);
        var shops = new List<PublicShopDto>();

        foreach (var shopInfo in shopInfos.Where(shopInfo => shopInfo.IsPublicVisible))
        {
            shops.Add(await MapWithCatalogItemsAsync(shopInfo, cancellationToken));
        }

        return shops.OrderBy(shop => shop.Name).ToList();
    }

    public async Task<ShopCreationStatusDto> GetCreationStatusAsync(string shopId, CancellationToken cancellationToken = default)
    {
        var shopInfo = await _repository.GetShopInfoByIdAsync(shopId, cancellationToken)
            ?? throw new InvalidOperationException("Manager account does not have shop information.");

        var shopCount = await _repository.CountManagedPublicShopsAsync(shopId, cancellationToken);

        if (shopCount == 0 && shopInfo.IsPublicVisible && !string.IsNullOrWhiteSpace(shopInfo.Slug))
        {
            shopCount = 1;
        }

        var rentedAreaCount = await _repository.CountRentedAreasAsync(shopId, shopInfo.ShopName, cancellationToken);
        var canCreate = shopCount < rentedAreaCount;

        var availableLocations = await _repository.GetRentedAreaLocationsAsync(shopId, shopInfo.ShopName, cancellationToken);

        string message;
        if (rentedAreaCount == 0)
        {
            message = "You have not registered any rental area (tenant) yet. Please register a tenant first to be eligible for creating shop pages.";
        }
        else if (canCreate)
        {
            message = $"You can create {rentedAreaCount - shopCount} more shop page(s).";
        }
        else
        {
            message = "You cannot create a new shop because the number of shop pages is equal to the number of rented areas.";
        }

        return new ShopCreationStatusDto
        {
            ShopCount = shopCount,
            RentedAreaCount = rentedAreaCount,
            CanCreate = canCreate,
            AvailableRentalLocations = availableLocations.Select(x => new AvailableRentalLocationDto
            {
                LocationSlot = x.LocationSlot,
                Floor = x.Floor,
                AreaName = x.ShopName
            }).ToList(),
            Message = message
        };
    }

    public async Task<PublicShopDto> CreateMyShopAsync(string shopId, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken = default)
    {
        var creationStatus = await GetCreationStatusAsync(shopId, cancellationToken);
        if (!creationStatus.CanCreate)
        {
            throw new InvalidOperationException(creationStatus.Message);
        }

        var ownerShopInfo = await _repository.GetShopInfoByIdAsync(shopId, cancellationToken)
            ?? throw new InvalidOperationException("Manager account does not have shop information.");

        var shopInfo = new ShopInfo
        {
            Id = Guid.NewGuid().ToString("N"),
            OwnerShopInfoId = shopId,
            ManagerName = ownerShopInfo.ManagerName,
            RentalLocation = ownerShopInfo.RentalLocation,
            Month = ownerShopInfo.Month,
            LeaseStartDate = ownerShopInfo.LeaseStartDate,
            ElectricityFee = ownerShopInfo.ElectricityFee,
            WaterFee = ownerShopInfo.WaterFee,
            ServiceFee = ownerShopInfo.ServiceFee,
            LeaseTermDays = ownerShopInfo.LeaseTermDays,
            ContractImage = ownerShopInfo.ContractImage,
            ContractImages = ownerShopInfo.ContractImages
        };

        await ApplyRequestAsync(shopInfo, request, cancellationToken);
        shopInfo.IsPublicVisible = true;
        await _repository.AddShopInfoAsync(shopInfo, cancellationToken);
        await SyncCatalogShopAsync(shopInfo, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await SyncProductsAsync(shopInfo, request, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await MapWithCatalogItemsAsync(shopInfo, cancellationToken);
    }

    public async Task<PublicShopDto?> UpdateMyShopAsync(string shopId, string requestedShopId, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken = default)
    {
        var shopInfo = await _repository.GetManagedShopInfoByIdAsync(shopId, requestedShopId, cancellationToken);
        if (shopInfo is null || !shopInfo.IsPublicVisible)
        {
            return null;
        }

        await ApplyRequestAsync(shopInfo, request, cancellationToken);
        await SyncCatalogShopAsync(shopInfo, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await SyncProductsAsync(shopInfo, request, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return await MapWithCatalogItemsAsync(shopInfo, cancellationToken);
    }

    public async Task<bool> DeleteMyShopAsync(string shopId, string requestedShopId, CancellationToken cancellationToken = default)
    {
        var shopInfo = await _repository.GetManagedShopInfoByIdAsync(shopId, requestedShopId, cancellationToken);
        if (shopInfo is null || !shopInfo.IsPublicVisible)
        {
            return false;
        }

        shopInfo.IsPublicVisible = false;
        await _repository.ReplaceProductsAsync([shopInfo.Id ?? string.Empty], shopInfo.Id ?? string.Empty, [], cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ApplyRequestAsync(ShopInfo shopInfo, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken)
    {
        NormalizeRequest(request);

        if (await _repository.ExistsVisibleSlugAsync(request.Slug, shopInfo.Id, cancellationToken))
        {
            throw new InvalidOperationException("Shop slug already exists.");
        }

        shopInfo.ShopName = request.Name;
        shopInfo.Slug = request.Slug;
        shopInfo.Category = request.Category;
        shopInfo.Floor = request.Floor;
        shopInfo.LocationSlot = request.LocationSlot;
        shopInfo.RentalLocation = request.LocationSlot;
        shopInfo.Summary = request.Summary;
        shopInfo.Description = request.Description;
        shopInfo.LogoUrl = await ResolveLogoUrlAsync(shopInfo, request, cancellationToken);
        shopInfo.CoverImageUrl = await ResolveCoverImageUrlAsync(shopInfo, request, cancellationToken);
        shopInfo.OpenHours = request.OpenHours;
        shopInfo.Badge = request.Badge;
        shopInfo.Offer = request.Offer;
        shopInfo.OpeningDate = request.OpeningDate;
        shopInfo.Tags = string.Join(", ", request.Tags);
    }

    private static void NormalizeRequest(UpsertShopInfoPublicRequestDto request)
    {
        request.Name = Require(request.Name, "Shop name");
        request.Slug = ShopInfoPublicMapper.GenerateSlug(Require(request.Slug, "Slug"));
        request.Category = Require(request.Category, "Category");
        request.Floor = Require(request.Floor, "Floor");
        request.LocationSlot = Require(request.LocationSlot, "Location");
        request.Summary = Require(request.Summary, "Summary");
        request.Description = Require(request.Description, "Description");
        request.LogoUrl = request.LogoUrl?.Trim() ?? string.Empty;
        request.CoverImageUrl = request.CoverImageUrl?.Trim() ?? string.Empty;
        request.OpenHours = string.IsNullOrWhiteSpace(request.OpenHours) ? "09:30 - 22:00" : request.OpenHours.Trim();
        request.Badge = string.IsNullOrWhiteSpace(request.Badge) ? null : request.Badge.Trim();
        request.Offer = string.IsNullOrWhiteSpace(request.Offer) ? null : request.Offer.Trim();
        request.Tags = request.Tags.Select(x => x.Trim()).Where(x => x.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private async Task SyncCatalogShopAsync(ShopInfo shopInfo, CancellationToken cancellationToken)
    {
        var shopId = Require(shopInfo.Id ?? string.Empty, "Shop id");
        await _repository.UpsertCatalogShopAsync(new PublicShop
        {
            Id = shopId,
            OwnerShopId = shopInfo.OwnerShopInfoId ?? shopId,
            Name = shopInfo.ShopName,
            Slug = shopInfo.Slug,
            Category = shopInfo.Category,
            Floor = shopInfo.Floor,
            LocationSlot = shopInfo.LocationSlot,
            Summary = shopInfo.Summary,
            Description = shopInfo.Description,
            LogoUrl = shopInfo.LogoUrl,
            CoverImageUrl = shopInfo.CoverImageUrl,
            OpenHours = shopInfo.OpenHours,
            Badge = shopInfo.Badge,
            Offer = shopInfo.Offer,
            ShopStatus = ShopInfoPublicMapper.DeriveShopStatus(shopInfo.OpeningDate),
            OpeningDate = shopInfo.OpeningDate
        }, cancellationToken);
    }

    private async Task SyncProductsAsync(ShopInfo shopInfo, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken)
    {
        var shopId = Require(shopInfo.Id ?? string.Empty, "Shop id");
        var legacyShopId = string.IsNullOrWhiteSpace(shopInfo.Slug) ? string.Empty : $"shop-{shopInfo.Slug}";

        await SyncProductsAsync(shopId, request, cancellationToken, legacyShopId);
    }

    private async Task SyncProductsAsync(string shopId, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken, string? legacyShopId = null)
    {
        var products = ParseProducts(request.ProductsJson);
        var mappedProducts = new List<PublicShopProduct>();

        for (var index = 0; index < products.Count; index++)
        {
            var product = products[index];
            product.Name = Require(product.Name, $"Product #{index + 1} name");

            if (product.Price <= 0)
            {
                throw new InvalidOperationException($"Product #{index + 1} price must be greater than 0.");
            }

            if (product.OldPrice is <= 0)
            {
                product.OldPrice = null;
            }

            if (product.DiscountPercent is <= 0)
            {
                product.DiscountPercent = null;
            }

            var imageUrl = await ResolveProductImageUrlAsync(product, request, index, cancellationToken);

            mappedProducts.Add(new PublicShopProduct
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = product.Name,
                ImageUrl = imageUrl,
                Price = product.Price,
                OldPrice = product.OldPrice,
                DiscountPercent = product.DiscountPercent,
                IsFeatured = product.IsFeatured,
                IsDiscounted = product.IsDiscounted
            });
        }

        await _repository.ReplaceProductsAsync([shopId, legacyShopId ?? string.Empty], shopId, mappedProducts, cancellationToken);
    }

    private static IReadOnlyList<UpsertShopProductRequestDto> ParseProducts(string? productsJson)
    {
        if (string.IsNullOrWhiteSpace(productsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<UpsertShopProductRequestDto>>(productsJson, JsonSerializerOptions) ?? [];
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Featured products data is invalid.");
        }
    }

    private async Task<string> ResolveProductImageUrlAsync(
        UpsertShopProductRequestDto product,
        UpsertShopInfoPublicRequestDto request,
        int productIndex,
        CancellationToken cancellationToken)
    {
        if (product.ImageFileIndex is not null)
        {
            if (product.ImageFileIndex < 0 || product.ImageFileIndex >= request.ProductImages.Count)
            {
                throw new InvalidOperationException($"Product #{productIndex + 1} image is invalid.");
            }

            return await _fileStorageService.SaveShopProductImageAsync(request.ProductImages[product.ImageFileIndex.Value], cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(product.ImageUrl))
        {
            return product.ImageUrl.Trim();
        }

        throw new InvalidOperationException($"Product #{productIndex + 1} image is required.");
    }

    private async Task<string> ResolveLogoUrlAsync(ShopInfo shopInfo, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken)
        => await ResolveLogoUrlAsync(shopInfo.LogoUrl, request, cancellationToken);

    private async Task<string> ResolveLogoUrlAsync(string? currentLogoUrl, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken)
    {
        if (request.LogoImage is not null)
        {
            return await _fileStorageService.SaveShopLogoAsync(request.LogoImage, cancellationToken);
        }

        return !string.IsNullOrWhiteSpace(request.LogoUrl)
            ? request.LogoUrl
            : currentLogoUrl ?? string.Empty;
    }

    private async Task<string> ResolveCoverImageUrlAsync(ShopInfo shopInfo, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken)
        => await ResolveCoverImageUrlAsync(shopInfo.CoverImageUrl, request, cancellationToken);

    private async Task<string> ResolveCoverImageUrlAsync(string? currentCoverImageUrl, UpsertShopInfoPublicRequestDto request, CancellationToken cancellationToken)
    {
        if (request.CoverImage is not null)
        {
            return await _fileStorageService.SaveShopCoverAsync(request.CoverImage, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.CoverImageUrl))
        {
            return request.CoverImageUrl;
        }

        if (!string.IsNullOrWhiteSpace(currentCoverImageUrl))
        {
            return currentCoverImageUrl;
        }

        throw new InvalidOperationException("Cover image is required.");
    }

    private static string Require(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private async Task<PublicShopDto> MapWithCatalogItemsAsync(ShopInfo shopInfo, CancellationToken cancellationToken)
    {
        var shopId = shopInfo.Id ?? string.Empty;
        var products = await _repository.GetProductsAsync(shopId, cancellationToken);
        var vouchers = await _repository.GetVouchersAsync(shopId, cancellationToken);

        if (!products.Any() && !string.IsNullOrWhiteSpace(shopInfo.Slug))
        {
            products = await _repository.GetProductsAsync($"shop-{shopInfo.Slug}", cancellationToken);
        }

        if (!vouchers.Any() && !string.IsNullOrWhiteSpace(shopInfo.Slug))
        {
            vouchers = await _repository.GetVouchersAsync($"shop-{shopInfo.Slug}", cancellationToken);
        }

        return ShopInfoPublicMapper.Map(shopInfo, products: products, vouchers: vouchers);
    }

    private async Task<PublicShopDto> MapWithCatalogItemsAsync(PublicShop shop, CancellationToken cancellationToken)
    {
        var products = await _repository.GetProductsAsync(shop.Id, cancellationToken);
        var vouchers = await _repository.GetVouchersAsync(shop.Id, cancellationToken);

        return ShopInfoPublicMapper.Map(shop, products: products, vouchers: vouchers);
    }
}
