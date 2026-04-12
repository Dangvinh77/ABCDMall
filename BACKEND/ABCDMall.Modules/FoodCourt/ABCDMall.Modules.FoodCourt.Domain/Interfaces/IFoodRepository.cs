using ABCDMall.Modules.FoodCourt.Domain.Entities;
namespace ABCDMall.Modules.FoodCourt.Domain.Interfaces;

public interface IFoodRepository
{
    Task<List<FoodItem>> GetAllAsync();
    Task CreateAsync(FoodItem item);
    Task<FoodItem?> GetByIdAsync(int id);
    Task<FoodItem?> GetBySlugAsync(string slug); 
    Task UpdateAsync(int id, FoodItem item);
    Task DeleteAsync(int id);
}