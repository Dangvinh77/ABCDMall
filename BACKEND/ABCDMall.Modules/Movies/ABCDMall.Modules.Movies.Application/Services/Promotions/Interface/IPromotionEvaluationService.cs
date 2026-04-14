using ABCDMall.Modules.Movies.Application.DTOs.Promotions;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions;

public interface IPromotionEvaluationService
{
    Task<EvaluatePromotionResponseDto> EvaluateAsync(EvaluatePromotionRequestDto request, CancellationToken cancellationToken = default);
}
