using System.Text.Json;
using AutoMapper;
using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Helpers;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public sealed class FoodManagerService : IFoodManagerService
{
    private const string QuotaReachedMessage = "All rented food-court slots already have a managed stall.";

    private readonly IFoodRepository _foodRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FoodManagerService> _logger;

    public FoodManagerService(
        IFoodRepository foodRepository,
        IMapper mapper,
        ILogger<FoodManagerService> logger)
    {
        _foodRepository = foodRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FoodDetailDto>> GetMyFoodStallsAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        var stalls = await _foodRepository.GetManagedFoodStallsAsync(ownerShopId, cancellationToken);
        return _mapper.Map<IReadOnlyList<FoodDetailDto>>(stalls);
    }

    public async Task<FoodManagerCreationStatusDto> GetCreationStatusAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        var rentedFoodCourtCount = await _foodRepository.CountFoodCourtRentalsAsync(ownerShopId, cancellationToken);
        var stallCount = await _foodRepository.CountManagedStallsAsync(ownerShopId, cancellationToken);
        var availableLocations = await _foodRepository.GetAvailableFoodCourtLocationsAsync(ownerShopId, cancellationToken);
        var canCreate = stallCount < rentedFoodCourtCount;

        return new FoodManagerCreationStatusDto
        {
            StallCount = stallCount,
            RentedFoodCourtCount = rentedFoodCourtCount,
            CanCreate = canCreate,
            Message = canCreate
                ? "You can create a food stall for an available food-court rental."
                : QuotaReachedMessage,
            AvailableRentalLocations = availableLocations
        };
    }

    public async Task<FoodDetailDto> CreateMyFoodStallAsync(string ownerShopId, UpsertFoodManagerRequestDto request, CancellationToken cancellationToken = default)
    {
        var rentedFoodCourtCount = await _foodRepository.CountFoodCourtRentalsAsync(ownerShopId, cancellationToken);
        var stallCount = await _foodRepository.CountManagedStallsAsync(ownerShopId, cancellationToken);
        if (stallCount >= rentedFoodCourtCount)
        {
            throw new InvalidOperationException(QuotaReachedMessage);
        }

        var slug = SlugHelper.GenerateSlug(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);
        if (await _foodRepository.SlugExistsAsync(slug, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("A food stall with the same slug already exists.");
        }

        var item = BuildFoodItem(ownerShopId, request, slug);
        await _foodRepository.CreateFoodAsync(item, cancellationToken);

        _logger.LogInformation("Created managed food stall {FoodId} for owner {OwnerShopId}.", item.Id, ownerShopId);
        return _mapper.Map<FoodDetailDto>(item);
    }

    public async Task<FoodDetailDto?> UpdateMyFoodStallAsync(string ownerShopId, string foodId, UpsertFoodManagerRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetManagedFoodStallByIdAsync(ownerShopId, foodId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var slug = SlugHelper.GenerateSlug(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);
        if (await _foodRepository.SlugExistsAsync(slug, existing.Id, cancellationToken))
        {
            throw new InvalidOperationException("A food stall with the same slug already exists.");
        }

        existing.Name = request.Name.Trim();
        existing.Slug = slug;
        existing.Description = request.Description?.Trim() ?? string.Empty;
        existing.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        existing.CategorySlug = request.CategorySlug.Trim();
        existing.Location = request.Location.Trim();
        existing.OpenHours = request.OpenHours?.Trim() ?? string.Empty;
        existing.Phone = request.Phone?.Trim() ?? string.Empty;
        existing.Promo = request.Promo?.Trim() ?? string.Empty;
        existing.IsActive = request.IsActive;
        existing.MenuItems = BuildMenuItems(existing.Id, request.MenuItems);

        await _foodRepository.UpdateFoodAsync(foodId, existing, cancellationToken);

        _logger.LogInformation("Updated managed food stall {FoodId} for owner {OwnerShopId}.", foodId, ownerShopId);
        return _mapper.Map<FoodDetailDto>(existing);
    }

    public async Task<bool> DeleteMyFoodStallAsync(string ownerShopId, string foodId, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetManagedFoodStallByIdAsync(ownerShopId, foodId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _foodRepository.DeleteFoodAsync(foodId, cancellationToken);
        _logger.LogInformation("Deleted managed food stall {FoodId} for owner {OwnerShopId}.", foodId, ownerShopId);
        return true;
    }

    public async Task<FoodMenuItemDto?> AddMenuItemAsync(string ownerShopId, string foodId, UpsertFoodMenuItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetManagedFoodStallByIdAsync(ownerShopId, foodId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var menuItem = BuildMenuItem(foodId, request, existing.MenuItems.Count);
        existing.MenuItems.Add(menuItem);

        await _foodRepository.UpdateFoodAsync(foodId, existing, cancellationToken);
        _logger.LogInformation("Added menu item {MenuItemId} to food stall {FoodId}.", menuItem.Id, foodId);
        return _mapper.Map<FoodMenuItemDto>(menuItem);
    }

    public async Task<FoodMenuItemDto?> UpdateMenuItemAsync(string ownerShopId, string foodId, string menuItemId, UpsertFoodMenuItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetManagedFoodStallByIdAsync(ownerShopId, foodId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var menuItem = existing.MenuItems.FirstOrDefault(item => item.Id == menuItemId);
        if (menuItem is null)
        {
            return null;
        }

        menuItem.Name = request.Name.Trim();
        menuItem.Price = request.Price;
        menuItem.Note = request.Note?.Trim() ?? string.Empty;
        menuItem.Tag = request.Tag?.Trim() ?? string.Empty;
        menuItem.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        menuItem.IngredientsJson = SerializeIngredients(request.Ingredients);
        menuItem.IsAvailable = request.IsAvailable;
        menuItem.DisplayOrder = request.DisplayOrder;

        await _foodRepository.UpdateFoodAsync(foodId, existing, cancellationToken);
        _logger.LogInformation("Updated menu item {MenuItemId} for food stall {FoodId}.", menuItemId, foodId);
        return _mapper.Map<FoodMenuItemDto>(menuItem);
    }

    public async Task<bool> DeleteMenuItemAsync(string ownerShopId, string foodId, string menuItemId, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetManagedFoodStallByIdAsync(ownerShopId, foodId, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        var menuItem = existing.MenuItems.FirstOrDefault(item => item.Id == menuItemId);
        if (menuItem is null)
        {
            return false;
        }

        existing.MenuItems.Remove(menuItem);
        await _foodRepository.UpdateFoodAsync(foodId, existing, cancellationToken);
        _logger.LogInformation("Deleted menu item {MenuItemId} from food stall {FoodId}.", menuItemId, foodId);
        return true;
    }

    private static FoodItem BuildFoodItem(string ownerShopId, UpsertFoodManagerRequestDto request, string slug)
    {
        var foodId = Guid.NewGuid().ToString("N");

        return new FoodItem
        {
            Id = foodId,
            OwnerShopId = ownerShopId,
            Name = request.Name.Trim(),
            Slug = slug,
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            CategorySlug = request.CategorySlug.Trim(),
            Location = request.Location.Trim(),
            OpenHours = request.OpenHours?.Trim() ?? string.Empty,
            Phone = request.Phone?.Trim() ?? string.Empty,
            Promo = request.Promo?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            MenuItems = BuildMenuItems(foodId, request.MenuItems)
        };
    }

    private static List<FoodMenuItem> BuildMenuItems(string foodId, IReadOnlyList<UpsertFoodMenuItemRequestDto> requests)
    {
        return requests
            .Select((request, index) => BuildMenuItem(foodId, request, index))
            .ToList();
    }

    private static FoodMenuItem BuildMenuItem(string foodId, UpsertFoodMenuItemRequestDto request, int index)
    {
        return new FoodMenuItem
        {
            Id = string.IsNullOrWhiteSpace(request.Id) ? Guid.NewGuid().ToString("N") : request.Id.Trim(),
            FoodStallId = foodId,
            Name = request.Name.Trim(),
            Price = request.Price,
            Note = request.Note?.Trim() ?? string.Empty,
            Tag = request.Tag?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            IngredientsJson = SerializeIngredients(request.Ingredients),
            IsAvailable = request.IsAvailable,
            DisplayOrder = request.DisplayOrder == 0 ? index + 1 : request.DisplayOrder
        };
    }

    private static string SerializeIngredients(IReadOnlyList<string> ingredients)
    {
        var normalized = ingredients
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .ToList();

        return JsonSerializer.Serialize(normalized);
    }
}
