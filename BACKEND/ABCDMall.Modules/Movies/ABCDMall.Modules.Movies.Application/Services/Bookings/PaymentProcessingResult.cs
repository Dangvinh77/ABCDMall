using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public sealed class PaymentProcessingResult
{
    public required Payment Payment { get; init; }
    public required Bookingg Booking { get; init; }
}
