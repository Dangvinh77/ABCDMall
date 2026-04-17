namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class CreateBookingHoldRequestDto
{
    public Guid ShowtimeId { get; set; }
    public IReadOnlyCollection<Guid> SeatInventoryIds { get; set; } = Array.Empty<Guid>();
    public IReadOnlyCollection<BookingQuoteComboItemDto> SnackCombos { get; set; } = Array.Empty<BookingQuoteComboItemDto>();
    public Guid? PromotionId { get; set; }
    public string? PaymentProvider { get; set; }
    public DateOnly? Birthday { get; set; }
    public Guid? GuestCustomerId { get; set; }
    public string? SessionId { get; set; }
}
