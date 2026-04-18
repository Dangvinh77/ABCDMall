using ABCDMall.Modules.FoodCourt.Domain.Entities;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public interface IFoodRepository
{
    Task<IReadOnlyList<FoodItem>> GetFoodsAsync(CancellationToken cancellationToken = default);
    Task<FoodItem?> GetFoodByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<FoodItem?> GetFoodBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task CreateFoodAsync(FoodItem item, CancellationToken cancellationToken = default);
    Task UpdateFoodAsync(string id, FoodItem item, CancellationToken cancellationToken = default);
    Task DeleteFoodAsync(string id, CancellationToken cancellationToken = default);
}

