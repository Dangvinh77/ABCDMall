namespace ABCDMall.Modules.Movies.Infrastructure.Services.Emails;

public interface ITicketEmailSender
{
    Task SendAsync(TicketEmailMessage message, CancellationToken cancellationToken = default);
}
