using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Admin;

public sealed class MoviesAdminRepository : IMoviesAdminRepository
{
    private readonly MoviesCatalogDbContext _catalogDbContext;
    private readonly MoviesBookingDbContext _bookingDbContext;
    private readonly ITicketEmailDispatcher _ticketEmailDispatcher;

    public MoviesAdminRepository(
        MoviesCatalogDbContext catalogDbContext,
        MoviesBookingDbContext bookingDbContext,
        ITicketEmailDispatcher ticketEmailDispatcher)
    {
        _catalogDbContext = catalogDbContext;
        _bookingDbContext = bookingDbContext;
        _ticketEmailDispatcher = ticketEmailDispatcher;
    }

    public async Task<MoviesAdminDashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var activeMovies = await _catalogDbContext.Movies
            .CountAsync(x => x.Status == MovieStatus.NowShowing || x.Status == MovieStatus.ComingSoon, cancellationToken);
        var upcomingShowtimes = await _catalogDbContext.Showtimes
            .CountAsync(x => x.StartAtUtc >= utcNow && x.Status == ShowtimeStatus.Open, cancellationToken);
        var totalBookings = await _bookingDbContext.Bookings.CountAsync(cancellationToken);
        var paidRevenue = await _bookingDbContext.Payments
            .Where(x => x.Status == PaymentStatus.Succeeded)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

