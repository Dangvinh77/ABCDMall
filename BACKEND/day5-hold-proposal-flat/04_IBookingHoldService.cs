using ABCDMall.Modules.Movies.Application.DTOs.Bookings;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IBookingHoldService
{
    Task<BookingHoldResponseDto> CreateAsync(
        CreateBookingHoldRequestDto request,
        CancellationToken cancellationToken = default);

    Task<BookingHoldResponseDto?> GetByIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);

    Task<bool> ReleaseAsync(
        Guid holdId,
        CancellationToken cancellationToken = default);
}
