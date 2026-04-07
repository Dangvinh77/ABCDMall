using ABCDMall.Modules.FoodCourt.Domain.Entities;

namespace ABCDMall.Modules.FoodCourt.Application.Interfaces;

public interface IFoodRepository
{
    Task<List<FoodItem>> GetAllAsync();
    Task CreateAsync(FoodItem item);
    Task<FoodItem?> GetByIdAsync(string id);
    Task<FoodItem?> GetBySlugAsync(string slug);
    Task UpdateAsync(string id, FoodItem item);
    Task DeleteAsync(string id);
}