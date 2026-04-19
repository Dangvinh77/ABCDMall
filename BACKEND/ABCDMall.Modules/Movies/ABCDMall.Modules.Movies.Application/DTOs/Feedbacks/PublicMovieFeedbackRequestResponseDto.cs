namespace ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;

public sealed class PublicMovieFeedbackRequestResponseDto
{
    public Guid FeedbackRequestId { get; set; }
    public Guid MovieId { get; set; }
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime AvailableAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool CanSubmit { get; set; }
    public string? Message { get; set; }
}
