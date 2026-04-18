using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Helpers;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public sealed class FoodCommandService : IFoodCommandService
{
    private readonly IFoodRepository _foodRepository;
    private readonly ILogger<FoodCommandService> _logger;

    public FoodCommandService(
        IFoodRepository foodRepository,
        ILogger<FoodCommandService> logger)
    {
        _foodRepository = foodRepository;
        _logger = logger;
    }

    public async Task CreateAsync(CreateFoodRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = new FoodItem
        {
            Id = string.IsNullOrWhiteSpace(request.Id) ? Guid.NewGuid().ToString("N") : request.Id.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ImageUrl = request.ImageUrl?.Trim() ?? string.Empty,
            Slug = SlugHelper.GenerateSlug(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug)
        };

        await _foodRepository.CreateFoodAsync(item, cancellationToken);

        _logger.LogInformation("Created food item {FoodName} with slug {FoodSlug}.", item.Name, item.Slug);
    }

    public async Task<bool> UpdateAsync(string id, UpdateFoodRequestDto request, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetFoodByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger.LogWarning("Cannot update food {FoodId} because it does not exist.", id);
            return false;
        }

        existing.Name = request.Name.Trim();
        existing.Description = request.Description?.Trim() ?? string.Empty;
        existing.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
        existing.Slug = SlugHelper.GenerateSlug(string.IsNullOrWhiteSpace(request.Slug) ? request.Name : request.Slug);

        await _foodRepository.UpdateFoodAsync(id, existing, cancellationToken);
        _logger.LogInformation("Updated food item {FoodId} with slug {FoodSlug}.", id, existing.Slug);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await _foodRepository.GetFoodByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            _logger.LogWarning("Cannot delete food {FoodId} because it does not exist.", id);
            return false;
        }

        await _foodRepository.DeleteFoodAsync(id, cancellationToken);
        _logger.LogInformation("Deleted food item {FoodId}.", id);
        return true;
    }
}
