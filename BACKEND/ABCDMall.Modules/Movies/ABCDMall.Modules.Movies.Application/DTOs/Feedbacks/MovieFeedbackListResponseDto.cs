namespace ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;

public sealed class MovieFeedbackListResponseDto
{
    public Guid MovieId { get; set; }
    public int? RatingFilter { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public decimal AverageRating { get; set; }
    public IReadOnlyDictionary<int, int> RatingBreakdown { get; set; } = new Dictionary<int, int>();
    public IReadOnlyList<MovieFeedbackResponseDto> Items { get; set; } = Array.Empty<MovieFeedbackResponseDto>();
}
