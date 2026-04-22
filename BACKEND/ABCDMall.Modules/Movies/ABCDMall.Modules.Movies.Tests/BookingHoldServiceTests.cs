using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class BookingHoldServiceTests
{
    [Fact]
    public async Task CreateAsync_should_create_a_single_seat_hold_with_ten_minute_expiry()
    {
        var service = BuildService();
        var request = new CreateBookingHoldRequestDto
        {
            ShowtimeId = BookingHoldServiceTestIds.ShowtimeId,
            SeatInventoryIds = new[] { BookingHoldServiceTestIds.SeatA1 }
        };

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.Single(result.Seats);
        Assert.Equal("Active", result.Status);
        Assert.InRange(result.RemainingSeconds, 590, 600);
        Assert.Equal("A1", result.Seats.Single().SeatCode);
    }

    [Fact]
    public async Task ReleaseAsync_should_only_release_the_target_hold()
    {
        var repository = new FakeBookingHoldRepository();
        var service = BuildService(repository);

        var first = await service.CreateAsync(new CreateBookingHoldRequestDto
        {
            ShowtimeId = BookingHoldServiceTestIds.ShowtimeId,
            SeatInventoryIds = new[] { BookingHoldServiceTestIds.SeatA1 }
        }, CancellationToken.None);

        var second = await service.CreateAsync(new CreateBookingHoldRequestDto
        {
            ShowtimeId = BookingHoldServiceTestIds.ShowtimeId,
            SeatInventoryIds = new[] { BookingHoldServiceTestIds.SeatA2 }
        }, CancellationToken.None);

        await service.ReleaseAsync(first.HoldId, CancellationToken.None);

        var released = await service.GetByIdAsync(first.HoldId, CancellationToken.None);
        var active = await service.GetByIdAsync(second.HoldId, CancellationToken.None);

        Assert.Equal("Released", released!.Status);
        Assert.Equal("Active", active!.Status);
    }

    private static BookingHoldService BuildService(FakeBookingHoldRepository? repository = null)
    {
        return new BookingHoldService(
            new FakeShowtimeRepository(),
            new AllowBookingPolicy(),
            new FakeBookingQuoteService(),
            repository ?? new FakeBookingHoldRepository(),
            new FakePromotionRepository());
    }

    private sealed class FakeShowtimeRepository : IShowtimeRepository
    {
        public Task<IReadOnlyList<Showtime>> GetShowtimesAsync(Guid? movieId, Guid? cinemaId, DateOnly? businessDate, string? hallType, string? language, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Showtime>>(Array.Empty<Showtime>());

        public Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        {
            Showtime? showtime = showtimeId == BookingHoldServiceTestIds.ShowtimeId
                ? new Showtime
                {
                    Id = BookingHoldServiceTestIds.ShowtimeId,
                    Status = ShowtimeStatus.Open,
                    StartAtUtc = DateTime.UtcNow.AddHours(2),
                    BusinessDate = DateOnly.FromDateTime(DateTime.UtcNow)
                }
                : null;

            return Task.FromResult(showtime);
        }

        public Task<IReadOnlyList<ShowtimeSeatInventory>> GetSeatMapByShowtimeIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ShowtimeSeatInventory> seats =
            [
                new ShowtimeSeatInventory
                {
                    Id = BookingHoldServiceTestIds.SeatA1,
                    ShowtimeId = BookingHoldServiceTestIds.ShowtimeId,
                    SeatCode = "A1",
                    RowLabel = "A",
                    ColumnNumber = 1,
                    SeatType = SeatType.Regular,
                    Price = 85000,
                    Status = SeatInventoryStatus.Available
                },
                new ShowtimeSeatInventory
                {
                    Id = BookingHoldServiceTestIds.SeatA2,
                    ShowtimeId = BookingHoldServiceTestIds.ShowtimeId,
                    SeatCode = "A2",
                    RowLabel = "A",
                    ColumnNumber = 2,
                    SeatType = SeatType.Regular,
                    Price = 85000,
                    Status = SeatInventoryStatus.Available
                }
            ];

            return Task.FromResult(seats);
        }

        public Task MarkSeatsBookedAsync(Guid showtimeId, IReadOnlyCollection<Guid> seatInventoryIds, DateTime utcNow, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class AllowBookingPolicy : IShowtimeBookingPolicy
    {
        public ShowtimeBookingDecision EvaluateForUser(Showtime showtime, DateTime utcNow)
            => new ShowtimeBookingDecision
            {
                IsBookable = true
            };

        public bool IsVisibleForUser(Showtime showtime, DateTime utcNow)
            => true;

        public void EnsureBookableForUser(Showtime showtime, DateTime utcNow)
        {
        }
    }

    private sealed class FakeBookingQuoteService : IBookingQuoteService
    {
        public Task<BookingQuoteResponseDto> QuoteAsync(BookingQuoteRequestDto request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new BookingQuoteResponseDto
            {
                ShowtimeId = request.ShowtimeId,
                SeatSubtotal = 85000m * request.SeatInventoryIds.Count,
                ServiceFeeTotal = 10000m * request.SeatInventoryIds.Count,
                ComboSubtotal = 0,
                DiscountTotal = 0,
                GrandTotal = 95000m * request.SeatInventoryIds.Count
            });
        }
    }

    private sealed class FakePromotionRepository : IPromotionRepository
    {
        public Task<IReadOnlyList<Promotion>> GetPromotionsAsync(bool activeOnly, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Promotion>>(Array.Empty<Promotion>());

        public Task<Promotion?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default)
            => Task.FromResult<Promotion?>(null);

        public Task<IReadOnlyList<SnackCombo>> GetSnackCombosAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SnackCombo>>(Array.Empty<SnackCombo>());

        public Task<SnackCombo?> GetSnackComboByIdAsync(Guid comboId, CancellationToken cancellationToken = default)
            => Task.FromResult<SnackCombo?>(null);

        public Task<int> CountRedemptionsAsync(Guid promotionId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<int> CountRedemptionsByGuestCustomerAsync(Guid promotionId, Guid guestCustomerId, CancellationToken cancellationToken = default)
            => Task.FromResult(0);
    }

    private static class BookingHoldServiceTestIds
    {
        public static readonly Guid ShowtimeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid SeatA1 = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
        public static readonly Guid SeatA2 = Guid.Parse("aaaaaaaa-2222-2222-2222-222222222222");
    }
}
