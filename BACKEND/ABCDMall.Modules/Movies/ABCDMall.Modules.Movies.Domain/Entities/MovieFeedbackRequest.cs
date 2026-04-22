using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class MovieFeedbackRequest
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid MovieId { get; set; }
    public Guid ShowtimeId { get; set; }
    public string PurchaserEmail { get; set; } = string.Empty;
    public string? TokenHash { get; set; }
    public MovieFeedbackRequestStatus Status { get; set; } = MovieFeedbackRequestStatus.Pending;
    public DateTime AvailableAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? InvalidatedAtUtc { get; set; }
    public int EmailRetryCount { get; set; }
    public string? LastEmailError { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Bookingg? Booking { get; set; }
    public MovieFeedback? Feedback { get; set; }
}
