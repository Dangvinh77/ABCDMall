using ABCDMall.Modules.Movies.Application.DTOs.Bookings;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public interface IBookingQuoteService
{
    Task<BookingQuoteResponseDto> QuoteAsync(
        BookingQuoteRequestDto request,
        CancellationToken cancellationToken = default);
}
