namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public sealed class MoviesAdminDashboardResponseDto
{
    public int ActiveMovies { get; set; }
    public int UpcomingShowtimes { get; set; }
    public int TotalBookings { get; set; }
    public decimal PaidRevenue { get; set; }
    public IReadOnlyList<MoviesAdminUpcomingShowtimeDto> UpcomingShowtimesSnapshot { get; set; } = [];
    public IReadOnlyList<MoviesAdminRecentBookingDto> RecentBookings { get; set; } = [];
    public IReadOnlyList<MoviesAdminTopMovieDto> TopMovies { get; set; } = [];
}

public sealed class MoviesAdminUpcomingShowtimeDto
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class MoviesAdminRecentBookingDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public decimal GrandTotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}

public sealed class MoviesAdminTopMovieDto
{
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int BookedSeats { get; set; }
    public decimal Revenue { get; set; }
}
