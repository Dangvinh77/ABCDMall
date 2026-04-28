using System.Data;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings;

public sealed class BookingRepository : IBookingRepository
{
    private readonly MoviesBookingDbContext _dbContext;

    public BookingRepository(MoviesBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BookingHold?> GetHoldForBookingAsync(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.BookingHolds
            .AsNoTracking()
            .Include(x => x.Seats)
            .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken);
    }

    public async Task<IReadOnlyList<BookingHold>> GetHoldsForBookingAsync(
        IReadOnlyCollection<Guid> holdIds,
        CancellationToken cancellationToken = default)
    {
        if (holdIds.Count == 0)
        {
            return Array.Empty<BookingHold>();
        }

        return await _dbContext.BookingHolds
            .AsNoTracking()
            .Include(x => x.Seats)
            .Where(x => holdIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<Booking?> GetByHoldIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.BookingHoldId == holdId, cancellationToken);
    }

    public Task<Booking?> GetByCombinedHoldIdsAsync(
        IReadOnlyCollection<Guid> holdIds,
        CancellationToken cancellationToken = default)
    {
        if (holdIds.Count == 0)
        {
            return Task.FromResult<Booking?>(null);
        }

        var primaryHoldId = holdIds.OrderBy(x => x).First();
        return GetByHoldIdAsync(primaryHoldId, cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
    }

    public Task<Booking?> GetByCodeAsync(
        string bookingCode,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.BookingCode == bookingCode, cancellationToken);
    }

    public Task<GuestCustomer?> FindGuestCustomerAsync(
        string email,
        string phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim();
        var normalizedPhoneNumber = phoneNumber.Trim();

        return _dbContext.GuestCustomers
            .FirstOrDefaultAsync(
                x => x.Email == normalizedEmail || x.PhoneNumber == normalizedPhoneNumber,
                cancellationToken);
    }

    public async Task<Booking> AddPendingBookingAsync(
        Booking booking,
        GuestCustomer? newGuestCustomer,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (!booking.BookingHoldId.HasValue)
        {
            throw new InvalidOperationException("Booking hold is required.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var existing = await _dbContext.Bookings
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.BookingHoldId == booking.BookingHoldId, cancellationToken);

        if (existing is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        var holdIsStillActive = await _dbContext.BookingHolds
            .AnyAsync(
                x => x.Id == booking.BookingHoldId.Value
                    && x.Status == BookingHoldStatus.Active
                    && x.ExpiresAtUtc > utcNow,
                cancellationToken);

        if (!holdIsStillActive)
        {
            throw new InvalidOperationException("Booking hold is no longer active.");
        }

        if (newGuestCustomer is not null)
        {
            _dbContext.GuestCustomers.Add(newGuestCustomer);
        }

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return booking;
    }

    public async Task<Booking> AddPendingBookingAsync(
        Booking booking,
        GuestCustomer? newGuestCustomer,
        IReadOnlyCollection<Guid> holdIds,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (holdIds.Count == 0)
        {
            throw new InvalidOperationException("Booking hold is required.");
        }

        var primaryHoldId = holdIds.OrderBy(x => x).First();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var existing = await _dbContext.Bookings
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.BookingHoldId == primaryHoldId, cancellationToken);

        if (existing is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return existing;
        }

        var holds = await _dbContext.BookingHolds
            .Where(x => holdIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var allActive = holds.Count == holdIds.Count
            && holds.All(x => x.Status == BookingHoldStatus.Active && x.ExpiresAtUtc > utcNow);

        if (!allActive)
        {
            throw new InvalidOperationException("One or more booking holds are no longer active.");
        }

        if (newGuestCustomer is not null)
        {
            _dbContext.GuestCustomers.Add(newGuestCustomer);
        }

        _dbContext.Bookings.Add(booking);

        foreach (var hold in holds)
        {
            hold.Status = BookingHoldStatus.Converted;
            hold.UpdatedAtUtc = utcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return booking;
    }
}
