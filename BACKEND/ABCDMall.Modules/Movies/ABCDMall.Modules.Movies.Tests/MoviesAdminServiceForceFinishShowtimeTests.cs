using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Admin;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesAdminServiceForceFinishShowtimeTests
{
    [Fact]
    public async Task ForceFinishShowtimeAsync_should_return_null_when_showtime_does_not_exist()
    {
        await using var catalogDbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ForceFinishShowtimeAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ForceFinishShowtimeAsync_should_move_end_time_to_the_past_and_refresh_updated_at()
    {
        await using var catalogDbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);
        var showtime = await catalogDbContext.Showtimes.OrderBy(x => x.StartAtUtc).FirstAsync();
        var originalUpdatedAtUtc = showtime.UpdatedAtUtc;
        var previousEndAtUtc = showtime.EndAtUtc;

        var result = await service.ForceFinishShowtimeAsync(showtime.Id);

        Assert.NotNull(result);
        Assert.Equal(showtime.Id, result!.ShowtimeId);
        Assert.Equal(previousEndAtUtc, result.PreviousEndAtUtc);
        Assert.True(result.NewEndAtUtc <= DateTime.UtcNow);
        Assert.Equal("Showtime end time moved to the past for feedback-email testing.", result.Message);

        var reloaded = await catalogDbContext.Showtimes.FirstAsync(x => x.Id == showtime.Id);
        Assert.Equal(result.NewEndAtUtc, reloaded.EndAtUtc);
        Assert.True(reloaded.UpdatedAtUtc >= originalUpdatedAtUtc);
    }

    [Fact]
    public async Task ForceFinishShowtimeAsync_should_move_pending_feedback_requests_for_the_same_showtime_to_the_past()
    {
        await using var catalogDbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);
        var showtime = await catalogDbContext.Showtimes.OrderBy(x => x.StartAtUtc).FirstAsync();
        var sameShowtimePending = await SeedFeedbackRequestAsync(
            bookingDbContext,
            showtime.Id,
            MovieFeedbackRequestStatus.Pending,
            DateTime.UtcNow.AddHours(2));
        var sameShowtimeSent = await SeedFeedbackRequestAsync(
            bookingDbContext,
            showtime.Id,
            MovieFeedbackRequestStatus.Sent,
            DateTime.UtcNow.AddHours(2));
        var otherShowtimePending = await SeedFeedbackRequestAsync(
            bookingDbContext,
            Guid.Parse("77777777-7777-7777-7777-777777777777"),
            MovieFeedbackRequestStatus.Pending,
            DateTime.UtcNow.AddHours(2));

        var result = await service.ForceFinishShowtimeAsync(showtime.Id);

        Assert.NotNull(result);

        var pendingReloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == sameShowtimePending.Id);
        var sentReloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == sameShowtimeSent.Id);
        var otherReloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == otherShowtimePending.Id);

        Assert.Equal(result!.NewEndAtUtc, pendingReloaded.AvailableAtUtc);
        Assert.True(pendingReloaded.UpdatedAtUtc >= sameShowtimePending.UpdatedAtUtc);
        Assert.True(sentReloaded.AvailableAtUtc > DateTime.UtcNow);
        Assert.True(otherReloaded.AvailableAtUtc > DateTime.UtcNow);
    }

    private static MoviesBookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesBookingDbContext(options);
    }

    private static async Task<MovieFeedbackRequest> SeedFeedbackRequestAsync(
        MoviesBookingDbContext dbContext,
        Guid showtimeId,
        MovieFeedbackRequestStatus status,
        DateTime availableAtUtc)
    {
        var now = DateTime.UtcNow;
        var request = new MovieFeedbackRequest
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            MovieId = Guid.NewGuid(),
            ShowtimeId = showtimeId,
            PurchaserEmail = "guest@example.com",
            Status = status,
            AvailableAtUtc = availableAtUtc,
            CreatedAtUtc = now.AddDays(-1),
            UpdatedAtUtc = now.AddDays(-1)
        };

        dbContext.MovieFeedbackRequests.Add(request);
        await dbContext.SaveChangesAsync();
        return request;
    }

    private sealed class NoOpTicketEmailDispatcher : ITicketEmailDispatcher
    {
        public Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
