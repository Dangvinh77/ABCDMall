using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IBookingRepository
{
    Task<Bookingg?> GetByIdAsync(
        Guid bookingId,
        CancellationToken cancellationToken = default);

    Task<BookingHold?> GetHoldForBookingAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BookingHold>> GetHoldsForBookingAsync(
        IReadOnlyCollection<Guid> holdIds,
        CancellationToken cancellationToken = default);

    Task<Bookingg?> GetByHoldIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);

    Task<Bookingg?> GetByCombinedHoldIdsAsync(
        IReadOnlyCollection<Guid> holdIds,
        CancellationToken cancellationToken = default);

    Task<Bookingg?> GetByCodeAsync(
        string bookingCode,
        CancellationToken cancellationToken = default);

    Task<GuestCustomer?> FindGuestCustomerAsync(
        string email,
        string phoneNumber,
        CancellationToken cancellationToken = default);

    Task<Bookingg> AddPendingBookingAsync(
        Bookingg booking,
        GuestCustomer? newGuestCustomer,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<Bookingg> AddPendingBookingAsync(
        Bookingg booking,
        GuestCustomer? newGuestCustomer,
        IReadOnlyCollection<Guid> holdIds,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
