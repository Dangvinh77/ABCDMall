namespace ABCDMall.Modules.Movies.Infrastructure.Services.Emails;

public sealed class MovieFeedbackEmailMessage
{
    public required string ToEmail { get; init; }
    public required string ToName { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
}
