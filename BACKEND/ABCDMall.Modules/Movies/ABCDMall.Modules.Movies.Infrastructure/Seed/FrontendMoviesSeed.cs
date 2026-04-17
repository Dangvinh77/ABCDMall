using System.Security.Cryptography;
using System.Text;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Seed;

public static class FrontendMoviesSeed
{
    public static async Task SeedCatalogAsync(MoviesCatalogDbContext db, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        await SeedGenresAsync(db, ct);
        await SeedCinemasAsync(db, now, ct);
        await SeedMoviesAsync(db, now, today, ct);
        await SeedMovieGenresAsync(db, ct);
        await SeedShowtimesAsync(db, now, today, ct);
    }

    public static async Task SeedBookingAsync(MoviesBookingDbContext db, CancellationToken ct = default)
    {
        // Frontend demo seed owns the full booking-side demo dataset:
        // snack combos, promotions, and promotion rules.
        var now = DateTime.UtcNow;
        var comboIds = await SeedSnackCombosAsync(db, now, ct);
        await SeedPromotionsAsync(db, comboIds, now, ct);
    }

    private static async Task SeedGenresAsync(MoviesCatalogDbContext db, CancellationToken ct)
    {
        var existing = await db.Genres.Select(x => x.Name).ToListAsync(ct);
        var genres = MovieSeeds
            .SelectMany(x => SplitGenres(x.Genres))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(x => !existing.Contains(x, StringComparer.OrdinalIgnoreCase))
            .Select(x => new Genre
            {
                Id = Id($"genre:{x}"),
                Name = x,
                Description = $"{x} movies from frontend hardcoded data."
            })
            .ToList();

        if (genres.Count == 0) return;
        await db.Genres.AddRangeAsync(genres, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedCinemasAsync(MoviesCatalogDbContext db, DateTime now, CancellationToken ct)
    {
        var seedByCode = CinemaSeeds.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var existingCodes = await db.Cinemas.Select(x => x.Code).ToListAsync(ct);
        var cinemas = CinemaSeeds
            .Where(x => !existingCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
            .Select(x => new Cinema
            {
                Id = Id($"cinema:{x.Code}"),
                Code = x.Code,
                Name = x.Name,
                AddressLine1 = x.Address,
                City = x.City,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            })
            .ToList();

        if (cinemas.Count > 0)
        {
            await db.Cinemas.AddRangeAsync(cinemas, ct);
            await db.SaveChangesAsync(ct);
        }

        var seededCinemas = await db.Cinemas
            .Where(x => CinemaSeeds.Select(seed => seed.Code).Contains(x.Code))
            .ToListAsync(ct);

        foreach (var cinema in seededCinemas)
        {
            var seed = seedByCode[cinema.Code];
            cinema.Name = seed.Name;
            cinema.AddressLine1 = seed.Address;
            cinema.City = seed.City;
            cinema.IsActive = true;
            cinema.UpdatedAtUtc = now;
        }

        await db.SaveChangesAsync(ct);

        var cinemaByCode = seededCinemas.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var existingHallKeys = await db.Halls.Select(x => x.CinemaId + "|" + x.HallCode).ToListAsync(ct);
        var halls = new List<Hall>();

        foreach (var cinema in CinemaSeeds)
        {
            foreach (var hallType in cinema.HallTypes)
            {
                var cinemaId = cinemaByCode[cinema.Code];
                var key = cinemaId + "|" + hallType;
                if (existingHallKeys.Contains(key)) continue;

                halls.Add(new Hall
                {
                    Id = Id($"hall:{cinema.Code}:{hallType}"),
                    CinemaId = cinemaId,
                    HallCode = hallType,
                    Name = $"{hallType} Hall - {cinema.Name}",
                    HallType = ToHallType(hallType),
                    SeatCapacity = 60,
                    IsActive = true
                });
            }
        }

        if (halls.Count > 0)
        {
            await db.Halls.AddRangeAsync(halls, ct);
            await db.SaveChangesAsync(ct);
        }

        var seededHalls = await db.Halls
            .Where(x => cinemaByCode.Values.Contains(x.CinemaId))
            .ToListAsync(ct);

        foreach (var hall in seededHalls)
        {
            if (await db.HallSeats.AnyAsync(x => x.HallId == hall.Id, ct)) continue;
            await db.HallSeats.AddRangeAsync(BuildSeats(hall), ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedMoviesAsync(MoviesCatalogDbContext db, DateTime now, DateOnly today, CancellationToken ct)
    {
        var seedByKey = MovieSeeds
            .Select((seed, index) => new { seed, index })
            .ToDictionary(x => x.seed.Key, StringComparer.OrdinalIgnoreCase);
        var existing = await db.Movies.Select(x => x.Slug).ToListAsync(ct);
        var movies = MovieSeeds
            .Where(x => !existing.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
            .Select((x, index) => new Movie
            {
                Id = Id($"movie:{x.Key}"),
                Title = x.Title,
                Slug = x.Key,
                Synopsis = x.Synopsis,
                DurationMinutes = x.DurationMinutes,
                PosterUrl = x.PosterUrl,
                TrailerUrl = $"https://example.com/trailers/{x.Key}.mp4",
                ReleaseDate = x.IsComingSoon ? today.AddDays(10 + index) : today.AddDays(-20 + index),
                RatingLabel = $"{x.Rating:0.0} {x.AgeRating}",
                DefaultLanguage = x.Language.Contains("Vietnamese", StringComparison.OrdinalIgnoreCase) ? LanguageType.Dubbed : LanguageType.Subtitle,
                Status = x.IsComingSoon ? MovieStatus.ComingSoon : MovieStatus.NowShowing,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            })
            .ToList();

        if (movies.Count > 0)
        {
            await db.Movies.AddRangeAsync(movies, ct);
            await db.SaveChangesAsync(ct);
        }

        var seededMovies = await db.Movies
            .Where(x => MovieSeeds.Select(seed => seed.Key).Contains(x.Slug))
            .ToListAsync(ct);

        foreach (var movie in seededMovies)
        {
            var indexedSeed = seedByKey[movie.Slug];
            var seed = indexedSeed.seed;
            movie.Title = seed.Title;
            movie.Synopsis = seed.Synopsis;
            movie.DurationMinutes = seed.DurationMinutes;
            movie.PosterUrl = seed.PosterUrl;
            movie.TrailerUrl = $"https://example.com/trailers/{seed.Key}.mp4";
            movie.ReleaseDate = seed.IsComingSoon ? today.AddDays(10 + indexedSeed.index) : today.AddDays(-20 + indexedSeed.index);
            movie.RatingLabel = $"{seed.Rating:0.0} {seed.AgeRating}";
            movie.DefaultLanguage = seed.Language.Contains("Vietnamese", StringComparison.OrdinalIgnoreCase) ? LanguageType.Dubbed : LanguageType.Subtitle;
            movie.Status = seed.IsComingSoon ? MovieStatus.ComingSoon : MovieStatus.NowShowing;
            movie.UpdatedAtUtc = now;
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedMovieGenresAsync(MoviesCatalogDbContext db, CancellationToken ct)
    {
        var genres = await db.Genres.ToDictionaryAsync(x => x.Name, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);
        var movies = await db.Movies
            .Where(x => MovieSeeds.Select(seed => seed.Key).Contains(x.Slug))
            .ToDictionaryAsync(x => x.Slug, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);
        var existing = await db.MovieGenres.Select(x => x.MovieId + "|" + x.GenreId).ToListAsync(ct);
        var links = new List<MovieGenre>();

        foreach (var seed in MovieSeeds)
        {
            if (!movies.TryGetValue(seed.Key, out var movieId)) continue;
            foreach (var genre in SplitGenres(seed.Genres))
            {
                var genreId = genres[genre];
                var key = movieId + "|" + genreId;
                if (existing.Contains(key)) continue;
                links.Add(new MovieGenre { MovieId = movieId, GenreId = genreId });
            }
        }

        if (links.Count == 0) return;
        await db.MovieGenres.AddRangeAsync(links, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedShowtimesAsync(MoviesCatalogDbContext db, DateTime now, DateOnly today, CancellationToken ct)
    {
        var movies = await db.Movies
            .Where(x => MovieSeeds.Select(seed => seed.Key).Contains(x.Slug))
            .ToDictionaryAsync(x => x.Slug, x => x, StringComparer.OrdinalIgnoreCase, ct);
        var cinemas = await db.Cinemas
            .Where(x => CinemaSeeds.Select(seed => seed.Code).Contains(x.Code))
            .ToDictionaryAsync(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase, ct);
        var halls = await db.Halls.ToListAsync(ct);
        var hallByCinemaAndType = halls.ToDictionary(x => x.CinemaId + "|" + x.HallCode, x => x);
        var existingShowtimes = await db.Showtimes.Select(x => x.Id).ToListAsync(ct);
        var showtimes = new List<Showtime>();

        foreach (var movieSeed in MovieSeeds.Where(x => !x.IsComingSoon))
        {
            var movie = movies[movieSeed.Key];
            foreach (var cinemaCode in movieSeed.CinemaCodes)
            {
                var cinemaSeed = CinemaSeeds.Single(x => x.Code == cinemaCode);
                var cinema = cinemas[cinemaCode];
                for (var day = 0; day < 7; day++)
                {
                    var date = today.AddDays(day);
                    for (var index = 0; index < cinemaSeed.Showtimes.Length; index++)
                    {
                        var hallType = cinemaSeed.HallTypes[index % cinemaSeed.HallTypes.Length];
                        var hall = hallByCinemaAndType[cinema.Id + "|" + hallType];
                        var id = Id($"showtime:{movieSeed.Key}:{cinemaCode}:{date:yyyyMMdd}:{cinemaSeed.Showtimes[index]}:{hallType}");
                        if (existingShowtimes.Contains(id)) continue;

                        var start = date.ToDateTime(TimeOnly.Parse(cinemaSeed.Showtimes[index]), DateTimeKind.Utc);
                        showtimes.Add(new Showtime
                        {
                            Id = id,
                            MovieId = movie.Id,
                            CinemaId = cinema.Id,
                            HallId = hall.Id,
                            BusinessDate = date,
                            StartAtUtc = start,
                            EndAtUtc = start.AddMinutes(movie.DurationMinutes + 15),
                            Language = movie.DefaultLanguage,
                            BasePrice = BasePrice(hallType),
                            Status = ShowtimeStatus.Open,
                            CreatedAtUtc = now,
                            UpdatedAtUtc = now
                        });
                    }
                }
            }
        }

        if (showtimes.Count > 0)
        {
            await db.Showtimes.AddRangeAsync(showtimes, ct);
            await db.SaveChangesAsync(ct);
        }

        var hallSeatList = await db.HallSeats
            .Where(x => halls.Select(hall => hall.Id).Contains(x.HallId))
            .ToListAsync(ct);
        var seatsByHall = hallSeatList
            .GroupBy(x => x.HallId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var seededShowtimes = await db.Showtimes
            .Where(x => movies.Values.Select(movie => movie.Id).Contains(x.MovieId) && cinemas.Values.Select(cinema => cinema.Id).Contains(x.CinemaId))
            .ToListAsync(ct);
        var existingInventoryKeys = await db.ShowtimeSeatInventories
            .Where(x => seededShowtimes.Select(showtime => showtime.Id).Contains(x.ShowtimeId))
            .Select(x => x.ShowtimeId + "|" + x.SeatCode)
            .ToListAsync(ct);
        var inventories = new List<ShowtimeSeatInventory>();

        foreach (var showtime in seededShowtimes)
        {
            if (!seatsByHall.TryGetValue(showtime.HallId, out var hallSeats)) continue;

            foreach (var seat in hallSeats)
            {
                var key = showtime.Id + "|" + seat.SeatCode;
                if (existingInventoryKeys.Contains(key)) continue;

                inventories.Add(new ShowtimeSeatInventory
                {
                    Id = Id($"inventory:{showtime.Id}:{seat.SeatCode}"),
                    ShowtimeId = showtime.Id,
                    HallSeatId = seat.Id,
                    SeatCode = seat.SeatCode,
                    RowLabel = seat.RowLabel,
                    ColumnNumber = seat.ColumnNumber,
                    SeatType = seat.SeatType,
                    CoupleGroupCode = seat.CoupleGroupCode,
                    Price = SeatPrice(showtime.BasePrice, seat.SeatType),
                    Status = seat.RowLabel == "A" && seat.ColumnNumber <= 2 && showtime.BusinessDate == today ? SeatInventoryStatus.Booked : SeatInventoryStatus.Available,
                    UpdatedAtUtc = now
                });
            }
        }

        if (inventories.Count == 0) return;
        await db.ShowtimeSeatInventories.AddRangeAsync(inventories, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task<Dictionary<string, Guid>> SeedSnackCombosAsync(MoviesBookingDbContext db, DateTime now, CancellationToken ct)
    {
        var seedByCode = SnackComboSeeds.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var existing = await db.SnackCombos.Select(x => x.Code).ToListAsync(ct);
        var combos = SnackComboSeeds
            .Where(x => !existing.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
            .Select(x => new SnackCombo
            {
                Id = Id($"combo:{x.Code}"),
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            })
            .ToList();

        if (combos.Count > 0)
        {
            await db.SnackCombos.AddRangeAsync(combos, ct);
            await db.SaveChangesAsync(ct);
        }

        var seededCombos = await db.SnackCombos
            .Where(x => SnackComboSeeds.Select(seed => seed.Code).Contains(x.Code))
            .ToListAsync(ct);

        foreach (var combo in seededCombos)
        {
            var seed = seedByCode[combo.Code];
            combo.Name = seed.Name;
            combo.Description = seed.Description;
            combo.Price = seed.Price;
            combo.IsActive = true;
            combo.UpdatedAtUtc = now;
        }

        await db.SaveChangesAsync(ct);

        return seededCombos.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);
    }

    private static async Task SeedPromotionsAsync(MoviesBookingDbContext db, IReadOnlyDictionary<string, Guid> comboIds, DateTime now, CancellationToken ct)
    {
        var seedByCode = PromotionSeeds.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var validFromUtc = new DateTimeOffset(now).AddDays(-7);
        var validToUtc = new DateTimeOffset(now).AddMonths(6);
        var existing = await db.Promotions.Select(x => x.Code).ToListAsync(ct);
        var promotions = PromotionSeeds
            .Where(x => !existing.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
            .Select(x => new Promotion
            {
                Id = Id($"promotion:{x.Code}"),
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Status = PromotionStatus.Active,
                ValidFromUtc = validFromUtc,
                ValidToUtc = validToUtc,
                PercentageValue = x.PercentDiscount,
                FlatDiscountValue = x.FlatDiscount,
                MaximumDiscountAmount = x.MaxDiscount,
                MinimumSpendAmount = x.MinimumSpend,
                MaxRedemptionsPerCustomer = x.MaxRedemptionsPerCustomer,
                IsAutoApplied = x.IsAutoApplied,
                MetadataJson = $$"""{"category":"{{x.Category}}"}""",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            })
            .ToList();

        if (promotions.Count > 0)
        {
            await db.Promotions.AddRangeAsync(promotions, ct);
            await db.SaveChangesAsync(ct);
        }

        var seededPromotions = await db.Promotions
            .Where(x => PromotionSeeds.Select(seed => seed.Code).Contains(x.Code))
            .ToListAsync(ct);

        foreach (var promotion in seededPromotions)
        {
            var seed = seedByCode[promotion.Code];
            promotion.Name = seed.Name;
            promotion.Description = seed.Description;
            promotion.Status = PromotionStatus.Active;
            promotion.ValidFromUtc = validFromUtc;
            promotion.ValidToUtc = validToUtc;
            promotion.PercentageValue = seed.PercentDiscount;
            promotion.FlatDiscountValue = seed.FlatDiscount;
            promotion.MaximumDiscountAmount = seed.MaxDiscount;
            promotion.MinimumSpendAmount = seed.MinimumSpend;
            promotion.MaxRedemptionsPerCustomer = seed.MaxRedemptionsPerCustomer;
            promotion.IsAutoApplied = seed.IsAutoApplied;
            promotion.MetadataJson = $$"""{"category":"{{seed.Category}}"}""";
            promotion.UpdatedAtUtc = now;
        }

        await db.SaveChangesAsync(ct);

        var promotionByCode = seededPromotions.ToDictionary(x => x.Code, x => x.Id, StringComparer.OrdinalIgnoreCase);
        var existingRules = await db.PromotionRules.Select(x => x.PromotionId + "|" + x.RuleType + "|" + x.RuleValue).ToListAsync(ct);
        var rules = new List<PromotionRule>();

        foreach (var promotion in PromotionSeeds)
        {
            var promotionId = promotionByCode[promotion.Code];
            foreach (var rule in promotion.Rules)
            {
                var ruleValue = rule.RuleValue == "combo-gold" ? comboIds["combo-gold"].ToString() : rule.RuleValue;
                var key = promotionId + "|" + rule.RuleType + "|" + ruleValue;
                if (existingRules.Contains(key)) continue;
                rules.Add(new PromotionRule
                {
                    Id = Id($"rule:{key}"),
                    PromotionId = promotionId,
                    RuleType = rule.RuleType,
                    RuleValue = ruleValue,
                    SortOrder = rule.SortOrder,
                    IsRequired = true
                });
            }
        }

        if (rules.Count == 0) return;
        await db.PromotionRules.AddRangeAsync(rules, ct);
        await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<HallSeat> BuildSeats(Hall hall)
    {
        var seats = new List<HallSeat>();
        foreach (var row in new[] { "A", "B", "C", "D", "E", "F" })
        {
            for (var col = 1; col <= 10; col++)
            {
                var isCouple = row == "F" && col % 2 == 1;
                var group = isCouple ? $"{hall.HallCode}-{row}-{col}" : null;
                seats.Add(CreateSeat(hall, row, col, isCouple ? SeatType.Couple : row is "D" or "E" ? SeatType.Vip : SeatType.Regular, group));
                if (!isCouple || col >= 10) continue;
                col++;
                seats.Add(CreateSeat(hall, row, col, SeatType.Couple, group));
            }
        }

        return seats;
    }

    private static HallSeat CreateSeat(Hall hall, string row, int col, SeatType type, string? group)
    {
        return new HallSeat
        {
            Id = Id($"seat:{hall.Id}:{row}{col}"),
            HallId = hall.Id,
            SeatCode = $"{row}{col}",
            RowLabel = row,
            ColumnNumber = col,
            SeatType = type,
            CoupleGroupCode = group,
            IsActive = true
        };
    }

    private static Guid Id(string value) => new(MD5.HashData(Encoding.UTF8.GetBytes($"abcd-mall-fe:{value}")));

    private static string[] SplitGenres(string value) => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static HallType ToHallType(string value) => value switch { "3D" => HallType.Standard3D, "IMAX" => HallType.Imax, "4DX" => HallType.FourDx, _ => HallType.Standard2D };

    private static decimal BasePrice(string value) => value switch { "3D" => 120000, "IMAX" => 140000, "4DX" => 160000, _ => 85000 };

    private static decimal SeatPrice(decimal basePrice, SeatType type) => type switch { SeatType.Vip => basePrice + 35000, SeatType.Couple => basePrice + 10000, _ => basePrice };

    private sealed record MovieSeedItem(string Key, string Title, string Synopsis, string Genres, double Rating, int DurationMinutes, string Director, string[] Cast, string Language, string AgeRating, string PosterUrl, bool IsComingSoon, string[] CinemaCodes);
    private sealed record CinemaSeedItem(string Code, string Name, string Address, string City, string[] Showtimes, string[] HallTypes);
    private sealed record SnackComboSeedItem(string Code, string Name, string Description, decimal Price);
    private sealed record PromotionSeedItem(string Code, string Name, string Description, string Category, decimal? PercentDiscount, decimal? FlatDiscount, decimal? MaxDiscount, decimal? MinimumSpend, int? MaxRedemptionsPerCustomer, bool IsAutoApplied, PromotionRuleSeedItem[] Rules);
    private sealed record PromotionRuleSeedItem(PromotionRuleType RuleType, string RuleValue, int SortOrder);

    private static readonly CinemaSeedItem[] CinemaSeeds =
    [
        new("abcd-mall", "ABCD Cinema - ABCD Mall", "Level 5, ABCD Mall, 123 Le Van Viet Street", "Ho Chi Minh City", ["09:30", "12:00", "14:30", "17:00", "19:30", "22:00"], ["2D", "3D", "IMAX"]),
        new("quan-1", "ABCD Cinema - District 1", "456 Nguyen Hue Boulevard, District 1", "Ho Chi Minh City", ["10:00", "13:15", "16:00", "19:00", "21:45"], ["2D", "3D"]),
        new("binh-duong", "ABCD Cinema - Binh Duong", "Level 3, Aeon Mall Binh Duong, Thuan An", "Binh Duong", ["08:45", "11:30", "14:00", "16:45", "19:15", "21:30"], ["2D", "4DX"]),
        new("go-vap", "ABCD Cinema - Go Vap", "789 Phan Van Tri Street, Go Vap", "Ho Chi Minh City", ["10:30", "13:00", "15:30", "18:00", "20:30"], ["2D", "3D"])
    ];

    private static readonly MovieSeedItem[] MovieSeeds =
    [
        new("cosmic-odyssey-now-1", "Cosmic Odyssey", "A daring crew of astronauts embarks on a breathtaking mission beyond the Milky Way, where black holes, alien ecosystems, and impossible choices test the limits of human courage.", "Sci-Fi, Adventure", 8.5, 148, "Christopher Wright", ["Tom Holland", "Zoe Saldana", "Oscar Isaac"], "English with Vietnamese subtitles", "T13", "https://images.unsplash.com/photo-1767048264833-5b65aacd1039?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "quan-1", "binh-duong", "go-vap"]),
        new("the-old-building-secret-now-2", "The Old Building Secret", "A young team of investigators explores an abandoned downtown tower and uncovers a maze of terrifying secrets, hidden floors, and a presence that has been waiting in the dark.", "Horror, Thriller", 7.8, 112, "James Wan Jr.", ["Florence Pugh", "Jacob Elordi", "Lupita Nyong'o"], "English with Vietnamese subtitles", "T18", "https://images.unsplash.com/photo-1595171694538-beb81da39d3e?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "quan-1", "go-vap"]),
        new("impossible-target-now-3", "Impossible Target", "Elite agent Ethan Cross has 48 hours to stop a global conspiracy armed with catastrophic technology. With every lead compromised, one mistake could change the world forever.", "Action, Adventure", 8.9, 135, "David Leitch", ["Tom Cruise", "Rebecca Ferguson", "Henry Cavill"], "English with Vietnamese subtitles", "T16", "https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "quan-1", "binh-duong", "go-vap"]),
        new("sunset-serenade-now-4", "Sunset Serenade", "During a quiet getaway in Da Lat, two lonely hearts meet by the lake at sunset and discover a tender romance shaped by modern pressures, missed chances, and the courage to be honest.", "Romance, Drama", 7.5, 105, "Nguyen Minh Chau", ["Kaity Nguyen", "Isaac", "Nhung Kate"], "Vietnamese", "P", "https://images.unsplash.com/photo-1759643509991-0b0ec261e395?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "binh-duong"]),
        new("cuoc-chien-ngam-now-5", "Cuoc Chien Ngam", "Detective Marcus Black takes a murder case that pulls him into a covert war between global crime syndicates and the institution that once trained him.", "Action, Crime", 8.1, 128, "Matt Reeves", ["Ryan Gosling", "Ana de Armas", "John David Washington"], "English with Vietnamese subtitles", "T16", "https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "quan-1", "go-vap"]),
        new("ngu-hanh-son-now-6", "Ngu Hanh Son", "Five young guardians inherit the power of sacred stones and must learn to wield them before an ancient darkness returns.", "Fantasy, Action", 7.9, 143, "Ly Hai", ["Tran Thanh", "Ngo Thanh Van", "Kieu Minh Tuan"], "Vietnamese", "T13", "https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "binh-duong", "go-vap"]),
        new("cuoc-chien-ngam", "Shadow War", "Veteran detective Marcus Black takes on a murder case that spirals into a covert war between international crime syndicates and the institution that once trained him.", "Action, Crime", 8.1, 128, "Matt Reeves", ["Ryan Gosling", "Ana de Armas", "John David Washington"], "English with Vietnamese subtitles", "T16", "https://images.unsplash.com/photo-1765510296004-614b6cc204da?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "quan-1", "go-vap"]),
        new("ngu-hanh-son", "Five Sacred Peaks", "Five young guardians inherit the power of sacred stones and must learn to control their gifts before an ancient darkness rises again.", "Fantasy, Action", 7.9, 143, "Ly Hai", ["Tran Thanh", "Ngo Thanh Van", "Kieu Minh Tuan"], "Vietnamese", "T13", "https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", false, ["abcd-mall", "binh-duong", "go-vap"]),
        new("kingdom-of-legends-soon-1", "Kingdom of Legends", "A young ruler begins a mythical quest to lift an ancient curse and save his kingdom, traveling through enchanted lands filled with monsters, magic, and a legendary blade.", "Fantasy, Adventure", 8.2, 156, "Peter Jackson Jr.", ["Timothee Chalamet", "Anya Taylor-Joy", "Idris Elba"], "English with Vietnamese subtitles", "T13", "https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", true, []),
        new("midnight-nightmare-soon-2", "Midnight Nightmare", "When nightmares begin to bleed into reality, a group of students discovers that something from the shadows has marked them, and the line between dream and death starts to vanish.", "Horror, Mystery", 7.9, 118, "Mike Flanagan", ["Sydney Sweeney", "Barry Keoghan", "Cate Blanchett"], "English with Vietnamese subtitles", "T18", "https://images.unsplash.com/photo-1699631596984-cfb063c5d968?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", true, []),
        new("unexpected-happiness-soon-3", "Unexpected Happiness", "A chaotic family vacation in a coastal village turns into a heartfelt summer comedy, where old misunderstandings, warm reunions, and small joys reveal what happiness really means.", "Comedy, Family", 7.3, 98, "Tran Thanh", ["Kaity Nguyen", "Tran Thanh", "Tuan Tran"], "Vietnamese", "P", "https://images.unsplash.com/photo-1758525862263-af89b090fb56?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", true, []),
        new("kingdom-of-legends-soon-4", "Kingdom of Legends", "A young ruler begins a mythical quest to lift an ancient curse and save his kingdom, traveling through enchanted lands filled with monsters, magic, and a legendary blade.", "Fantasy, Adventure", 8.2, 156, "Peter Jackson Jr.", ["Timothee Chalamet", "Anya Taylor-Joy", "Idris Elba"], "English with Vietnamese subtitles", "T13", "https://images.unsplash.com/photo-1761948245703-cbf27a3e7502?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", true, []),
        new("midnight-nightmare-soon-5", "Midnight Nightmare", "When nightmares begin to bleed into reality, a group of students discovers that something from the shadows has marked them, and the line between dream and death starts to vanish.", "Horror, Mystery", 7.9, 118, "Mike Flanagan", ["Sydney Sweeney", "Barry Keoghan", "Cate Blanchett"], "English with Vietnamese subtitles", "T18", "https://images.unsplash.com/photo-1699631596984-cfb063c5d968?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", true, []),
        new("unexpected-happiness-soon-6", "Unexpected Happiness", "A chaotic family vacation in a coastal village turns into a heartfelt summer comedy, where old misunderstandings, warm reunions, and small joys reveal what happiness really means.", "Comedy, Family", 7.3, 98, "Tran Thanh", ["Kaity Nguyen", "Tran Thanh", "Tuan Tran"], "Vietnamese", "P", "https://images.unsplash.com/photo-1758525862263-af89b090fb56?crop=entropy&cs=tinysrgb&fit=max&fm=jpg&q=80&w=1080", true, [])
    ];

    private static readonly SnackComboSeedItem[] SnackComboSeeds =
    [
        new("combo-solo", "Solo Popcorn Combo", "1 large popcorn and 1 soft drink.", 89000),
        new("combo-double", "Double Movie Combo", "2 large popcorns and 2 soft drinks for sharing.", 159000),
        new("combo-gold", "Combo Gold", "1 caramel popcorn, 2 drinks and 1 snack tray.", 142000)
    ];

    private static readonly PromotionSeedItem[] PromotionSeeds =
    [
        new("WEEKEND", "Weekend Tickets - Free Popcorn Combo", "Available on Saturday and Sunday for orders with at least 2 tickets.", "weekend", null, null, null, 150000, null, false, [new(PromotionRuleType.BusinessDate, "Weekend", 1), new(PromotionRuleType.SeatCount, "2", 2)]),
        new("MOMO30", "Pay with MoMo - Get 30% Off", "Save 30% on ticket value, up to 60,000 VND, when paying with MoMo.", "bank", 30, null, 60000, 120000, null, false, [new(PromotionRuleType.PaymentProvider, "Momo", 1)]),
        new("DATENIGHT", "Date Night - Buy 1 Get 1", "Choose a couple seat and receive 1 regular ticket for the same showtime.", "ticket", null, 85000, null, 0, null, false, [new(PromotionRuleType.SeatType, "Couple", 1)]),
        new("VCB25", "Vietcombank - 25% Off", "Enjoy 25% off on weekday bookings, up to 50,000 VND.", "bank", 25, null, 50000, 100000, null, false, [new(PromotionRuleType.PaymentProvider, "VnPay", 1)]),
        new("GROUP20", "Group Booking - 20% Off 5 Tickets", "Get 20% off when booking 5 tickets or more in one order.", "ticket", 20, null, null, 0, null, false, [new(PromotionRuleType.SeatCount, "5", 1)]),
        new("COMBOGOLD", "Combo Gold - Special Snack Price", "Add Combo Gold to unlock the special 85,000 VND promotional price.", "combo", null, 57000, null, 0, null, false, [new(PromotionRuleType.Combo, "combo-gold", 1)]),
        new("BIRTHDAY", "Birthday Special - 1 Free Ticket", "Enter your birthday at checkout to claim a free regular ticket during your birthday month.", "member", null, 85000, null, 0, 1, false, [new(PromotionRuleType.BirthdayMonth, "CurrentMonth", 1)]),
        new("EARLYBIRD", "Early Bird - 35% Off", "Valid for showtimes starting from 09:00 to before 11:00.", "ticket", 35, null, null, 0, null, false, [new(PromotionRuleType.Showtime, "Morning", 1)])
    ];
}
