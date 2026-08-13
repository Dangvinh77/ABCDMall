namespace ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;

public sealed class SubmitMovieFeedbackByTokenRequestDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
}
