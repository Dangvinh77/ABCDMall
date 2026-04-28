using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class MovieFeedback
{
    public Guid Id { get; set; }
    public Guid? FeedbackRequestId { get; set; }
    public Guid? BookingId { get; set; }
    public Guid MovieId { get; set; }
    public Guid? ShowtimeId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? TagsJson { get; set; }
    public string? DisplayName { get; set; }
    public string? CreatedByEmail { get; set; }
    public MovieFeedbackModerationStatus ModerationStatus { get; set; } = MovieFeedbackModerationStatus.Approved;
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ModeratedAtUtc { get; set; }
    public string? ModeratedBy { get; set; }
    public string? ModerationReason { get; set; }

    public Booking? Booking { get; set; }
    public MovieFeedbackRequest? FeedbackRequest { get; set; }
}
