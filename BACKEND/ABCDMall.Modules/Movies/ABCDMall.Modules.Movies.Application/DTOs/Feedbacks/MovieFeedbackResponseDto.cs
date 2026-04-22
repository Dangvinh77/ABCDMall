namespace ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;

public sealed class MovieFeedbackResponseDto
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid? ShowtimeId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
    public string ModerationStatus { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
