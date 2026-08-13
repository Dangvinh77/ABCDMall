namespace ABCDMall.Modules.Movies.Infrastructure.Services.Emails;

public sealed class TicketEmailMessage
{
    public required string ToEmail { get; init; }
    public required string ToName { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
    public required string AttachmentFileName { get; init; }
    public required string AttachmentContentType { get; init; }
    public required byte[] AttachmentBytes { get; init; }
}
