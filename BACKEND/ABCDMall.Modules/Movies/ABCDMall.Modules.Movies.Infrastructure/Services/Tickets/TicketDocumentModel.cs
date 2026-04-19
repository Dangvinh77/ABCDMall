namespace ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;

public sealed class TicketDocumentModel
{
    public required string BookingCode { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerEmail { get; init; }
    public required string MovieTitle { get; init; }
    public required string CinemaName { get; init; }
    public required string HallName { get; init; }
    public required string ShowtimeText { get; init; }
    public required string PaymentProvider { get; init; }
    public required string PaymentTimeText { get; init; }
    public required string TotalText { get; init; }
    public required string QrCodePayload { get; init; }
    public required IReadOnlyCollection<TicketDocumentSeat> Seats { get; init; }
    public required IReadOnlyCollection<TicketDocumentLine> Combos { get; init; }
}

public sealed class TicketDocumentSeat
{
    public required string TicketCode { get; init; }
    public required string SeatCode { get; init; }
    public required string Description { get; init; }
}

public sealed class TicketDocumentLine
{
    public required string Description { get; init; }
    public required string Quantity { get; init; }
}
