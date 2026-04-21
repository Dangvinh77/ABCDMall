namespace ABCDMall.Modules.Movies.Application.DTOs.Payments;

public sealed class CreateStripeCheckoutSessionRequestDto
{
    public Guid BookingId { get; set; }
}
