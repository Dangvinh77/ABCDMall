using ABCDMall.Modules.Movies.Application.Services.Admin;
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

    private static MoviesBookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesBookingDbContext(options);
    }

    private sealed class NoOpTicketEmailDispatcher : ITicketEmailDispatcher
    {
        public Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
