using ABCDMall.Modules.Movies.Application.DTOs.Admin;

namespace ABCDMall.Modules.Movies.Application.Services.Admin;

public interface IMoviesAdminService
{
    Task<MoviesAdminDashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MoviesAdminMovieListItemDto>> GetMoviesAsync(CancellationToken cancellationToken = default);
    Task<MoviesAdminMovieListItemDto?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<MoviesAdminMovieListItemDto> CreateMovieAsync(MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default);
    Task<MoviesAdminMovieListItemDto?> UpdateMovieAsync(Guid movieId, MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteMovieAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MoviesAdminShowtimeListItemDto>> GetShowtimesAsync(Guid? movieId, DateOnly? businessDate, CancellationToken cancellationToken = default);
    Task<MoviesAdminShowtimeListItemDto> CreateShowtimeAsync(MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default);
    Task<MoviesAdminShowtimeListItemDto?> UpdateShowtimeAsync(Guid showtimeId, MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(string? status, CancellationToken cancellationToken = default);
    Task<MoviesAdminBookingDetailDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<MoviesAdminLookupResponseDto> GetLookupsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(string? status, string? provider, CancellationToken cancellationToken = default);
    Task<MoviesAdminPaymentDetailDto?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(CancellationToken cancellationToken = default);
    Task ResendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<MoviesAdminRevenueReportDto> GetRevenueReportAsync(
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
        Guid? movieId,
        Guid? cinemaId,
        string? provider,
        string? paymentStatus,
        CancellationToken cancellationToken = default);
}
