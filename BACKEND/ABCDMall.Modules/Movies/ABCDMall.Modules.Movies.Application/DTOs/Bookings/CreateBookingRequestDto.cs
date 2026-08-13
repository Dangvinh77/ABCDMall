namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class CreateBookingRequestDto
{
    public IReadOnlyCollection<Guid> HoldIds { get; set; } = Array.Empty<Guid>();

    public Guid HoldId
    {
        get => HoldIds.FirstOrDefault();
        set => HoldIds = value == Guid.Empty ? Array.Empty<Guid>() : new[] { value };
    }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
}
