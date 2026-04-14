using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/movies-promotions")]
public sealed class MoviesPromotionsController : ControllerBase
{
    private readonly IPromotionQueryService _promotionQueryService;
    private readonly IPromotionEvaluationService _promotionEvaluationService;
    private readonly IValidator<EvaluatePromotionRequestDto> _evaluateValidator;

    public MoviesPromotionsController(
        IPromotionQueryService promotionQueryService,
        IPromotionEvaluationService promotionEvaluationService,
        IValidator<EvaluatePromotionRequestDto> evaluateValidator)
    {
        _promotionQueryService = promotionQueryService;
        _promotionEvaluationService = promotionEvaluationService;
        _evaluateValidator = evaluateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromotionResponseDto>>> GetPromotions(
        [FromQuery] string? category,
        [FromQuery] bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        // category support giup frontend lay dung nhom promotion can render:
        // ticket, combo, member, bank, weekend, all.
        var promotions = await _promotionQueryService.GetPromotionsAsync(category, activeOnly, cancellationToken);
        return Ok(promotions);
    }

    [HttpGet("{promotionId:guid}")]
    public async Task<ActionResult<PromotionDetailResponseDto>> GetPromotionById(
        Guid promotionId,
        CancellationToken cancellationToken = default)
    {
        var promotion = await _promotionQueryService.GetPromotionByIdAsync(promotionId, cancellationToken);
        return promotion is null ? NotFound() : Ok(promotion);
    }

    [HttpPost("evaluate")]
    public async Task<ActionResult<EvaluatePromotionResponseDto>> EvaluatePromotion(
        [FromBody] EvaluatePromotionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // Validator duoc goi explicit de loi tra ve ro tung field cho frontend day 3.
        var validationResult = await _evaluateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(x => x.ErrorMessage).ToArray())));
        }

        var result = await _promotionEvaluationService.EvaluateAsync(request, cancellationToken);
        return Ok(result);
    }
}
