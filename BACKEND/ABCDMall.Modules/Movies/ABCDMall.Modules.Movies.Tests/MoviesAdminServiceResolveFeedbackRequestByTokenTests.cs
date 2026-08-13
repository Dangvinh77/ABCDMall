using System.Security.Cryptography;
using System.Text;
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

public sealed class MoviesAdminServiceResolveFeedbackRequestByTokenTests
{
    [Fact]
    public async Task ResolveFeedbackRequestIdByTokenAsync_should_return_null_when_token_is_unknown()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ResolveFeedbackRequestIdByTokenAsync("unknown-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveFeedbackRequestIdByTokenAsync_should_return_request_id_for_matching_token()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        const string token = "feedback-token";
        var request = await SeedFeedbackRequestAsync(bookingDbContext, token);
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ResolveFeedbackRequestIdByTokenAsync(token);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result!.RequestId);
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

    private static async Task<MovieFeedbackRequest> SeedFeedbackRequestAsync(MoviesBookingDbContext dbContext, string token)
    {
        var now = DateTime.UtcNow;
        var request = new MovieFeedbackRequest
        {
            Id = Guid.Parse("93000000-0000-0000-0000-000000000001"),
            BookingId = Guid.NewGuid(),
            MovieId = Guid.NewGuid(),
            ShowtimeId = Guid.NewGuid(),
            PurchaserEmail = "guest@example.com",
            TokenHash = HashToken(token),
            Status = MovieFeedbackRequestStatus.Sent,
            AvailableAtUtc = now.AddHours(-1),
            SentAtUtc = now.AddHours(-1),
            CreatedAtUtc = now.AddDays(-1),
            UpdatedAtUtc = now.AddDays(-1)
        };

        dbContext.MovieFeedbackRequests.Add(request);
        await dbContext.SaveChangesAsync();
        return request;
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }

    private sealed class NoOpTicketEmailDispatcher : ITicketEmailDispatcher
    {
        public Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
