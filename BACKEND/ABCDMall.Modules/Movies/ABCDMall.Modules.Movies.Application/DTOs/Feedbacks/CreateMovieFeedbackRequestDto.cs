namespace ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;

public sealed class CreateMovieFeedbackRequestDto
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
}
