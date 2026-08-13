using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/revenue")]
public sealed class MoviesAdminRevenueController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;

    public MoviesAdminRevenueController(IMoviesAdminService moviesAdminService)
    {
        _moviesAdminService = moviesAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<MoviesAdminRevenueReportDto>> GetRevenueReport(
        [FromQuery] DateTime? dateFromUtc,
        [FromQuery] DateTime? dateToUtc,
        [FromQuery] Guid? movieId,
        [FromQuery] Guid? cinemaId,
        [FromQuery] string? provider,
        [FromQuery] string? paymentStatus,
        CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetRevenueReportAsync(
            dateFromUtc,
            dateToUtc,
            movieId,
            cinemaId,
            provider,
            paymentStatus,
            cancellationToken));
}
