using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/emails")]
public sealed class MoviesAdminEmailsController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;

    public MoviesAdminEmailsController(IMoviesAdminService moviesAdminService)
    {
        _moviesAdminService = moviesAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MoviesAdminEmailLogItemDto>>> GetEmailLogs(
        [FromQuery] string? query,
        [FromQuery] string? deliveryStatus,
        [FromQuery] string? outboxStatus,
        CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetEmailLogsAsync(query, deliveryStatus, outboxStatus, cancellationToken));

    [HttpPost("{bookingId:guid}/resend")]
    public async Task<IActionResult> ResendTicketEmail(Guid bookingId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _moviesAdminService.ResendTicketEmailAsync(bookingId, cancellationToken);
            return Ok(new { message = "Ticket email resent successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to resend ticket email.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
