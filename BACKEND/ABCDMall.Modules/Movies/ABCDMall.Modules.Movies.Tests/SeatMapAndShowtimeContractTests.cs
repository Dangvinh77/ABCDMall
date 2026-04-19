using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Screening;
using ABCDMall.Modules.Movies.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class SeatMapAndShowtimeContractTests
{
    private static readonly ShowtimeBookingPolicy BookingPolicy = new();

    [Fact]
    public async Task SeatMap_ShouldContainRequiredFields_AndCoupleSeatsShouldHaveGroupCode()
    {
        await using var dbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        var repository = new ShowtimeRepository(dbContext);
        var queryService = new SeatMapQueryService(repository, new FakeBookingHoldRepository(), BookingPolicy, NullLogger<SeatMapQueryService>.Instance);
        var showtimeId = dbContext.Showtimes.Select(x => x.Id).First();

        var result = await queryService.GetByShowtimeIdAsync(showtimeId);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.ShowtimeId);
        Assert.NotEqual(Guid.Empty, result.HallId);
        Assert.NotEmpty(result.HallType);
        Assert.NotEmpty(result.Seats);

        foreach (var seat in result.Seats)
        {
            Assert.NotEqual(Guid.Empty, seat.SeatInventoryId);
            Assert.False(string.IsNullOrWhiteSpace(seat.SeatCode));
            Assert.False(string.IsNullOrWhiteSpace(seat.Row));
            Assert.True(seat.Col > 0);
            Assert.False(string.IsNullOrWhiteSpace(seat.SeatType));
            Assert.False(string.IsNullOrWhiteSpace(seat.Status));
            Assert.True(seat.Price > 0);

            if (string.Equals(seat.SeatType, SeatType.Couple.ToString(), StringComparison.Ordinal))
            {
                Assert.False(string.IsNullOrWhiteSpace(seat.CoupleGroupCode));
            }
        }
    }

    [Fact]
    public async Task SeatMap_CoupleSeatsInSameGroup_ShouldHaveConsistentStatus()
    {
        await using var dbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        var repository = new ShowtimeRepository(dbContext);
        var queryService = new SeatMapQueryService(repository, new FakeBookingHoldRepository(), BookingPolicy, NullLogger<SeatMapQueryService>.Instance);
        var showtimeId = dbContext.Showtimes.Select(x => x.Id).First();

        var result = await queryService.GetByShowtimeIdAsync(showtimeId);

        var groupedCoupleSeats = result!.Seats
            .Where(x => string.Equals(x.SeatType, SeatType.Couple.ToString(), StringComparison.Ordinal))
            .GroupBy(x => x.CoupleGroupCode)
            .ToList();

        Assert.NotEmpty(groupedCoupleSeats);

        foreach (var group in groupedCoupleSeats)
        {
            Assert.False(string.IsNullOrWhiteSpace(group.Key));
            Assert.Equal(2, group.Count());
            Assert.Single(group.Select(x => x.Status).Distinct(StringComparer.Ordinal));
        }
    }

    [Fact]
    public async Task ShowtimeDetail_ShouldExposeDev1BoundaryFields()
    {
        await using var dbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        var repository = new ShowtimeRepository(dbContext);
        var queryService = new ShowtimeQueryService(repository, BookingPolicy, NullLogger<ShowtimeQueryService>.Instance);
        var showtimeId = dbContext.Showtimes.Select(x => x.Id).First();

        var result = await queryService.GetByIdAsync(showtimeId);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.ShowtimeId);
        Assert.NotEqual(Guid.Empty, result.MovieId);
        Assert.NotEqual(Guid.Empty, result.CinemaId);
        Assert.NotEqual(Guid.Empty, result.HallId);
        Assert.Contains(result.HallType, new[] { "2D", "3D", "IMAX", "4DX" });
        Assert.Contains(result.Language, new[] { "Sub", "Dub" });
        Assert.True(result.BasePrice > 0);
        Assert.False(string.IsNullOrWhiteSpace(result.Status));
    }

    [Fact]
    public async Task ShowtimeList_ShouldSupportBusinessDateHallTypeAndLanguageFilters()
    {
        await using var dbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        var repository = new ShowtimeRepository(dbContext);
        var queryService = new ShowtimeQueryService(repository, BookingPolicy, NullLogger<ShowtimeQueryService>.Instance);
        var unfilteredResult = await queryService.GetListAsync();
        var sample = unfilteredResult.First();

        var result = await queryService.GetListAsync(
            businessDate: sample.BusinessDate,
            hallType: sample.HallType,
            language: sample.Language);

        Assert.NotEmpty(result);
        Assert.All(result, item =>
        {
            Assert.Equal(sample.BusinessDate, item.BusinessDate);
            Assert.Equal(sample.HallType, item.HallType);
            Assert.Equal(sample.Language, item.Language);
        });
    }
}
