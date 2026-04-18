using ABCDMall.Modules.Movies.Application.DTOs.Bookings;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IBookingService
{
    Task<CreateBookingResponseDto> CreateAsync(
        CreateBookingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<BookingDetailResponseDto?> GetByCodeAsync(
        string bookingCode,
        CancellationToken cancellationToken = default);
}
