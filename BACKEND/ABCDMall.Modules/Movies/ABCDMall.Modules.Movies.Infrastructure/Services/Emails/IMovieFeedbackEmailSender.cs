namespace ABCDMall.Modules.Movies.Infrastructure.Services.Emails;

public interface IMovieFeedbackEmailSender
{
    Task SendAsync(MovieFeedbackEmailMessage message, CancellationToken cancellationToken = default);
}