        var movieMap = await _catalogDbContext.Movies.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
        var showtimeMovieMap = await _catalogDbContext.Showtimes.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.MovieId, cancellationToken);

        var upcoming = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .Include(x => x.Movie)
            .Include(x => x.Cinema)
            .Include(x => x.Hall)
            .Where(x => x.StartAtUtc >= utcNow)
            .OrderBy(x => x.StartAtUtc)
            .Take(6)
            .Select(x => new MoviesAdminUpcomingShowtimeDto
            {
                ShowtimeId = x.Id,
                MovieId = x.MovieId,
                MovieTitle = x.Movie != null ? x.Movie.Title : string.Empty,
                CinemaName = x.Cinema != null ? x.Cinema.Name : string.Empty,
                HallName = x.Hall != null ? x.Hall.Name : string.Empty,
                BusinessDate = x.BusinessDate,
                StartAtUtc = x.StartAtUtc,
                Status = x.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var recentBookingsRaw = await _bookingDbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Payments)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(6)
            .ToListAsync(cancellationToken);

        var recentBookings = recentBookingsRaw.Select(x =>
        {
            var payment = x.Payments.OrderByDescending(p => p.CreatedAtUtc).FirstOrDefault();
            var movieTitle = showtimeMovieMap.TryGetValue(x.ShowtimeId, out var movieId) && movieMap.TryGetValue(movieId, out var mappedTitle)
                ? mappedTitle
                : string.Empty;

            return new MoviesAdminRecentBookingDto
            {
                BookingId = x.Id,
                BookingCode = x.BookingCode,
                CustomerName = x.CustomerName,
                MovieTitle = movieTitle,
                CreatedAtUtc = x.CreatedAtUtc,
                GrandTotal = x.GrandTotal,
                Status = x.Status.ToString(),
                PaymentStatus = payment?.Status.ToString() ?? PaymentStatus.Pending.ToString()
            };
        }).ToList();

        var bookingsForRanking = await _bookingDbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .ToListAsync(cancellationToken);

        var topMovies = bookingsForRanking
            .Where(x => x.Payments.Any(payment => payment.Status == PaymentStatus.Succeeded))
            .GroupBy(x => showtimeMovieMap.GetValueOrDefault(x.ShowtimeId))
            .Where(x => x.Key != Guid.Empty)
            .Select(group => new MoviesAdminTopMovieDto
            {
                MovieId = group.Key,
                MovieTitle = movieMap.GetValueOrDefault(group.Key, "Unknown"),
                BookedSeats = group.Sum(booking => booking.Items.Count(item => item.ItemType == "Seat")),
                Revenue = group.Sum(booking => booking.Payments.Where(payment => payment.Status == PaymentStatus.Succeeded).Sum(payment => payment.Amount))
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToList();

        return new MoviesAdminDashboardResponseDto
        {
            ActiveMovies = activeMovies,
            UpcomingShowtimes = upcomingShowtimes,
            TotalBookings = totalBookings,
            PaidRevenue = paidRevenue,
            UpcomingShowtimesSnapshot = upcoming,
            RecentBookings = recentBookings,
            TopMovies = topMovies
        };
    }

    public async Task<IReadOnlyList<MoviesAdminMovieListItemDto>> GetMoviesAsync(CancellationToken cancellationToken = default)
    {
        var showtimeCounts = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .GroupBy(x => x.MovieId)
            .Select(x => new { x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        var movies = await _catalogDbContext.Movies
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return movies.Select(x => ToMovieDto(x, showtimeCounts.GetValueOrDefault(x.Id))).ToList();
    }

    public async Task<MoviesAdminMovieListItemDto?> GetMovieByIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var movie = await _catalogDbContext.Movies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == movieId, cancellationToken);
        if (movie is null)
        {
            return null;
        }

        var showtimeCount = await _catalogDbContext.Showtimes.CountAsync(x => x.MovieId == movieId, cancellationToken);
        return ToMovieDto(movie, showtimeCount);
    }

    public async Task<MoviesAdminMovieListItemDto> CreateMovieAsync(MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default)
    {
        ValidateMovieRequest(request);

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = await EnsureUniqueSlugAsync(request.Slug, request.Title, null, cancellationToken),
            Synopsis = Normalize(request.Synopsis),
            DurationMinutes = request.DurationMinutes,
            PosterUrl = Normalize(request.PosterUrl),
            TrailerUrl = Normalize(request.TrailerUrl),
            ReleaseDate = request.ReleaseDate,
            RatingLabel = Normalize(request.RatingLabel),
            DefaultLanguage = ParseLanguage(request.DefaultLanguage),
            Status = ParseMovieStatus(request.Status),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _catalogDbContext.Movies.Add(movie);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return ToMovieDto(movie, 0);
    }

    public async Task<MoviesAdminMovieListItemDto?> UpdateMovieAsync(Guid movieId, MoviesAdminMovieUpsertDto request, CancellationToken cancellationToken = default)
    {
        ValidateMovieRequest(request);

        var movie = await _catalogDbContext.Movies.FirstOrDefaultAsync(x => x.Id == movieId, cancellationToken);
        if (movie is null)
        {
            return null;
        }

        movie.Title = request.Title.Trim();
        movie.Slug = await EnsureUniqueSlugAsync(request.Slug, request.Title, movieId, cancellationToken);
        movie.Synopsis = Normalize(request.Synopsis);
        movie.DurationMinutes = request.DurationMinutes;
        movie.PosterUrl = Normalize(request.PosterUrl);
        movie.TrailerUrl = Normalize(request.TrailerUrl);
        movie.ReleaseDate = request.ReleaseDate;
        movie.RatingLabel = Normalize(request.RatingLabel);
        movie.DefaultLanguage = ParseLanguage(request.DefaultLanguage);
        movie.Status = ParseMovieStatus(request.Status);
        movie.UpdatedAtUtc = DateTime.UtcNow;

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        var showtimeCount = await _catalogDbContext.Showtimes.CountAsync(x => x.MovieId == movieId, cancellationToken);
        return ToMovieDto(movie, showtimeCount);
    }

    public async Task<bool> DeleteMovieAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        var movie = await _catalogDbContext.Movies.FirstOrDefaultAsync(x => x.Id == movieId, cancellationToken);
        if (movie is null)
        {
            return false;
        }

        movie.Status = MovieStatus.Inactive;
        movie.UpdatedAtUtc = DateTime.UtcNow;
        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<MoviesAdminShowtimeListItemDto>> GetShowtimesAsync(
        Guid? movieId,
        DateOnly? businessDate,
        CancellationToken cancellationToken = default)
    {
        var query = _catalogDbContext.Showtimes
            .AsNoTracking()
            .Include(x => x.Movie)
            .Include(x => x.Cinema)
            .Include(x => x.Hall)
            .AsQueryable();

        if (movieId.HasValue)
        {
            query = query.Where(x => x.MovieId == movieId.Value);
        }

        if (businessDate.HasValue)
        {
            query = query.Where(x => x.BusinessDate == businessDate.Value);
        }

        return await query
            .OrderBy(x => x.BusinessDate)
            .ThenBy(x => x.StartAtUtc)
            .Select(x => ToShowtimeDto(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<MoviesAdminShowtimeListItemDto> CreateShowtimeAsync(MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default)
    {
        var movie = await _catalogDbContext.Movies.FirstOrDefaultAsync(x => x.Id == request.MovieId, cancellationToken)
            ?? throw new InvalidOperationException("Movie does not exist.");
        var cinema = await _catalogDbContext.Cinemas.FirstOrDefaultAsync(x => x.Id == request.CinemaId, cancellationToken)
            ?? throw new InvalidOperationException("Cinema does not exist.");
        var hall = await _catalogDbContext.Halls.FirstOrDefaultAsync(x => x.Id == request.HallId && x.CinemaId == request.CinemaId, cancellationToken)
            ?? throw new InvalidOperationException("Hall does not exist for this cinema.");

        var endAtUtc = request.StartAtUtc.AddMinutes(movie.DurationMinutes + 15);
        await EnsureNoShowtimeConflictAsync(request.HallId, request.StartAtUtc, endAtUtc, null, cancellationToken);

        var showtime = new Showtime
        {
            Id = Guid.NewGuid(),
            MovieId = request.MovieId,
            CinemaId = request.CinemaId,
            HallId = request.HallId,
            BusinessDate = request.BusinessDate,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = endAtUtc,
            Language = ParseLanguage(request.Language),
            BasePrice = request.BasePrice,
            Status = ParseShowtimeStatus(request.Status),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Movie = movie,
            Cinema = cinema,
            Hall = hall
        };

        _catalogDbContext.Showtimes.Add(showtime);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        await CreateSeatInventoryAsync(showtime, cancellationToken);

        return ToShowtimeDto(showtime);
    }

    public async Task<MoviesAdminShowtimeListItemDto?> UpdateShowtimeAsync(Guid showtimeId, MoviesAdminShowtimeUpsertDto request, CancellationToken cancellationToken = default)
    {
        var showtime = await _catalogDbContext.Showtimes
            .Include(x => x.Movie)
            .Include(x => x.Cinema)
            .Include(x => x.Hall)
            .FirstOrDefaultAsync(x => x.Id == showtimeId, cancellationToken);

        if (showtime is null)
        {
            return null;
        }

        var movie = await _catalogDbContext.Movies.FirstOrDefaultAsync(x => x.Id == request.MovieId, cancellationToken)
            ?? throw new InvalidOperationException("Movie does not exist.");
        var cinema = await _catalogDbContext.Cinemas.FirstOrDefaultAsync(x => x.Id == request.CinemaId, cancellationToken)
            ?? throw new InvalidOperationException("Cinema does not exist.");
        var hall = await _catalogDbContext.Halls.FirstOrDefaultAsync(x => x.Id == request.HallId && x.CinemaId == request.CinemaId, cancellationToken)
            ?? throw new InvalidOperationException("Hall does not exist for this cinema.");

        var endAtUtc = request.StartAtUtc.AddMinutes(movie.DurationMinutes + 15);
        await EnsureNoShowtimeConflictAsync(request.HallId, request.StartAtUtc, endAtUtc, showtimeId, cancellationToken);

        var hallChanged = showtime.HallId != request.HallId;

        showtime.MovieId = request.MovieId;
        showtime.CinemaId = request.CinemaId;
        showtime.HallId = request.HallId;
        showtime.BusinessDate = request.BusinessDate;
        showtime.StartAtUtc = request.StartAtUtc;
        showtime.EndAtUtc = endAtUtc;
        showtime.Language = ParseLanguage(request.Language);
        showtime.BasePrice = request.BasePrice;
        showtime.Status = ParseShowtimeStatus(request.Status);
        showtime.UpdatedAtUtc = DateTime.UtcNow;
        showtime.Movie = movie;
        showtime.Cinema = cinema;
        showtime.Hall = hall;

        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        if (hallChanged)
        {
            await ResetSeatInventoryAsync(showtime, cancellationToken);
        }

        return ToShowtimeDto(showtime);
    }

    public async Task<bool> DeleteShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _catalogDbContext.Showtimes.FirstOrDefaultAsync(x => x.Id == showtimeId, cancellationToken);
        if (showtime is null)
        {
            return false;
        }

        showtime.Status = ShowtimeStatus.Cancelled;
        showtime.UpdatedAtUtc = DateTime.UtcNow;
        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _bookingDbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        var bookings = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(200)
            .ToListAsync(cancellationToken);

        var showtimeIds = bookings.Select(x => x.ShowtimeId).Distinct().ToArray();
        var showtimeMap = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .Include(x => x.Movie)
            .Include(x => x.Cinema)
            .Where(x => showtimeIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return bookings.Select(x =>
        {
            var payment = x.Payments.OrderByDescending(p => p.CreatedAtUtc).FirstOrDefault();
            showtimeMap.TryGetValue(x.ShowtimeId, out var showtime);

            return new MoviesAdminBookingListItemDto
            {
                Id = x.Id,
                BookingCode = x.BookingCode,
                ShowtimeId = x.ShowtimeId,
                MovieTitle = showtime?.Movie?.Title ?? string.Empty,
                CinemaName = showtime?.Cinema?.Name ?? string.Empty,
                ShowtimeStartAtUtc = showtime?.StartAtUtc ?? DateTime.MinValue,
                CustomerName = x.CustomerName,
                CustomerEmail = x.CustomerEmail,
                CustomerPhoneNumber = x.CustomerPhoneNumber,
                GrandTotal = x.GrandTotal,
                Currency = x.Currency,
                Status = x.Status.ToString(),
                PaymentStatus = payment?.Status.ToString() ?? PaymentStatus.Pending.ToString(),
                CreatedAtUtc = x.CreatedAtUtc
            };
        }).ToList();
    }

    public async Task<MoviesAdminBookingDetailDto?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingDbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);

        if (booking is null)
        {
            return null;
        }

        var showtime = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .Include(x => x.Movie)
            .Include(x => x.Cinema)
            .Include(x => x.Hall)
            .FirstOrDefaultAsync(x => x.Id == booking.ShowtimeId, cancellationToken);

        var payment = booking.Payments.OrderByDescending(x => x.CreatedAtUtc).FirstOrDefault();

        return new MoviesAdminBookingDetailDto
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            ShowtimeId = booking.ShowtimeId,
            MovieTitle = showtime?.Movie?.Title ?? string.Empty,
            CinemaName = showtime?.Cinema?.Name ?? string.Empty,
            HallName = showtime?.Hall?.Name ?? string.Empty,
            BusinessDate = showtime?.BusinessDate ?? default,
            StartAtUtc = showtime?.StartAtUtc ?? DateTime.MinValue,
            CustomerName = booking.CustomerName,
            CustomerEmail = booking.CustomerEmail,
            CustomerPhoneNumber = booking.CustomerPhoneNumber,
            SeatSubtotal = booking.SeatSubtotal,
            ComboSubtotal = booking.ComboSubtotal,
            ServiceFee = booking.ServiceFee,
            DiscountAmount = booking.DiscountAmount,
            GrandTotal = booking.GrandTotal,
            Currency = booking.Currency,
            Status = booking.Status.ToString(),
            PaymentStatus = payment?.Status.ToString() ?? PaymentStatus.Pending.ToString(),
            ProviderTransactionId = payment?.ProviderTransactionId,
            FailureReason = payment?.FailureReason,
            CreatedAtUtc = booking.CreatedAtUtc,
            Items = booking.Items.Select(item => new MoviesAdminBookingItemDto
            {
                ItemType = item.ItemType,
                ItemCode = item.ItemCode,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            }).ToList()
        };
    }

    public async Task<MoviesAdminLookupResponseDto> GetLookupsAsync(CancellationToken cancellationToken = default)
    {
        var movies = await _catalogDbContext.Movies
            .AsNoTracking()
            .Where(x => x.Status != MovieStatus.Inactive)
            .OrderBy(x => x.Title)
            .Select(x => new MoviesAdminLookupItemDto { Id = x.Id, Name = x.Title })
            .ToListAsync(cancellationToken);

        var cinemas = await _catalogDbContext.Cinemas
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new MoviesAdminLookupItemDto { Id = x.Id, Name = x.Name })
            .ToListAsync(cancellationToken);

        var halls = await _catalogDbContext.Halls
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new MoviesAdminHallLookupItemDto
            {
                Id = x.Id,
                CinemaId = x.CinemaId,
                Name = x.Name
            })
            .ToListAsync(cancellationToken);

        return new MoviesAdminLookupResponseDto
        {
            Movies = movies,
            Cinemas = cinemas,
            Halls = halls
        };
    }

    public async Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(
        string? status,
        string? provider,
        CancellationToken cancellationToken = default)
    {
        var payments = await _bookingDbContext.Payments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(300)
            .ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var parsedStatus))
        {
            payments = payments.Where(x => x.Status == parsedStatus).ToList();
        }

        if (!string.IsNullOrWhiteSpace(provider) && Enum.TryParse<PaymentProvider>(provider, true, out var parsedProvider))
        {
            payments = payments.Where(x => x.Provider == parsedProvider).ToList();
        }

        var bookings = await _bookingDbContext.Bookings
            .AsNoTracking()
            .Where(x => payments.Select(payment => payment.BookingId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var showtimeMap = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .Where(x => bookings.Values.Select(booking => booking.ShowtimeId).Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var movieMap = await _catalogDbContext.Movies
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);

        return payments.Select(payment =>
        {
            bookings.TryGetValue(payment.BookingId, out var booking);
            var movieTitle = booking is not null
                && showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime)
                && movieMap.TryGetValue(showtime.MovieId, out var title)
                ? title
                : string.Empty;

            return new MoviesAdminPaymentListItemDto
            {
                Id = payment.Id,
                BookingId = payment.BookingId,
                BookingCode = booking?.BookingCode ?? string.Empty,
                MovieTitle = movieTitle,
                CustomerEmail = booking?.CustomerEmail ?? string.Empty,
                Provider = payment.Provider.ToString(),
                Status = payment.Status.ToString(),
                Amount = payment.Amount,
                Currency = payment.Currency,
                ProviderTransactionId = payment.ProviderTransactionId,
                CreatedAtUtc = payment.CreatedAtUtc,
                UpdatedAtUtc = payment.UpdatedAtUtc,
                CompletedAtUtc = payment.CompletedAtUtc
            };
        }).ToList();
    }

    public async Task<MoviesAdminPaymentDetailDto?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default)
    {
        var payment = await _bookingDbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == paymentId, cancellationToken);

        if (payment is null)
        {
            return null;
        }

        var booking = await _bookingDbContext.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == payment.BookingId, cancellationToken);

        var movieTitle = string.Empty;
        if (booking is not null)
        {
            var showtime = await _catalogDbContext.Showtimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == booking.ShowtimeId, cancellationToken);
            if (showtime is not null)
            {
                movieTitle = await _catalogDbContext.Movies
                    .AsNoTracking()
                    .Where(x => x.Id == showtime.MovieId)
                    .Select(x => x.Title)
                    .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;
            }
        }

        return new MoviesAdminPaymentDetailDto
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            BookingCode = booking?.BookingCode ?? string.Empty,
            MovieTitle = movieTitle,
            CustomerEmail = booking?.CustomerEmail ?? string.Empty,
            Provider = payment.Provider.ToString(),
            Status = payment.Status.ToString(),
            Amount = payment.Amount,
            Currency = payment.Currency,
            ProviderTransactionId = payment.ProviderTransactionId,
            CreatedAtUtc = payment.CreatedAtUtc,
            UpdatedAtUtc = payment.UpdatedAtUtc,
            CompletedAtUtc = payment.CompletedAtUtc,
            FailureReason = payment.FailureReason,
            CallbackPayloadJson = payment.CallbackPayloadJson
        };
    }

    public async Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingDbContext.Bookings
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var showtimeMap = await _catalogDbContext.Showtimes
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var movieMap = await _catalogDbContext.Movies
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);

        var outboxItems = await _bookingDbContext.OutboxEvents
            .AsNoTracking()
            .Where(x => x.EventType == TicketEmailOutboxEvent.EventType)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        var outboxByBookingId = outboxItems
            .Select(item => new
            {
                Event = item,
                BookingId = TryReadBookingId(item.PayloadJson)
            })
            .Where(x => x.BookingId.HasValue)
            .GroupBy(x => x.BookingId!.Value)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.Event.OccurredAtUtc).First().Event);

        var tickets = await _bookingDbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToListAsync(cancellationToken);

        return tickets
            .GroupBy(x => x.BookingId)
            .Select(group =>
            {
                var latestTicket = group.OrderByDescending(x => x.IssuedAtUtc).First();
                bookings.TryGetValue(group.Key, out var booking);
                var movieTitle = booking is not null
                    && showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime)
                    && movieMap.TryGetValue(showtime.MovieId, out var title)
                    ? title
                    : string.Empty;
                outboxByBookingId.TryGetValue(group.Key, out var outbox);

                return new MoviesAdminEmailLogItemDto
                {
                    BookingId = group.Key,
                    BookingCode = booking?.BookingCode ?? string.Empty,
                    CustomerEmail = booking?.CustomerEmail ?? string.Empty,
                    MovieTitle = movieTitle,
                    DeliveryStatus = latestTicket.DeliveryStatus,
                    PdfFileName = latestTicket.PdfFileName,
                    IssuedAtUtc = latestTicket.IssuedAtUtc,
                    EmailSentAtUtc = latestTicket.EmailSentAtUtc,
                    EmailSendError = latestTicket.EmailSendError,
                    OutboxStatus = outbox?.Status ?? "NotQueued",
                    OutboxRetryCount = outbox?.RetryCount ?? 0,
                    OutboxLastError = outbox?.LastError
                };
            })
            .OrderByDescending(x => x.IssuedAtUtc)
            .ToList();
    }

    public async Task ResendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        await _ticketEmailDispatcher.SendTicketEmailAsync(bookingId, cancellationToken);
    }

    public async Task<MoviesAdminRevenueReportDto> GetRevenueReportAsync(
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
        Guid? movieId,
        Guid? cinemaId,
        string? provider,
        string? paymentStatus,
        CancellationToken cancellationToken = default)
    {
        var payments = await _bookingDbContext.Payments
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
        var bookings = await _bookingDbContext.Bookings.AsNoTracking().ToListAsync(cancellationToken);
        var showtimes = await _catalogDbContext.Showtimes.AsNoTracking().ToListAsync(cancellationToken);
        var movies = await _catalogDbContext.Movies.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Title, cancellationToken);
        var cinemas = await _catalogDbContext.Cinemas.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var bookingMap = bookings.ToDictionary(x => x.Id);
        var showtimeMap = showtimes.ToDictionary(x => x.Id);

        IEnumerable<Payment> filteredPayments = payments;

        if (dateFromUtc.HasValue)
        {
            filteredPayments = filteredPayments.Where(x => (x.CompletedAtUtc ?? x.CreatedAtUtc) >= dateFromUtc.Value);
        }

        if (dateToUtc.HasValue)
        {
            filteredPayments = filteredPayments.Where(x => (x.CompletedAtUtc ?? x.CreatedAtUtc) <= dateToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(provider) && Enum.TryParse<PaymentProvider>(provider, true, out var parsedProvider))
        {
            filteredPayments = filteredPayments.Where(x => x.Provider == parsedProvider);
        }

        if (!string.IsNullOrWhiteSpace(paymentStatus) && Enum.TryParse<PaymentStatus>(paymentStatus, true, out var parsedPaymentStatus))
        {
            filteredPayments = filteredPayments.Where(x => x.Status == parsedPaymentStatus);
        }

        if (movieId.HasValue)
        {
            filteredPayments = filteredPayments.Where(x =>
                bookingMap.TryGetValue(x.BookingId, out var booking)
                && showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime)
                && showtime.MovieId == movieId.Value);
        }

        if (cinemaId.HasValue)
        {
            filteredPayments = filteredPayments.Where(x =>
                bookingMap.TryGetValue(x.BookingId, out var booking)
                && showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime)
                && showtime.CinemaId == cinemaId.Value);
        }

        var filteredList = filteredPayments.ToList();

        var paidPayments = filteredList.Where(x => x.Status == PaymentStatus.Succeeded).ToList();

        return new MoviesAdminRevenueReportDto
        {
            DateFromUtc = dateFromUtc,
            DateToUtc = dateToUtc,
            TotalPaidRevenue = paidPayments.Sum(x => x.Amount),
            TotalBookings = filteredList
                .Select(x => x.BookingId)
                .Distinct()
                .Count(),
            SuccessfulPayments = filteredList.Count(x => x.Status == PaymentStatus.Succeeded),
            FailedPayments = filteredList.Count(x => x.Status == PaymentStatus.Failed),
            ByMovie = paidPayments
                .GroupBy(x =>
                {
                    if (bookingMap.TryGetValue(x.BookingId, out var booking) &&
                        showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime))
                    {
                        return movies.GetValueOrDefault(showtime.MovieId, "Unknown");
                    }

                    return "Unknown";
                })
                .Select(group => new MoviesAdminRevenueBreakdownDto
                {
                    Label = group.Key,
                    Revenue = group.Sum(x => x.Amount),
                    BookingCount = group.Select(x => x.BookingId).Distinct().Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList(),
            ByCinema = paidPayments
                .GroupBy(x =>
                {
                    if (bookingMap.TryGetValue(x.BookingId, out var booking) &&
                        showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime))
                    {
                        return cinemas.GetValueOrDefault(showtime.CinemaId, "Unknown");
                    }

                    return "Unknown";
                })
                .Select(group => new MoviesAdminRevenueBreakdownDto
                {
                    Label = group.Key,
                    Revenue = group.Sum(x => x.Amount),
                    BookingCount = group.Select(x => x.BookingId).Distinct().Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList(),
            ByProvider = paidPayments
                .GroupBy(x => x.Provider.ToString())
                .Select(group => new MoviesAdminRevenueBreakdownDto
                {
                    Label = group.Key,
                    Revenue = group.Sum(x => x.Amount),
                    BookingCount = group.Select(x => x.BookingId).Distinct().Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList()
        };
    }

    private async Task<string> EnsureUniqueSlugAsync(string? requestedSlug, string title, Guid? currentMovieId, CancellationToken cancellationToken)
    {
        var baseSlug = Slugify(string.IsNullOrWhiteSpace(requestedSlug) ? title : requestedSlug);
        var slug = baseSlug;
        var index = 1;

        while (await _catalogDbContext.Movies.AnyAsync(
                   x => x.Slug == slug && (!currentMovieId.HasValue || x.Id != currentMovieId.Value),
                   cancellationToken))
        {
            slug = $"{baseSlug}-{index++}";
        }

        return slug;
    }

    private async Task EnsureNoShowtimeConflictAsync(
        Guid hallId,
        DateTime startAtUtc,
        DateTime endAtUtc,
        Guid? currentShowtimeId,
        CancellationToken cancellationToken)
    {
        var hasConflict = await _catalogDbContext.Showtimes.AnyAsync(
            x => x.HallId == hallId
                 && x.Status != ShowtimeStatus.Cancelled
                 && (!currentShowtimeId.HasValue || x.Id != currentShowtimeId.Value)
                 && x.StartAtUtc < endAtUtc
                 && (x.EndAtUtc ?? x.StartAtUtc) > startAtUtc,
            cancellationToken);

        if (hasConflict)
        {
            throw new InvalidOperationException("This hall already has another showtime in the selected time range.");
        }
    }

    private async Task CreateSeatInventoryAsync(Showtime showtime, CancellationToken cancellationToken)
    {
        var hallSeats = await _catalogDbContext.HallSeats
            .AsNoTracking()
            .Where(x => x.HallId == showtime.HallId && x.IsActive)
            .ToListAsync(cancellationToken);

        if (hallSeats.Count == 0)
        {
            return;
        }

        var inventories = hallSeats.Select(seat => new ShowtimeSeatInventory
        {
            Id = Guid.NewGuid(),
            ShowtimeId = showtime.Id,
            HallSeatId = seat.Id,
            SeatCode = seat.SeatCode,
            RowLabel = seat.RowLabel,
            ColumnNumber = seat.ColumnNumber,
            SeatType = seat.SeatType,
            CoupleGroupCode = seat.CoupleGroupCode,
            Price = CalculateSeatPrice(showtime.BasePrice, seat.SeatType),
            Status = SeatInventoryStatus.Available,
            UpdatedAtUtc = DateTime.UtcNow
        }).ToList();

        await _catalogDbContext.ShowtimeSeatInventories.AddRangeAsync(inventories, cancellationToken);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ResetSeatInventoryAsync(Showtime showtime, CancellationToken cancellationToken)
    {
        var existing = await _catalogDbContext.ShowtimeSeatInventories
            .Where(x => x.ShowtimeId == showtime.Id)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            _catalogDbContext.ShowtimeSeatInventories.RemoveRange(existing);
            await _catalogDbContext.SaveChangesAsync(cancellationToken);
        }

        await CreateSeatInventoryAsync(showtime, cancellationToken);
    }

    private static MoviesAdminMovieListItemDto ToMovieDto(Movie movie, int showtimeCount)
    {
        return new MoviesAdminMovieListItemDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Slug = movie.Slug,
            Synopsis = movie.Synopsis,
            DurationMinutes = movie.DurationMinutes,
            PosterUrl = movie.PosterUrl,
            TrailerUrl = movie.TrailerUrl,
            ReleaseDate = movie.ReleaseDate,
            RatingLabel = movie.RatingLabel,
            DefaultLanguage = movie.DefaultLanguage.ToString(),
            Status = movie.Status.ToString(),
            ShowtimeCount = showtimeCount
        };
    }

    private static MoviesAdminShowtimeListItemDto ToShowtimeDto(Showtime showtime)
    {
        return new MoviesAdminShowtimeListItemDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title ?? string.Empty,
            CinemaId = showtime.CinemaId,
            CinemaName = showtime.Cinema?.Name ?? string.Empty,
            HallId = showtime.HallId,
            HallName = showtime.Hall?.Name ?? string.Empty,
            BusinessDate = showtime.BusinessDate,
            StartAtUtc = showtime.StartAtUtc,
            EndAtUtc = showtime.EndAtUtc,
            Language = showtime.Language.ToString(),
            BasePrice = showtime.BasePrice,
            Status = showtime.Status.ToString()
        };
    }

    private static decimal CalculateSeatPrice(decimal basePrice, SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Vip => Math.Round(basePrice * 1.2m, 2),
            SeatType.Couple => Math.Round(basePrice * 1.5m, 2),
            _ => basePrice
        };
    }

    private static void ValidateMovieRequest(MoviesAdminMovieUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Movie title is required.");
        }

        if (request.DurationMinutes <= 0)
        {
            throw new InvalidOperationException("Movie duration must be greater than 0.");
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Slugify(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        return string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    private static MovieStatus ParseMovieStatus(string value)
        => Enum.TryParse<MovieStatus>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException("Invalid movie status.");

    private static ShowtimeStatus ParseShowtimeStatus(string value)
        => Enum.TryParse<ShowtimeStatus>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException("Invalid showtime status.");

    private static LanguageType ParseLanguage(string value)
        => Enum.TryParse<LanguageType>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException("Invalid language.");

    private static Guid? TryReadBookingId(string payloadJson)
    {
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(payloadJson);
            var raw = document.RootElement.GetProperty("bookingId").GetString();
            return Guid.TryParse(raw, out var bookingId) ? bookingId : null;
        }
        catch
        {
            return null;
        }
    }
}
