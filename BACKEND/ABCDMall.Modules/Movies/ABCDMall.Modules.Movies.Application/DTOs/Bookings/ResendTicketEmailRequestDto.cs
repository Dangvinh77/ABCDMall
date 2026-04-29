namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class ResendTicketEmailRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string BookingCode { get; set; } = string.Empty;
}
