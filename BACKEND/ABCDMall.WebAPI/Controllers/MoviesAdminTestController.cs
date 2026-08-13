using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/test")]
public sealed class MoviesAdminTestController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;
    private readonly IHostEnvironment _environment;

    public MoviesAdminTestController(IMoviesAdminService moviesAdminService, IHostEnvironment environment)
    {
        _moviesAdminService = moviesAdminService;
        _environment = environment;
    }

    [HttpPost("showtimes/{showtimeId:guid}/finish-now")]
    public async Task<ActionResult<MoviesAdminForceFinishShowtimeResponseDto>> ForceFinishShowtime(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Test"))
        {
            return NotFound();
        }

        var result = await _moviesAdminService.ForceFinishShowtimeAsync(showtimeId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("feedback-requests/{requestId:guid}/expire-opened")]
    public async Task<ActionResult<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto>> ForceExpireOpenedFeedbackRequest(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Test"))
        {
            return NotFound();
        }

        var result = await _moviesAdminService.ForceExpireOpenedFeedbackRequestAsync(requestId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("feedback-links/resolve-request")]
    public async Task<ActionResult<MoviesAdminResolveFeedbackRequestByTokenResponseDto>> ResolveFeedbackRequest(
        [FromBody] MoviesAdminResolveFeedbackRequestByTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Test"))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Token is required.",
                Detail = "Provide the feedback token in the request body.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var result = await _moviesAdminService.ResolveFeedbackRequestIdByTokenAsync(request.Token, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
