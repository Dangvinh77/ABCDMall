using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;

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

    Task<IReadOnlyList<MovieFeedbackRequest>> GetPendingInvitationRequestsAsync(
        DateTime utcNow,
        int take,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackRequest> MarkOpenedAsync(
        Guid requestId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackRequest> MarkInvitationSentAsync(
        Guid requestId,
        string tokenHash,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackRequest> MarkInvitationFailedAsync(
        Guid requestId,
        string error,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<MovieFeedbackRequest> MarkExpiredAsync(
        Guid requestId,
        MovieFeedbackRequestExpiredReason reason,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<MovieFeedback> SubmitByRequestAsync(
        MovieFeedbackRequest request,
        MovieFeedback feedback,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
