using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;
using ABCDMall.Modules.Movies.Application.Services.Feedbacks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/movie-feedback")]
public sealed class MovieFeedbackController : ControllerBase
{
    private readonly IMovieFeedbackService _movieFeedbackService;
    private readonly IValidator<SubmitMovieFeedbackByTokenRequestDto> _submitValidator;

    public MovieFeedbackController(
        IMovieFeedbackService movieFeedbackService,
        IValidator<SubmitMovieFeedbackByTokenRequestDto> submitValidator)
    {
        _movieFeedbackService = movieFeedbackService;
        _submitValidator = submitValidator;
    }

    [HttpGet("public/{token}")]
    public async Task<ActionResult<PublicMovieFeedbackRequestResponseDto>> GetPublicFeedbackRequest(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _movieFeedbackService.GetPublicRequestAsync(token, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to open feedback link.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("public/{token}")]
    public async Task<ActionResult<MovieFeedbackResponseDto>> SubmitPublicFeedback(
        string token,
        [FromBody] SubmitMovieFeedbackByTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _submitValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(x => x.ErrorMessage).ToArray())));
        }

        try
        {
            var response = await _movieFeedbackService.SubmitByTokenAsync(token, request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to submit feedback.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
