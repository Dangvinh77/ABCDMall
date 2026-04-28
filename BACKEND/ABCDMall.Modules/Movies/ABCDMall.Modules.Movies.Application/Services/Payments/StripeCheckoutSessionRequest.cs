using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Payments;

public sealed class StripeCheckoutSessionRequest
{
    public required Booking Booking { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
}
