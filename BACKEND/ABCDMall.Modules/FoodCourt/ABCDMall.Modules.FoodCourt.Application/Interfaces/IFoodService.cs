using ABCDMall.Shared.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;

namespace ABCDMall.Modules.FoodCourt.Application.Interfaces;

public interface IFoodService
{
    Task<List<FoodItemDto>> GetAllAsync();
    Task<FoodItemDto?> GetByIdAsync(int id);
    Task<FoodItemDto?> GetBySlugAsync(string slug);
    Task CreateAsync(FoodItem item);
    Task<bool> UpdateAsync(int id, FoodItemDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<FoodItemDto>> SearchAsync(string keyword);
}