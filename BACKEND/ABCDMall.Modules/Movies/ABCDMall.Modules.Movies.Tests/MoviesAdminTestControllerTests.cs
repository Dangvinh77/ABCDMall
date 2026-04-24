using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesAdminTestControllerTests
{
    [Fact]
    public async Task ForceFinishShowtime_should_return_not_found_outside_dev_or_test()
    {
        var service = new FakeMoviesAdminService();
        var controller = new MoviesAdminTestController(service, new FakeHostEnvironment("Production"));

        var result = await controller.ForceFinishShowtime(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(0, service.CallCount);
    }

    [Fact]
    public async Task ForceFinishShowtime_should_return_ok_when_environment_is_development()
    {
        var response = new MoviesAdminForceFinishShowtimeResponseDto
        {
            ShowtimeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            PreviousEndAtUtc = DateTime.UtcNow.AddHours(2),
            NewEndAtUtc = DateTime.UtcNow.AddMinutes(-1),
            Message = "Showtime end time moved to the past for feedback-email testing."
        };

        var service = new FakeMoviesAdminService
        {
            Response = response
        };
        var controller = new MoviesAdminTestController(service, new FakeHostEnvironment(Environments.Development));

        var result = await controller.ForceFinishShowtime(response.ShowtimeId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<MoviesAdminForceFinishShowtimeResponseDto>(ok.Value);
        Assert.Equal(response.ShowtimeId, payload.ShowtimeId);
        Assert.Equal(1, service.CallCount);
    }

    private sealed class FakeMoviesAdminService : IMoviesAdminService
    {
        public int CallCount { get; private set; }
        public MoviesAdminForceFinishShowtimeResponseDto? Response { get; set; }
        public MoviesAdminForceExpireOpenedFeedbackRequestResponseDto? ForceExpireOpenedResponse { get; set; }

        public Task<MoviesAdminForceFinishShowtimeResponseDto?> ForceFinishShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        {
            CallCount += 1;
            return Task.FromResult(Response);
        }

        public Task<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto?> ForceExpireOpenedFeedbackRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
            => Task.FromResult(ForceExpireOpenedResponse);

        public Task<MoviesAdminDashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminMovieListItemDto>> GetMoviesAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminMovieListItemDto?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminMovieListItemDto> CreateMovieAsync(MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminMovieListItemDto?> UpdateMovieAsync(Guid movieId, MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteMovieAsync(Guid movieId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminPromotionListItemDto>> GetPromotionsAsync(string? status, string? query, bool activeOnly, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminPromotionDetailDto?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminPromotionDetailDto> CreatePromotionAsync(MoviesAdminPromotionUpsertDto request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminPromotionDetailDto?> UpdatePromotionAsync(Guid promotionId, MoviesAdminPromotionUpsertDto request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeletePromotionAsync(Guid promotionId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminShowtimeListItemDto>> GetShowtimesAsync(Guid? movieId, DateOnly? businessDate, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminShowtimeListItemDto> CreateShowtimeAsync(MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminShowtimeListItemDto?> UpdateShowtimeAsync(Guid showtimeId, MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(string? status, string? paymentStatus, Guid? movieId, Guid? cinemaId, string? query, DateTime? dateFromUtc, DateTime? dateToUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(string? status, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminBookingDetailDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminLookupResponseDto> GetLookupsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(string? status, string? provider, Guid? movieId, Guid? cinemaId, string? query, DateTime? dateFromUtc, DateTime? dateToUtc, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(string? status, string? provider, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminPaymentDetailDto?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(string? query, string? deliveryStatus, string? outboxStatus, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task ResendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<MoviesAdminRevenueReportDto> GetRevenueReportAsync(DateTime? dateFromUtc, DateTime? dateToUtc, Guid? movieId, Guid? cinemaId, string? provider, string? paymentStatus, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
            ContentRootFileProvider = new NullFileProvider();
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "Movies.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
