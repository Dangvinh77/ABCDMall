using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;

public interface IFoodService
{
    Task<List<FoodItemDto>> GetAllAsync();
    Task CreateAsync(FoodItem item); 
    Task<FoodItemDto?> GetBySlugAsync(string slug);
}