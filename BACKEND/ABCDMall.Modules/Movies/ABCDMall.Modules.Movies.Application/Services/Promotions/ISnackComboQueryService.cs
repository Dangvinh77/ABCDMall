using ABCDMall.Modules.Movies.Application.DTOs.Promotions;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public interface ISnackComboQueryService
{
    Task<IReadOnlyList<SnackComboResponseDto>> GetSnackCombosAsync(CancellationToken cancellationToken = default);
}
