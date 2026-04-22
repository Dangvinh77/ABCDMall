using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/bookings")]
public sealed class MoviesAdminBookingsController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;

    public MoviesAdminBookingsController(IMoviesAdminService moviesAdminService)
    {
        _moviesAdminService = moviesAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MoviesAdminBookingListItemDto>>> GetBookings(
        [FromQuery] string? status,
        CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetBookingsAsync(status, cancellationToken));

    [HttpGet("{bookingId:guid}")]
    public async Task<ActionResult<MoviesAdminBookingDetailDto>> GetBookingById(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _moviesAdminService.GetBookingByIdAsync(bookingId, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }
}
