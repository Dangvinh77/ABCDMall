using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/dashboard")]
public sealed class MoviesAdminDashboardController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;

    public MoviesAdminDashboardController(IMoviesAdminService moviesAdminService)
    {
        _moviesAdminService = moviesAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<MoviesAdminDashboardResponseDto>> GetDashboard(CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetDashboardAsync(cancellationToken));
}
