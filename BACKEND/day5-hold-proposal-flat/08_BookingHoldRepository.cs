using System.Data;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings;

public sealed class BookingHoldRepository : IBookingHoldRepository
{
    private readonly MoviesBookingDbContext _dbContext;

    public BookingHoldRepository(MoviesBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var seatInventoryIds = hold.Seats.Select(x => x.SeatInventoryId).ToArray();
        var now = DateTime.UtcNow;

        var conflicts = await _dbContext.BookingHoldSeats
            .AsNoTracking()
            .Where(seat => seatInventoryIds.Contains(seat.SeatInventoryId)
                && seat.BookingHold != null
                && seat.BookingHold.Status == BookingHoldStatus.Active
                && seat.BookingHold.ExpiresAtUtc > now)
            .Select(seat => seat.SeatCode)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        if (conflicts.Length > 0)
        {
            throw new InvalidOperationException($"Selected seats are already being held: {string.Join(", ", conflicts)}.");
        }

        _dbContext.BookingHolds.Add(hold);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return hold;
    }

    public async Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.BookingHolds
            .AsNoTracking()
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken);
    }

    public async Task<bool> ReleaseAsync(
        Guid holdId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var hold = await _dbContext.BookingHolds
            .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken);

        if (hold is null)
        {
            return false;
        }

        if (hold.Status == BookingHoldStatus.Active)
        {
            hold.Status = BookingHoldStatus.Released;
            hold.UpdatedAtUtc = utcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<int> ExpireAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var expiredHolds = await _dbContext.BookingHolds
            .Where(x => x.Status == BookingHoldStatus.Active && x.ExpiresAtUtc <= utcNow)
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var hold in expiredHolds)
        {
            hold.Status = BookingHoldStatus.Expired;
            hold.UpdatedAtUtc = utcNow;
        }

        if (expiredHolds.Count == 0)
        {
            return 0;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return expiredHolds.Count;
    }
}
