using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public interface IFoodQueryService
{
    Task<IReadOnlyList<FoodItemDto>> GetListAsync(string? keyword = null, CancellationToken cancellationToken = default);
    Task<FoodDetailDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<FoodDetailDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}

