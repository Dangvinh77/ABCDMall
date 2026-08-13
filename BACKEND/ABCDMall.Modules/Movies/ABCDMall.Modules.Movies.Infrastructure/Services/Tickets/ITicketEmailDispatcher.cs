namespace ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;

public interface ITicketEmailDispatcher
{
    Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
