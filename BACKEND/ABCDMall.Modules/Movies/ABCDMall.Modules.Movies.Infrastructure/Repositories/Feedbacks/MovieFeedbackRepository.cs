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
            .Include(x => x.Feedback)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<MovieFeedback> SubmitByRequestAsync(
        MovieFeedbackRequest request,
        MovieFeedback feedback,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var trackedRequest = await _dbContext.MovieFeedbackRequests
            .Include(x => x.Feedback)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (trackedRequest is null)
        {
            throw new InvalidOperationException("Feedback request not found.");
        }

        if (trackedRequest.Feedback is not null || trackedRequest.Status == MovieFeedbackRequestStatus.Submitted)
        {
            throw new InvalidOperationException("Feedback link was already used.");
        }

        trackedRequest.Status = MovieFeedbackRequestStatus.Submitted;
        trackedRequest.SubmittedAtUtc = utcNow;
        trackedRequest.InvalidatedAtUtc = utcNow;
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
