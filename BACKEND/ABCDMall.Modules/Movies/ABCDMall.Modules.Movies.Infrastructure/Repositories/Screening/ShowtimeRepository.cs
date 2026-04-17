using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Screening;

public sealed class ShowtimeRepository : IShowtimeRepository
{
    private readonly MoviesCatalogDbContext _dbContext;

    public ShowtimeRepository(MoviesCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Showtime>> GetShowtimesAsync(
        Guid? movieId,
        Guid? cinemaId,
        DateOnly? businessDate,
        string? hallType,
        string? language,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Showtimes
            .AsNoTracking()
            .Include(showtime => showtime.Movie)
            .Include(showtime => showtime.Cinema)
            .Include(showtime => showtime.Hall)
            .Where(showtime => showtime.Status == ShowtimeStatus.Open)
            .AsQueryable();

        if (movieId.HasValue)
        {
            query = query.Where(showtime => showtime.MovieId == movieId.Value);
        }

        if (cinemaId.HasValue)
        {
            query = query.Where(showtime => showtime.CinemaId == cinemaId.Value);
        }

        if (businessDate.HasValue)
        {
            query = query.Where(showtime => showtime.BusinessDate == businessDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(hallType))
        {
            query = query.Where(showtime => showtime.Hall != null
                && showtime.Hall.HallType.ToString().ToLower() == hallType.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(showtime => showtime.Language.ToString().ToLower() == language.Trim().ToLower());
        }

        return await query
            .OrderBy(showtime => showtime.BusinessDate)
            .ThenBy(showtime => showtime.StartAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Showtime?> GetShowtimeByIdAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Showtimes
            .AsNoTracking()
            .Include(showtime => showtime.Movie)
            .Include(showtime => showtime.Cinema)
            .Include(showtime => showtime.Hall)
            .FirstOrDefaultAsync(showtime => showtime.Id == showtimeId, cancellationToken);
    }

    public async Task<IReadOnlyList<ShowtimeSeatInventory>> GetSeatMapByShowtimeIdAsync(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShowtimeSeatInventories
            .AsNoTracking()
            .Include(seat => seat.HallSeat)
            .Where(seat => seat.ShowtimeId == showtimeId)
            .OrderBy(seat => seat.RowLabel)
            .ThenBy(seat => seat.ColumnNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkSeatsBookedAsync(
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        // DAY5 TEST-ONLY CONFIRM FLOW:
        // Đánh dấu ghế Booked trực tiếp để test luồng Confirm.
        // Khi có booking/payment thật, thao tác này nên nằm trong use case xác nhận thanh toán/booking hoàn chỉnh.
        var distinctSeatInventoryIds = seatInventoryIds.Distinct().ToArray();
        var seats = await _dbContext.ShowtimeSeatInventories
            .Where(seat => seat.ShowtimeId == showtimeId && distinctSeatInventoryIds.Contains(seat.Id))
            .ToListAsync(cancellationToken);

        if (seats.Count != distinctSeatInventoryIds.Length)
        {
            throw new InvalidOperationException("Some selected seats were not found for this showtime.");
        }

        var unavailableSeats = seats
            .Where(seat => seat.Status != SeatInventoryStatus.Available)
            .Select(seat => seat.SeatCode)
            .ToArray();

        if (unavailableSeats.Length > 0)
        {
            throw new InvalidOperationException($"Selected seats are no longer available: {string.Join(", ", unavailableSeats)}.");
        }

        foreach (var seat in seats)
        {
            seat.Status = SeatInventoryStatus.Booked;
            seat.UpdatedAtUtc = utcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
