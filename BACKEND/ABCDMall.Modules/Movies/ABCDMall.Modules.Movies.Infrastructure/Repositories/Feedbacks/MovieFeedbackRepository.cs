using ABCDMall.Modules.Movies.Application.Services.Feedbacks;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Feedbacks;

public sealed class MovieFeedbackRepository : IMovieFeedbackRepository
{
    private readonly MoviesBookingDbContext _dbContext;

    public MovieFeedbackRepository(MoviesBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MovieFeedback>> GetVisibleByMovieAsync(
        Guid movieId,
        int? rating,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = VisibleFeedbacks(movieId);

        if (rating.HasValue)
        {
            query = query.Where(x => x.Rating == rating.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MovieFeedback>> GetVisibleForAggregateAsync(
        Guid movieId,
        CancellationToken cancellationToken = default)
    {
        return await VisibleFeedbacks(movieId)
            .ToListAsync(cancellationToken);
    }

    public async Task<MovieFeedback> AddAsync(
        MovieFeedback feedback,
        CancellationToken cancellationToken = default)
    {
        _dbContext.MovieFeedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return feedback;
    }

    public Task<MovieFeedbackRequest?> GetRequestByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.MovieFeedbackRequests
            .Include(x => x.Feedbacks)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<MovieFeedbackRequest> MarkOpenedAsync(
        Guid requestId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var trackedRequest = await _dbContext.MovieFeedbackRequests
            .Include(x => x.Feedbacks)
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken)
            ?? throw new InvalidOperationException("Feedback request not found.");

        if (!trackedRequest.FirstOpenedAtUtc.HasValue)
        {
            trackedRequest.FirstOpenedAtUtc = utcNow;
        }

        trackedRequest.LastOpenedAtUtc = utcNow;
        trackedRequest.UpdatedAtUtc = utcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return trackedRequest;
    }

    public async Task<MovieFeedbackRequest> MarkExpiredAsync(
        Guid requestId,
        MovieFeedbackRequestExpiredReason reason,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var trackedRequest = await _dbContext.MovieFeedbackRequests
            .Include(x => x.Feedbacks)
            .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken)
            ?? throw new InvalidOperationException("Feedback request not found.");

        trackedRequest.Status = MovieFeedbackRequestStatus.Expired;
        trackedRequest.ExpiredReason = reason;
        trackedRequest.InvalidatedAtUtc = utcNow;
        trackedRequest.UpdatedAtUtc = utcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return trackedRequest;
    }

    public async Task<MovieFeedback> SubmitByRequestAsync(
        MovieFeedbackRequest request,
        MovieFeedback feedback,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var trackedRequest = await _dbContext.MovieFeedbackRequests
            .Include(x => x.Feedbacks)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (trackedRequest is null)
        {
            throw new InvalidOperationException("Feedback request not found.");
        }

        if (trackedRequest.Status != MovieFeedbackRequestStatus.Sent
            || trackedRequest.InvalidatedAtUtc.HasValue
            || trackedRequest.AvailableAtUtc > utcNow
            || !trackedRequest.ExpiresAtUtc.HasValue
            || trackedRequest.ExpiresAtUtc.Value <= utcNow)
        {
            throw new InvalidOperationException("You have already submitted feedback for this movie.");
        }

        if (trackedRequest.Status == MovieFeedbackRequestStatus.Submitted || trackedRequest.Feedbacks.Count >= 3)
        {
            throw new InvalidOperationException("You have already submitted feedback for this movie.");
        }

        if (trackedRequest.Feedbacks.Count == 2)
        {
            trackedRequest.Status = MovieFeedbackRequestStatus.Submitted;
            trackedRequest.SubmittedAtUtc = utcNow;
            trackedRequest.InvalidatedAtUtc = utcNow;
        }

        trackedRequest.UpdatedAtUtc = utcNow;

        _dbContext.MovieFeedbacks.Add(feedback);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return feedback;
    }

    private IQueryable<MovieFeedback> VisibleFeedbacks(Guid movieId)
    {
        return _dbContext.MovieFeedbacks
            .AsNoTracking()
            .Where(x => x.MovieId == movieId
                && x.IsVisible
                && x.ModerationStatus == MovieFeedbackModerationStatus.Approved);
    }
}
