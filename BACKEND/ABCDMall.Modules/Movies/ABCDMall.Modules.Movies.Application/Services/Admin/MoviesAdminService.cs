using ABCDMall.Modules.Movies.Application.DTOs.Admin;

namespace ABCDMall.Modules.Movies.Application.Services.Admin;

public sealed class MoviesAdminService : IMoviesAdminService
{
    private readonly IMoviesAdminRepository _repository;

    public MoviesAdminService(IMoviesAdminRepository repository)
    {
        _repository = repository;
    }

    public Task<MoviesAdminDashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        => _repository.GetDashboardAsync(cancellationToken);

    public Task<IReadOnlyList<MoviesAdminMovieListItemDto>> GetMoviesAsync(CancellationToken cancellationToken = default)
        => _repository.GetMoviesAsync(cancellationToken);

    public Task<MoviesAdminMovieListItemDto?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
        => _repository.GetMovieByIdAsync(movieId, cancellationToken);

    public Task<MoviesAdminMovieListItemDto> CreateMovieAsync(MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default)
        => _repository.CreateMovieAsync(request, cancellationToken);

    public Task<MoviesAdminMovieListItemDto?> UpdateMovieAsync(Guid movieId, MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default)
        => _repository.UpdateMovieAsync(movieId, request, cancellationToken);

    public Task<bool> DeleteMovieAsync(Guid movieId, CancellationToken cancellationToken = default)
        => _repository.DeleteMovieAsync(movieId, cancellationToken);

    public Task<IReadOnlyList<MoviesAdminPromotionListItemDto>> GetPromotionsAsync(
        string? status,
        string? query,
        bool activeOnly,
        CancellationToken cancellationToken = default)
        => _repository.GetPromotionsAsync(status, query, activeOnly, cancellationToken);

    public Task<MoviesAdminPromotionDetailDto?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default)
        => _repository.GetPromotionByIdAsync(promotionId, cancellationToken);

    public Task<MoviesAdminPromotionDetailDto> CreatePromotionAsync(MoviesAdminPromotionUpsertDto request, CancellationToken cancellationToken = default)
        => _repository.CreatePromotionAsync(request, cancellationToken);

    public Task<MoviesAdminPromotionDetailDto?> UpdatePromotionAsync(Guid promotionId, MoviesAdminPromotionUpsertDto request, CancellationToken cancellationToken = default)
        => _repository.UpdatePromotionAsync(promotionId, request, cancellationToken);

    public Task<bool> DeletePromotionAsync(Guid promotionId, CancellationToken cancellationToken = default)
        => _repository.DeletePromotionAsync(promotionId, cancellationToken);

    public Task<IReadOnlyList<MoviesAdminShowtimeListItemDto>> GetShowtimesAsync(Guid? movieId, DateOnly? businessDate, CancellationToken cancellationToken = default)
        => _repository.GetShowtimesAsync(movieId, businessDate, cancellationToken);

    public Task<MoviesAdminShowtimeListItemDto> CreateShowtimeAsync(MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default)
        => _repository.CreateShowtimeAsync(request, cancellationToken);

    public Task<MoviesAdminShowtimeListItemDto?> UpdateShowtimeAsync(Guid showtimeId, MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default)
        => _repository.UpdateShowtimeAsync(showtimeId, request, cancellationToken);

    public Task<bool> DeleteShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        => _repository.DeleteShowtimeAsync(showtimeId, cancellationToken);

    public Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(
        string? status,
        string? paymentStatus,
        Guid? movieId,
        Guid? cinemaId,
        string? query,
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
        CancellationToken cancellationToken = default)
        => _repository.GetBookingsAsync(
            status,
            paymentStatus,
            movieId,
            cinemaId,
            query,
            dateFromUtc,
            dateToUtc,
            cancellationToken);

    public Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(string? status, CancellationToken cancellationToken = default)
        => _repository.GetBookingsAsync(status, cancellationToken);

    public Task<MoviesAdminBookingDetailDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => _repository.GetBookingByIdAsync(bookingId, cancellationToken);

    public Task<MoviesAdminLookupResponseDto> GetLookupsAsync(CancellationToken cancellationToken = default)
        => _repository.GetLookupsAsync(cancellationToken);

    public Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(
        string? status,
        string? provider,
        Guid? movieId,
        Guid? cinemaId,
        string? query,
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
        CancellationToken cancellationToken = default)
        => _repository.GetPaymentsAsync(
            status,
            provider,
            movieId,
            cinemaId,
            query,
            dateFromUtc,
            dateToUtc,
            cancellationToken);

    public Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(string? status, string? provider, CancellationToken cancellationToken = default)
        => _repository.GetPaymentsAsync(status, provider, cancellationToken);

    public Task<MoviesAdminPaymentDetailDto?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
        => _repository.GetPaymentByIdAsync(paymentId, cancellationToken);

    public Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(
        string? query,
        string? deliveryStatus,
        string? outboxStatus,
        CancellationToken cancellationToken = default)
        => _repository.GetEmailLogsAsync(query, deliveryStatus, outboxStatus, cancellationToken);

    public Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(CancellationToken cancellationToken = default)
        => _repository.GetEmailLogsAsync(cancellationToken);

    public Task ResendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => _repository.ResendTicketEmailAsync(bookingId, cancellationToken);

    public Task<MoviesAdminRevenueReportDto> GetRevenueReportAsync(
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
        Guid? movieId,
        Guid? cinemaId,
        string? provider,
        string? paymentStatus,
        CancellationToken cancellationToken = default)
        => _repository.GetRevenueReportAsync(
            dateFromUtc,
            dateToUtc,
            movieId,
            cinemaId,
            provider,
            paymentStatus,
            cancellationToken);
}
