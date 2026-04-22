using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Feedbacks;

public interface IMovieFeedbackRepository
{
    Task<IReadOnlyList<MovieFeedback>> GetVisibleByMovieAsync(
        Guid movieId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MovieFeedback>> GetVisibleForAggregateAsync(
        Guid movieId,
        CancellationToken cancellationToken = default);

    Task<MovieFeedback> AddAsync(
        MovieFeedback feedback,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackRequest?> GetRequestByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<MovieFeedback> SubmitByRequestAsync(
        MovieFeedbackRequest request,
        MovieFeedback feedback,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
