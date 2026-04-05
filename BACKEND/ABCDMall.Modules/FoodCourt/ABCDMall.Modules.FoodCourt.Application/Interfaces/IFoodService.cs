using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;

public interface IFoodService
{
    Task<List<FoodItemDto>> GetAllAsync();
    Task<FoodItemDto?> GetByIdAsync(string id);
    Task<FoodItemDto?> GetBySlugAsync(string slug);
    Task CreateAsync(FoodItem item);
    Task<bool> UpdateAsync(string id, FoodItemDto dto);
    Task<bool> DeleteAsync(string id);
    Task<List<FoodItemDto>> SearchAsync(string keyword);
}