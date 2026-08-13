using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Admin;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesAdminServiceForceExpireOpenedFeedbackRequestTests
{
    [Fact]
    public async Task ForceExpireOpenedFeedbackRequestAsync_should_return_null_when_request_does_not_exist()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ForceExpireOpenedFeedbackRequestAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ForceExpireOpenedFeedbackRequestAsync_should_move_opened_timestamps_to_the_past_and_keep_sent_status()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        var request = await SeedFeedbackRequestAsync(bookingDbContext);
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);
        var originalUpdatedAtUtc = request.UpdatedAtUtc;

        var result = await service.ForceExpireOpenedFeedbackRequestAsync(request.Id);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result!.RequestId);
        Assert.True(result.NewFirstOpenedAtUtc <= DateTime.UtcNow.AddDays(-7));
        Assert.True(result.NewLastOpenedAtUtc <= DateTime.UtcNow.AddDays(-7));
        Assert.Equal("Feedback request opened timestamps moved to the past for expiry testing.", result.Message);

        var reloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == request.Id);
        Assert.Equal(MovieFeedbackRequestStatus.Sent, reloaded.Status);
        Assert.Null(reloaded.ExpiredReason);
        Assert.Null(reloaded.InvalidatedAtUtc);
        Assert.True(reloaded.UpdatedAtUtc >= originalUpdatedAtUtc);
    }

    private static MoviesCatalogDbContext CreateCatalogDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesCatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesCatalogDbContext(options);
    }

    private static MoviesBookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesBookingDbContext(options);
    }

    private static async Task<MovieFeedbackRequest> SeedFeedbackRequestAsync(MoviesBookingDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var request = new MovieFeedbackRequest
        {
            Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
            BookingId = Guid.Parse("90000000-0000-0000-0000-000000000002"),
            MovieId = Guid.Parse("90000000-0000-0000-0000-000000000003"),
            ShowtimeId = Guid.Parse("90000000-0000-0000-0000-000000000004"),
            PurchaserEmail = "guest@example.com",
            TokenHash = "token-hash",
            Status = MovieFeedbackRequestStatus.Sent,
            AvailableAtUtc = now.AddHours(-2),
            SentAtUtc = now.AddHours(-1),
            ExpiresAtUtc = now.AddDays(30),
            FirstOpenedAtUtc = now.AddDays(-1),
            LastOpenedAtUtc = now.AddDays(-1),
            CreatedAtUtc = now.AddDays(-2),
            UpdatedAtUtc = now.AddDays(-2)
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
