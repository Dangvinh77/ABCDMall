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
        [FromQuery] string? paymentStatus,
        [FromQuery] Guid? movieId,
        [FromQuery] Guid? cinemaId,
        [FromQuery] string? query,
        [FromQuery] DateTime? dateFromUtc,
        [FromQuery] DateTime? dateToUtc,
        CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetBookingsAsync(
            status,
            paymentStatus,
            movieId,
            cinemaId,
            query,
            dateFromUtc,
            dateToUtc,
            cancellationToken));

    [HttpGet("{bookingId:guid}")]
    public async Task<ActionResult<MoviesAdminBookingDetailDto>> GetBookingById(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _moviesAdminService.GetBookingByIdAsync(bookingId, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }
}
