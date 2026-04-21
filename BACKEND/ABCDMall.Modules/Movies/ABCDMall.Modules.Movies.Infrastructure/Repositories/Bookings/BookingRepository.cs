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

    public Task<Bookingg?> GetByHoldIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.BookingHoldId == holdId, cancellationToken);
    }

    public Task<Bookingg?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
    }

    public Task<Bookingg?> GetByCodeAsync(
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

    public async Task<Bookingg> AddPendingBookingAsync(
        Bookingg booking,
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
}
