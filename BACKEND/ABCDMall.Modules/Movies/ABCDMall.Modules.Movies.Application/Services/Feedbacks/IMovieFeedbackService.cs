using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;

namespace ABCDMall.Modules.Movies.Application.Services.Feedbacks;

public interface IMovieFeedbackService
{
    Task<MovieFeedbackListResponseDto> GetByMovieAsync(
        Guid movieId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackResponseDto> CreateForMovieAsync(
        Guid movieId,
        CreateMovieFeedbackRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PublicMovieFeedbackRequestResponseDto> GetPublicRequestAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackResponseDto> SubmitByTokenAsync(
        string token,
        SubmitMovieFeedbackByTokenRequestDto request,
        CancellationToken cancellationToken = default);
}
