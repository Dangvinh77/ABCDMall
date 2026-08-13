using System.Net;
using System.Net.Mail;
using ABCDMall.Modules.Movies.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ABCDMall.Modules.Movies.Infrastructure.Services.Emails;

public sealed class SmtpTicketEmailSender : ITicketEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpTicketEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(TicketEmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("EmailSettings are not configured. Please set Host and FromEmail before sending ticket emails.");
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, string.IsNullOrWhiteSpace(_settings.FromName) ? "ABCD Cinema" : _settings.FromName),
            Subject = message.Subject,
            Body = message.HtmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new MailAddress(message.ToEmail, message.ToName));

        var attachmentStream = new MemoryStream(message.AttachmentBytes);
        mailMessage.Attachments.Add(new Attachment(attachmentStream, message.AttachmentFileName, message.AttachmentContentType));

        using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_settings.UserName))
        {
            smtpClient.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
        }

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}
