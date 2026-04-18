using ABCDMall.Modules.FoodCourt.Application.DTOs;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public interface IFoodQueryService
{
    Task<IReadOnlyList<FoodItemDto>> GetListAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<FoodItemDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<FoodItemDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

