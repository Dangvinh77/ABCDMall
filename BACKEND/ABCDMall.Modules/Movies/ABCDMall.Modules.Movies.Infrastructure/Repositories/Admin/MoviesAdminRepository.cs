using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    public async Task<IReadOnlyList<MoviesAdminPromotionListItemDto>> GetPromotionsAsync(
        string? status,
        string? query,
        bool activeOnly,
        CancellationToken cancellationToken = default)
    {
        var promotionsQuery = _bookingDbContext.Promotions
            .AsNoTracking()
            .Include(x => x.Rules)
            .Include(x => x.Redemptions)
            .AsQueryable();

        if (activeOnly)
        {
            promotionsQuery = promotionsQuery.Where(x => x.Status == PromotionStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PromotionStatus>(status, true, out var parsedStatus))
        {
            promotionsQuery = promotionsQuery.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            promotionsQuery = promotionsQuery.Where(x =>
                x.Code.Contains(normalizedQuery) ||
                x.Name.Contains(normalizedQuery) ||
                x.Description.Contains(normalizedQuery));
        }

        var promotions = await promotionsQuery
            .OrderByDescending(x => x.IsAutoApplied)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return promotions.Select(ToPromotionListItemDto).ToList();
    }

    public async Task<MoviesAdminPromotionDetailDto?> GetPromotionByIdAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        var promotion = await _bookingDbContext.Promotions
            .AsNoTracking()
            .Include(x => x.Rules.OrderBy(rule => rule.SortOrder))
            .Include(x => x.Redemptions)
            .FirstOrDefaultAsync(x => x.Id == promotionId, cancellationToken);

        return promotion is null ? null : ToPromotionDetailDto(promotion);
    }

    public async Task<MoviesAdminPromotionDetailDto> CreatePromotionAsync(MoviesAdminPromotionUpsertDto request, CancellationToken cancellationToken = default)
    {
        ValidatePromotionRequest(request);

        var normalizedCode = request.Code.Trim();
        var existingPromotion = await _bookingDbContext.Promotions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode, cancellationToken);

        if (existingPromotion is not null)
        {
            throw new InvalidOperationException(BuildPromotionCodeConflictMessage(existingPromotion));
        }

        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Status = ParsePromotionStatus(request.Status),
            ValidFromUtc = request.ValidFromUtc,
            ValidToUtc = request.ValidToUtc,
            PercentageValue = request.PercentageValue,
            FlatDiscountValue = request.FlatDiscountValue,
            MaximumDiscountAmount = request.MaximumDiscountAmount,
            MinimumSpendAmount = request.MinimumSpendAmount,
            MaxRedemptions = request.MaxRedemptions,
            MaxRedemptionsPerCustomer = request.MaxRedemptionsPerCustomer,
            IsAutoApplied = request.IsAutoApplied,
            MetadataJson = BuildPromotionMetadataJson(request),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
            Rules = request.Rules
                .OrderBy(rule => rule.SortOrder)
                .Select(rule => new PromotionRule
                {
                    Id = Guid.NewGuid(),
                    RuleType = ParsePromotionRuleType(rule.RuleType),
                    RuleValue = rule.RuleValue.Trim(),
                    ThresholdValue = rule.ThresholdValue,
                    SortOrder = rule.SortOrder,
                    IsRequired = rule.IsRequired
                })
                .ToList()
        };

        _bookingDbContext.Promotions.Add(promotion);
        await _bookingDbContext.SaveChangesAsync(cancellationToken);

        return ToPromotionDetailDto(promotion);
    }

    public async Task<MoviesAdminPromotionDetailDto?> UpdatePromotionAsync(Guid promotionId, MoviesAdminPromotionUpsertDto request, CancellationToken cancellationToken = default)
    {
        ValidatePromotionRequest(request);

        var promotion = await _bookingDbContext.Promotions
            .Include(x => x.Rules)
            .Include(x => x.Redemptions)
            .FirstOrDefaultAsync(x => x.Id == promotionId, cancellationToken);

        if (promotion is null)
        {
            return null;
        }

        var normalizedCode = request.Code.Trim();
        var existingPromotion = await _bookingDbContext.Promotions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == normalizedCode && x.Id != promotionId, cancellationToken);

        if (existingPromotion is not null)
        {
            throw new InvalidOperationException(BuildPromotionCodeConflictMessage(existingPromotion));
        }

        promotion.Code = normalizedCode;
        promotion.Name = request.Name.Trim();
        promotion.Description = request.Description.Trim();
        promotion.Status = ParsePromotionStatus(request.Status);
        promotion.ValidFromUtc = request.ValidFromUtc;
        promotion.ValidToUtc = request.ValidToUtc;
        promotion.PercentageValue = request.PercentageValue;
        promotion.FlatDiscountValue = request.FlatDiscountValue;
        promotion.MaximumDiscountAmount = request.MaximumDiscountAmount;
        promotion.MinimumSpendAmount = request.MinimumSpendAmount;
        promotion.MaxRedemptions = request.MaxRedemptions;
        promotion.MaxRedemptionsPerCustomer = request.MaxRedemptionsPerCustomer;
        promotion.IsAutoApplied = request.IsAutoApplied;
        promotion.MetadataJson = BuildPromotionMetadataJson(request);
        promotion.UpdatedAtUtc = DateTime.UtcNow;

        if (promotion.Rules.Count > 0)
        {
            _bookingDbContext.PromotionRules.RemoveRange(promotion.Rules);
        }

        promotion.Rules = request.Rules
            .OrderBy(rule => rule.SortOrder)
            .Select(rule => new PromotionRule
            {
                Id = Guid.NewGuid(),
                PromotionId = promotion.Id,
                RuleType = ParsePromotionRuleType(rule.RuleType),
                RuleValue = rule.RuleValue.Trim(),
                ThresholdValue = rule.ThresholdValue,
                SortOrder = rule.SortOrder,
                IsRequired = rule.IsRequired
            })
            .ToList();

        await _bookingDbContext.SaveChangesAsync(cancellationToken);
        return ToPromotionDetailDto(promotion);
    }

    public async Task<bool> DeletePromotionAsync(Guid promotionId, CancellationToken cancellationToken = default)
    {
        var promotion = await _bookingDbContext.Promotions.FirstOrDefaultAsync(x => x.Id == promotionId, cancellationToken);
        if (promotion is null)
        {
            return false;
        }

        promotion.Status = PromotionStatus.Inactive;
        promotion.UpdatedAtUtc = DateTime.UtcNow;
        await _bookingDbContext.SaveChangesAsync(cancellationToken);
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

    public async Task<MoviesAdminForceFinishShowtimeResponseDto?> ForceFinishShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)
    {
        var showtime = await _catalogDbContext.Showtimes.FirstOrDefaultAsync(x => x.Id == showtimeId, cancellationToken);
        if (showtime is null)
        {
            return null;
        }

        var utcNow = DateTime.UtcNow;
        var forcedEndAtUtc = utcNow.AddMinutes(-1);
        var previousEndAtUtc = showtime.EndAtUtc;
        var pendingRequests = await _bookingDbContext.MovieFeedbackRequests
            .Where(x => x.ShowtimeId == showtimeId && x.Status == MovieFeedbackRequestStatus.Pending)
            .ToListAsync(cancellationToken);

        showtime.EndAtUtc = forcedEndAtUtc;
        showtime.UpdatedAtUtc = utcNow;

        foreach (var pendingRequest in pendingRequests)
        {
            pendingRequest.AvailableAtUtc = forcedEndAtUtc;
            pendingRequest.UpdatedAtUtc = utcNow;
        }

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        await _bookingDbContext.SaveChangesAsync(cancellationToken);

        return new MoviesAdminForceFinishShowtimeResponseDto
        {
            ShowtimeId = showtime.Id,
            PreviousEndAtUtc = previousEndAtUtc,
            NewEndAtUtc = forcedEndAtUtc,
            Message = "Showtime end time moved to the past for feedback-email testing."
        };
    }

    public async Task<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto?> ForceExpireOpenedFeedbackRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _bookingDbContext.MovieFeedbackRequests.FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);
        if (request is null)
        {
            return null;
        }

        var utcNow = DateTime.UtcNow;
        var forcedOpenedAtUtc = utcNow.AddDays(-8);
        var previousFirstOpenedAtUtc = request.FirstOpenedAtUtc;
        var previousLastOpenedAtUtc = request.LastOpenedAtUtc;

        request.Status = MovieFeedbackRequestStatus.Sent;
        request.FirstOpenedAtUtc = forcedOpenedAtUtc;
        request.LastOpenedAtUtc = forcedOpenedAtUtc;
        request.ExpiredReason = null;
        request.InvalidatedAtUtc = null;
        request.UpdatedAtUtc = utcNow;

        await _bookingDbContext.SaveChangesAsync(cancellationToken);

        return new MoviesAdminForceExpireOpenedFeedbackRequestResponseDto
        {
            RequestId = request.Id,
            PreviousFirstOpenedAtUtc = previousFirstOpenedAtUtc,
            PreviousLastOpenedAtUtc = previousLastOpenedAtUtc,
            NewFirstOpenedAtUtc = forcedOpenedAtUtc,
            NewLastOpenedAtUtc = forcedOpenedAtUtc,
            Message = "Feedback request opened timestamps moved to the past for expiry testing."
        };
    }

    public async Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(
        string? status,
        string? paymentStatus,
        Guid? movieId,
        Guid? cinemaId,
        string? query,
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
        CancellationToken cancellationToken = default)
    {
        var bookingQuery = _bookingDbContext.Bookings
            .AsNoTracking()
            .Include(x => x.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, true, out var parsedStatus))
        {
            bookingQuery = bookingQuery.Where(x => x.Status == parsedStatus);
        }

        if (dateFromUtc.HasValue)
        {
            bookingQuery = bookingQuery.Where(x => x.CreatedAtUtc >= dateFromUtc.Value);
        }

        if (dateToUtc.HasValue)
        {
            bookingQuery = bookingQuery.Where(x => x.CreatedAtUtc <= dateToUtc.Value);
        }

        var bookings = await bookingQuery
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

        var bookingResults = bookings.Select(x =>
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
        });

        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            bookingResults = bookingResults.Where(x => string.Equals(x.PaymentStatus, paymentStatus, StringComparison.OrdinalIgnoreCase));
        }

        if (movieId.HasValue)
        {
            bookingResults = bookingResults.Where(x =>
                showtimeMap.TryGetValue(x.ShowtimeId, out var showtime)
                && showtime.MovieId == movieId.Value);
        }

        if (cinemaId.HasValue)
        {
            bookingResults = bookingResults.Where(x =>
                showtimeMap.TryGetValue(x.ShowtimeId, out var showtime)
                && showtime.CinemaId == cinemaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            bookingResults = bookingResults.Where(x =>
                ContainsIgnoreCase(x.BookingCode, normalizedQuery)
                || ContainsIgnoreCase(x.CustomerName, normalizedQuery)
                || ContainsIgnoreCase(x.CustomerEmail, normalizedQuery)
                || ContainsIgnoreCase(x.CustomerPhoneNumber, normalizedQuery)
                || ContainsIgnoreCase(x.MovieTitle, normalizedQuery)
                || ContainsIgnoreCase(x.CinemaName, normalizedQuery));
        }

        return bookingResults.ToList();
    }

    public Task<IReadOnlyList<MoviesAdminBookingListItemDto>> GetBookingsAsync(string? status, CancellationToken cancellationToken = default)
        => GetBookingsAsync(status, null, null, null, null, null, null, cancellationToken);

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
        Guid? movieId,
        Guid? cinemaId,
        string? query,
        DateTime? dateFromUtc,
        DateTime? dateToUtc,
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

        if (dateFromUtc.HasValue)
        {
            payments = payments.Where(x => (x.CompletedAtUtc ?? x.CreatedAtUtc) >= dateFromUtc.Value).ToList();
        }

        if (dateToUtc.HasValue)
        {
            payments = payments.Where(x => (x.CompletedAtUtc ?? x.CreatedAtUtc) <= dateToUtc.Value).ToList();
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

        var paymentResults = payments.Select(payment =>
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
        });

        if (movieId.HasValue)
        {
            paymentResults = paymentResults.Where(x =>
                bookings.TryGetValue(x.BookingId, out var booking)
                && showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime)
                && showtime.MovieId == movieId.Value);
        }

        if (cinemaId.HasValue)
        {
            paymentResults = paymentResults.Where(x =>
                bookings.TryGetValue(x.BookingId, out var booking)
                && showtimeMap.TryGetValue(booking.ShowtimeId, out var showtime)
                && showtime.CinemaId == cinemaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            paymentResults = paymentResults.Where(x =>
                ContainsIgnoreCase(x.BookingCode, normalizedQuery)
                || ContainsIgnoreCase(x.MovieTitle, normalizedQuery)
                || ContainsIgnoreCase(x.CustomerEmail, normalizedQuery)
                || ContainsIgnoreCase(x.ProviderTransactionId, normalizedQuery));
        }

        return paymentResults.ToList();
    }

    public Task<IReadOnlyList<MoviesAdminPaymentListItemDto>> GetPaymentsAsync(string? status, string? provider, CancellationToken cancellationToken = default)
        => GetPaymentsAsync(status, provider, null, null, null, null, null, cancellationToken);

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

    public async Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(
        string? query,
        string? deliveryStatus,
        string? outboxStatus,
        CancellationToken cancellationToken = default)
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

        IEnumerable<MoviesAdminEmailLogItemDto> emailLogs = tickets
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
            .OrderByDescending(x => x.IssuedAtUtc);

        if (!string.IsNullOrWhiteSpace(deliveryStatus))
        {
            emailLogs = emailLogs.Where(x => string.Equals(x.DeliveryStatus, deliveryStatus, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(outboxStatus))
        {
            emailLogs = emailLogs.Where(x => string.Equals(x.OutboxStatus, outboxStatus, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim();
            emailLogs = emailLogs.Where(x =>
                ContainsIgnoreCase(x.BookingCode, normalizedQuery)
                || ContainsIgnoreCase(x.CustomerEmail, normalizedQuery)
                || ContainsIgnoreCase(x.MovieTitle, normalizedQuery)
                || ContainsIgnoreCase(x.PdfFileName, normalizedQuery)
                || ContainsIgnoreCase(x.EmailSendError, normalizedQuery)
                || ContainsIgnoreCase(x.OutboxLastError, normalizedQuery));
        }

        return emailLogs.ToList();
    }

    public Task<IReadOnlyList<MoviesAdminEmailLogItemDto>> GetEmailLogsAsync(CancellationToken cancellationToken = default)
        => GetEmailLogsAsync(null, null, null, cancellationToken);

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

    private static MoviesAdminPromotionListItemDto ToPromotionListItemDto(Promotion promotion)
    {
        var metadata = ReadPromotionDisplayMetadata(promotion);

        return new MoviesAdminPromotionListItemDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            Description = promotion.Description,
            Category = metadata.Category,
            Status = promotion.Status.ToString(),
            ValidFromUtc = promotion.ValidFromUtc,
            ValidToUtc = promotion.ValidToUtc,
            PercentageValue = promotion.PercentageValue,
            FlatDiscountValue = promotion.FlatDiscountValue,
            MaximumDiscountAmount = promotion.MaximumDiscountAmount,
            MinimumSpendAmount = promotion.MinimumSpendAmount,
            MaxRedemptions = promotion.MaxRedemptions,
            MaxRedemptionsPerCustomer = promotion.MaxRedemptionsPerCustomer,
            IsAutoApplied = promotion.IsAutoApplied,
            ImageUrl = metadata.ImageUrl,
            BadgeText = metadata.BadgeText,
            AccentFrom = metadata.AccentFrom,
            AccentTo = metadata.AccentTo,
            DisplayCondition = metadata.DisplayCondition,
            IsFeatured = metadata.IsFeatured,
            DisplayPriority = metadata.DisplayPriority,
            RuleCount = promotion.Rules.Count,
            RedemptionCount = promotion.Redemptions.Count
        };
    }

    private static MoviesAdminPromotionDetailDto ToPromotionDetailDto(Promotion promotion)
    {
        var metadata = ReadPromotionDisplayMetadata(promotion);

        return new MoviesAdminPromotionDetailDto
        {
            Id = promotion.Id,
            Code = promotion.Code,
            Name = promotion.Name,
            Description = promotion.Description,
            Category = metadata.Category,
            Status = promotion.Status.ToString(),
            ValidFromUtc = promotion.ValidFromUtc,
            ValidToUtc = promotion.ValidToUtc,
            PercentageValue = promotion.PercentageValue,
            FlatDiscountValue = promotion.FlatDiscountValue,
            MaximumDiscountAmount = promotion.MaximumDiscountAmount,
            MinimumSpendAmount = promotion.MinimumSpendAmount,
            MaxRedemptions = promotion.MaxRedemptions,
            MaxRedemptionsPerCustomer = promotion.MaxRedemptionsPerCustomer,
            IsAutoApplied = promotion.IsAutoApplied,
            ImageUrl = metadata.ImageUrl,
            BadgeText = metadata.BadgeText,
            AccentFrom = metadata.AccentFrom,
            AccentTo = metadata.AccentTo,
            DisplayCondition = metadata.DisplayCondition,
            IsFeatured = metadata.IsFeatured,
            DisplayPriority = metadata.DisplayPriority,
            MetadataJson = promotion.MetadataJson,
            RuleCount = promotion.Rules.Count,
            RedemptionCount = promotion.Redemptions.Count,
            Rules = promotion.Rules
                .OrderBy(rule => rule.SortOrder)
                .Select(rule => new MoviesAdminPromotionRuleDto
                {
                    RuleType = rule.RuleType.ToString(),
                    RuleValue = rule.RuleValue,
                    ThresholdValue = rule.ThresholdValue,
                    SortOrder = rule.SortOrder,
                    IsRequired = rule.IsRequired
                })
                .ToList()
        };
    }

    private static void ValidatePromotionRequest(MoviesAdminPromotionUpsertDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new InvalidOperationException("Promotion code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Promotion name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("Promotion description is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Category))
        {
            throw new InvalidOperationException("Promotion category is required.");
        }

        if (!request.PercentageValue.HasValue && !request.FlatDiscountValue.HasValue)
        {
            throw new InvalidOperationException("Promotion must define a percentage or flat discount.");
        }

        if (request.PercentageValue.HasValue && (request.PercentageValue <= 0 || request.PercentageValue > 100))
        {
            throw new InvalidOperationException("Percentage discount must be between 0 and 100.");
        }

        if (request.FlatDiscountValue.HasValue && request.FlatDiscountValue <= 0)
        {
            throw new InvalidOperationException("Flat discount must be greater than 0.");
        }

        if (request.ValidFromUtc.HasValue && request.ValidToUtc.HasValue && request.ValidFromUtc > request.ValidToUtc)
        {
            throw new InvalidOperationException("Promotion validity range is invalid.");
        }

        foreach (var rule in request.Rules)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleType))
            {
                throw new InvalidOperationException("Promotion rule type is required.");
            }

            if (string.IsNullOrWhiteSpace(rule.RuleValue))
            {
                throw new InvalidOperationException("Promotion rule value is required.");
            }
        }

        _ = BuildPromotionMetadataJson(request);
    }

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

    private static PromotionStatus ParsePromotionStatus(string value)
        => Enum.TryParse<PromotionStatus>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException("Invalid promotion status.");

    private static PromotionRuleType ParsePromotionRuleType(string value)
        => Enum.TryParse<PromotionRuleType>(value, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException("Invalid promotion rule type.");

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

    private static bool ContainsIgnoreCase(string? source, string value)
        => !string.IsNullOrWhiteSpace(source)
           && source.Contains(value, StringComparison.OrdinalIgnoreCase);

    private static string BuildPromotionMetadataJson(MoviesAdminPromotionUpsertDto request)
    {
        JsonObject root = [];

        if (!string.IsNullOrWhiteSpace(request.MetadataJson))
        {
            try
            {
                root = JsonNode.Parse(request.MetadataJson)?.AsObject() ?? [];
            }
            catch (Exception) when (request.MetadataJson is not null)
            {
                throw new InvalidOperationException("Metadata JSON must be a valid JSON object.");
            }
        }

        root["category"] = NormalizeCategory(request.Category);
        WriteOptional(root, "imageUrl", request.ImageUrl);
        WriteOptional(root, "badgeText", request.BadgeText);
        WriteOptional(root, "accentFrom", request.AccentFrom);
        WriteOptional(root, "accentTo", request.AccentTo);
        WriteOptional(root, "displayCondition", request.DisplayCondition);
        root["isFeatured"] = request.IsFeatured;
        root["displayPriority"] = request.DisplayPriority;

        return root.ToJsonString();
    }

    private static string BuildPromotionCodeConflictMessage(Promotion promotion)
        => promotion.Status == PromotionStatus.Inactive
            ? "Promotion code already exists in an inactive campaign. Turn off 'Active only' and edit the existing promotion instead."
            : $"Promotion code already exists in a {promotion.Status} campaign.";

    private static PromotionDisplayMetadata ReadPromotionDisplayMetadata(Promotion promotion)
    {
        var metadata = new PromotionDisplayMetadata
        {
            Category = InferPromotionCategory(promotion),
            BadgeText = promotion.Code,
            DisplayCondition = promotion.IsAutoApplied
                ? "Applied automatically when eligible"
                : "Select this offer before checkout"
        };

        if (string.IsNullOrWhiteSpace(promotion.MetadataJson))
        {
            return metadata;
        }

        try
        {
            using var document = JsonDocument.Parse(promotion.MetadataJson);
            metadata.Category = ReadString(document.RootElement, "category") is { Length: > 0 } category
                ? NormalizeCategory(category)
                : metadata.Category;
            metadata.ImageUrl = ReadString(document.RootElement, "imageUrl");
            metadata.BadgeText = ReadString(document.RootElement, "badgeText") ?? metadata.BadgeText;
            metadata.AccentFrom = ReadString(document.RootElement, "accentFrom");
            metadata.AccentTo = ReadString(document.RootElement, "accentTo");
            metadata.DisplayCondition = ReadString(document.RootElement, "displayCondition") ?? metadata.DisplayCondition;
            metadata.IsFeatured = ReadBool(document.RootElement, "isFeatured");
            metadata.DisplayPriority = ReadInt(document.RootElement, "displayPriority");
        }
        catch (JsonException)
        {
            return metadata;
        }

        return metadata;
    }

    private static string InferPromotionCategory(Promotion promotion)
    {
        if (promotion.Rules.Any(rule => rule.RuleType == PromotionRuleType.Combo))
        {
            return "combo";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == PromotionRuleType.PaymentProvider))
        {
            return "bank";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == PromotionRuleType.BusinessDate))
        {
            return "weekend";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == PromotionRuleType.BirthdayMonth))
        {
            return "member";
        }

        if (promotion.Rules.Any(rule => rule.RuleType == PromotionRuleType.SeatType))
        {
            return "ticket";
        }

        return "all";
    }

    private static string NormalizeCategory(string value)
        => string.IsNullOrWhiteSpace(value) ? "all" : value.Trim().ToLowerInvariant();

    private static void WriteOptional(JsonObject root, string propertyName, string? value)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            root.Remove(propertyName);
            return;
        }

        root[propertyName] = normalized;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() : null;
    }

    private static bool ReadBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        return property.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => false
        };
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return 0;
        }

        return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : 0;
    }

    private sealed class PromotionDisplayMetadata
    {
        public string Category { get; set; } = "all";
        public string? ImageUrl { get; set; }
        public string? BadgeText { get; set; }
        public string? AccentFrom { get; set; }
        public string? AccentTo { get; set; }
        public string? DisplayCondition { get; set; }
        public bool IsFeatured { get; set; }
        public int DisplayPriority { get; set; }
    }
}
