using ABCDMall.Modules.FoodCourt.Application.DTOs;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace ABCDMall.Modules.FoodCourt.Application.Services.Foods;

public sealed class FoodQueryService : IFoodQueryService
{
    private readonly IFoodRepository _foodRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FoodQueryService> _logger;

    public FoodQueryService(
        IFoodRepository foodRepository,
        IMapper mapper,
        ILogger<FoodQueryService> logger)
    {
        _foodRepository = foodRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FoodItemDto>> GetListAsync(string? keyword = null, CancellationToken cancellationToken = default)
    {
        var foods = await _foodRepository.GetFoodsAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            foods = foods
                .Where(food => !string.IsNullOrWhiteSpace(food.Name) &&
                               food.Name.Contains(keyword.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        _logger.LogInformation(
            "Fetched {FoodCount} foods using keyword filter {KeywordFilter}.",
            foods.Count,
            string.IsNullOrWhiteSpace(keyword) ? "all" : keyword.Trim());

        return _mapper.Map<IReadOnlyList<FoodItemDto>>(foods);
    }

    public async Task<FoodItemDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var food = await _foodRepository.GetFoodByIdAsync(id, cancellationToken);
        if (food is null)
        {
            _logger.LogWarning("Food {FoodId} was not found.", id);
            return null;
        }

        return _mapper.Map<FoodItemDto>(food);
    }

    public async Task<FoodItemDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var food = await _foodRepository.GetFoodBySlugAsync(slug, cancellationToken);
        if (food is null)
        {
            _logger.LogWarning("Food with slug {FoodSlug} was not found.", slug);
            return null;
        }

        return _mapper.Map<FoodItemDto>(food);
    }
}

