namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class CreateBookingRequestDto
{
    public Guid HoldId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
}
