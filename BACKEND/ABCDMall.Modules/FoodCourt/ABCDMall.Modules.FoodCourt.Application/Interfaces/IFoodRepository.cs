using ABCDMall.Modules.FoodCourt.Domain.Entities;

namespace ABCDMall.Modules.FoodCourt.Application.Interfaces;

public interface IFoodRepository
{
    Task<List<FoodItem>> GetAllAsync();
    Task CreateAsync(FoodItem item);
    Task<FoodItem?> GetBySlugAsync(string slug);
}