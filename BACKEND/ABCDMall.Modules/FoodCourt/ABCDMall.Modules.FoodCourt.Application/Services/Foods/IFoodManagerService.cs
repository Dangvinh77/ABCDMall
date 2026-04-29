using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public interface IFoodManagerService
{
    Task<IReadOnlyList<FoodDetailDto>> GetMyFoodStallsAsync(string ownerShopId, CancellationToken cancellationToken = default);
    Task<FoodManagerCreationStatusDto> GetCreationStatusAsync(string ownerShopId, CancellationToken cancellationToken = default);
    Task<FoodDetailDto> CreateMyFoodStallAsync(string ownerShopId, UpsertFoodManagerRequestDto request, CancellationToken cancellationToken = default);
    Task<FoodDetailDto?> UpdateMyFoodStallAsync(string ownerShopId, string foodId, UpsertFoodManagerRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMyFoodStallAsync(string ownerShopId, string foodId, CancellationToken cancellationToken = default);
    Task<FoodMenuItemDto?> AddMenuItemAsync(string ownerShopId, string foodId, UpsertFoodMenuItemRequestDto request, CancellationToken cancellationToken = default);
    Task<FoodMenuItemDto?> UpdateMenuItemAsync(string ownerShopId, string foodId, string menuItemId, UpsertFoodMenuItemRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMenuItemAsync(string ownerShopId, string foodId, string menuItemId, CancellationToken cancellationToken = default);
}
