using System.Security.Cryptography;
using System.Text;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Options;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Services.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;

public sealed class TicketEmailDispatcher : ITicketEmailDispatcher
{
    private readonly MoviesBookingDbContext _bookingDbContext;
    private readonly MoviesCatalogDbContext _catalogDbContext;
    private readonly ITicketPdfRenderer _pdfRenderer;
    private readonly ITicketEmailSender _emailSender;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<TicketEmailDispatcher> _logger;

    public TicketEmailDispatcher(
        MoviesBookingDbContext bookingDbContext,
        MoviesCatalogDbContext catalogDbContext,
        ITicketPdfRenderer pdfRenderer,
        ITicketEmailSender emailSender,
        IOptions<StripeSettings> stripeSettings,
        ILogger<TicketEmailDispatcher> logger)
    {
        _bookingDbContext = bookingDbContext;
        _catalogDbContext = catalogDbContext;
        _pdfRenderer = pdfRenderer;
        _emailSender = emailSender;
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
    }

    public async Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingDbContext.Bookings
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .Include(x => x.Tickets)
            .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            throw new InvalidOperationException("Booking not found for ticket email.");
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Ticket email can only be sent for confirmed bookings.");
        }

        if (booking.Tickets.Count == 0)
        {
            throw new InvalidOperationException("Booking has no issued tickets.");
        }

        var showtime = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .Include(x => x.Movie)
            .Include(x => x.Cinema)
            .Include(x => x.Hall)
            .FirstOrDefaultAsync(x => x.Id == booking.ShowtimeId, cancellationToken);

        if (showtime?.Movie is null || showtime.Cinema is null || showtime.Hall is null)
        {
            throw new InvalidOperationException("Showtime details were not found for ticket email.");
        }

        var succeededPayment = booking.Payments
            .Where(x => x.Status == PaymentStatus.Succeeded)
            .OrderByDescending(x => x.CompletedAtUtc ?? x.UpdatedAtUtc)
            .FirstOrDefault();

        var ticketDocument = BuildTicketDocument(booking, showtime, succeededPayment);
        var pdfBytes = _pdfRenderer.Render(ticketDocument);
        var fileName = $"ABCD-Cinema-Ticket-{booking.BookingCode}.pdf";
        var feedbackLink = await PrepareFeedbackLinkAsync(booking, cancellationToken);

        await _emailSender.SendAsync(new TicketEmailMessage
        {
            ToEmail = booking.CustomerEmail,
            ToName = string.IsNullOrWhiteSpace(booking.CustomerName) ? "ABCD Cinema guest" : booking.CustomerName,
            Subject = $"Payment successful - your ABCD Cinema ticket {booking.BookingCode}",
            HtmlBody = BuildEmailBody(booking, ticketDocument, feedbackLink),
            AttachmentFileName = fileName,
            AttachmentContentType = "application/pdf",
            AttachmentBytes = pdfBytes
        }, cancellationToken);

        foreach (var ticket in booking.Tickets)
        {
            ticket.DeliveryStatus = TicketDeliveryStatuses.Delivered;
            ticket.PdfFileName = fileName;
            ticket.EmailSentAtUtc = DateTime.UtcNow;
            ticket.EmailSendError = null;
            ticket.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _bookingDbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Ticket email sent for booking {BookingCode}.", booking.BookingCode);
    }

    private async Task<string?> PrepareFeedbackLinkAsync(Bookingg booking, CancellationToken cancellationToken)
    {
        var feedbackRequest = await _bookingDbContext.MovieFeedbackRequests
            .FirstOrDefaultAsync(
                x => x.BookingId == booking.Id && x.ShowtimeId == booking.ShowtimeId,
                cancellationToken);

        if (feedbackRequest is null)
        {
            return null;
        }

        if (feedbackRequest.Status is MovieFeedbackRequestStatus.Submitted
            or MovieFeedbackRequestStatus.Cancelled
            or MovieFeedbackRequestStatus.Expired)
        {
            return null;
        }

        var token = GenerateFeedbackToken();
        feedbackRequest.TokenHash = HashToken(token);
        feedbackRequest.Status = MovieFeedbackRequestStatus.Sent;
        feedbackRequest.SentAtUtc = DateTime.UtcNow;
        feedbackRequest.UpdatedAtUtc = DateTime.UtcNow;

        var baseUrl = string.IsNullOrWhiteSpace(_stripeSettings.FrontendBaseUrl)
            ? "http://localhost:5173"
            : _stripeSettings.FrontendBaseUrl.TrimEnd('/');

        return $"{baseUrl}/movies/feedback/{Uri.EscapeDataString(token)}";
    }

    private static TicketDocumentModel BuildTicketDocument(
        Bookingg booking,
        Showtime showtime,
        Payment? payment)
    {
        var seatItems = booking.Items
            .Where(x => string.Equals(x.ItemType, "Seat", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.ItemCode)
            .ToArray();

        var seatRows = seatItems
            .Select(item =>
            {
                var ticket = booking.Tickets.FirstOrDefault(x => x.BookingItemId == item.Id || x.SeatInventoryId == item.SeatInventoryId);
                return new TicketDocumentSeat
                {
                    TicketCode = ticket?.TicketCode ?? booking.BookingCode,
                    SeatCode = item.ItemCode,
                    Description = item.Description
                };
            })
            .ToArray();

        var comboRows = booking.Items
            .Where(x => !string.Equals(x.ItemType, "Seat", StringComparison.OrdinalIgnoreCase))
            .Select(x => new TicketDocumentLine
            {
                Description = x.Description,
                Quantity = x.Quantity.ToString()
            })
            .ToArray();

        var startLocal = DateTime.SpecifyKind(showtime.StartAtUtc, DateTimeKind.Utc).ToLocalTime();
        var completedLocal = payment?.CompletedAtUtc is null
            ? booking.UpdatedAtUtc.ToLocalTime()
            : DateTime.SpecifyKind(payment.CompletedAtUtc.Value, DateTimeKind.Utc).ToLocalTime();

        return new TicketDocumentModel
        {
            BookingCode = booking.BookingCode,
            CustomerName = string.IsNullOrWhiteSpace(booking.CustomerName) ? "ABCD Cinema guest" : booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            MovieTitle = showtime.Movie?.Title ?? "Movie",
            CinemaName = showtime.Cinema?.Name ?? "ABCD Cinema",
            HallName = showtime.Hall?.Name ?? showtime.Hall?.HallCode ?? "Hall",
            ShowtimeText = startLocal.ToString("dd/MM/yyyy HH:mm"),
            PaymentProvider = payment?.Provider.ToString() ?? "Payment",
            PaymentTimeText = completedLocal.ToString("dd/MM/yyyy HH:mm"),
            TotalText = $"{booking.GrandTotal:N0} {booking.Currency}",
            QrCodePayload = $"ABCDMALL|BOOKING:{booking.BookingCode}|TICKETS:{string.Join(",", booking.Tickets.Select(x => x.TicketCode))}",
            Seats = seatRows,
            Combos = comboRows
        };
    }

    private static string BuildEmailBody(Bookingg booking, TicketDocumentModel document, string? feedbackLink)
    {
        var feedbackParagraph = string.IsNullOrWhiteSpace(feedbackLink)
            ? string.Empty
            : $$"""
            <p>
                After the show ends, you can share feedback for
                <strong>{{System.Net.WebUtility.HtmlEncode(document.MovieTitle)}}</strong> here:
                <a href="{{System.Net.WebUtility.HtmlEncode(feedbackLink)}}">Open movie feedback</a>.
                This link is available from the end of the showtime for 72 hours and allows up to 3 submissions.
            </p>
            """;

        return $$"""
            <p>Hello {{System.Net.WebUtility.HtmlEncode(document.CustomerName)}},</p>
            <p>Your payment for booking <strong>{{System.Net.WebUtility.HtmlEncode(booking.BookingCode)}}</strong> was successful.</p>
            <p>Your ABCD Cinema ticket PDF is attached to this email.</p>
            <p>
                <strong>Movie:</strong> {{System.Net.WebUtility.HtmlEncode(document.MovieTitle)}}<br />
                <strong>Showtime:</strong> {{System.Net.WebUtility.HtmlEncode(document.ShowtimeText)}}<br />
                <strong>Seats:</strong> {{System.Net.WebUtility.HtmlEncode(string.Join(", ", document.Seats.Select(x => x.SeatCode)))}}<br />
                <strong>Total:</strong> {{System.Net.WebUtility.HtmlEncode(document.TotalText)}}
            </p>
            <p>Please arrive 15 minutes before showtime.</p>
            {{feedbackParagraph}}
            <p>ABCD Cinema</p>
            """;
    }

    private static string GenerateFeedbackToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }
}
