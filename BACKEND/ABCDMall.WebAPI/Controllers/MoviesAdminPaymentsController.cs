using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/payments")]
public sealed class MoviesAdminPaymentsController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;

    public MoviesAdminPaymentsController(IMoviesAdminService moviesAdminService)
    {
        _moviesAdminService = moviesAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MoviesAdminPaymentListItemDto>>> GetPayments(
        [FromQuery] string? status,
        [FromQuery] string? provider,
        CancellationToken cancellationToken = default)
        => Ok(await _moviesAdminService.GetPaymentsAsync(status, provider, cancellationToken));

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<MoviesAdminPaymentDetailDto>> GetPaymentById(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _moviesAdminService.GetPaymentByIdAsync(paymentId, cancellationToken);
        return payment is null ? NotFound() : Ok(payment);
    }
}
