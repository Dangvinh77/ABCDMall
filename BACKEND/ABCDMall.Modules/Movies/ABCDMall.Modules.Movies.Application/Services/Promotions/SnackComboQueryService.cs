using ABCDMall.Modules.Movies.Application.DTOs.Promotions;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public sealed class SnackComboQueryService : ISnackComboQueryService
{
    private readonly IPromotionRepository _promotionRepository;

    public SnackComboQueryService(IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<IReadOnlyList<SnackComboResponseDto>> GetSnackCombosAsync(CancellationToken cancellationToken = default)
    {
        // Day 3 endpoint nay giup frontend bo mock combo data va lay comboId dung de evaluate promo.
        var combos = await _promotionRepository.GetSnackCombosAsync(cancellationToken);

        return combos
            .OrderBy(combo => combo.Name)
            .Select(combo => new SnackComboResponseDto
            {
                Id = combo.Id,
                Code = combo.Code,
                Name = combo.Name,
                Description = combo.Description,
                Price = combo.Price,
                ImageUrl = combo.ImageUrl,
                IsActive = combo.IsActive
            })
            .ToList();
    }
}
