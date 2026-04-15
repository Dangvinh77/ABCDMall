namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingQuoteLineDto
{
    public string Type { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
