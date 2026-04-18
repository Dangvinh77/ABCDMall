using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public interface IFoodCommandService
{
    Task CreateAsync(CreateFoodRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(string id, UpdateFoodRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
