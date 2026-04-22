namespace ABCDMall.Modules.Movies.Application.Services.Showtimes;

public sealed class ShowtimeBookingDecision
{
    public bool IsBookable { get; init; }

    public string? UnavailableReason { get; init; }
}
