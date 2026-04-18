using ABCDMall.Modules.Movies.Application.DTOs.Payments;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("{paymentId:guid}")]
    public async Task<ActionResult<PaymentStatusResponseDto>> GetPaymentStatus(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetStatusAsync(paymentId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
