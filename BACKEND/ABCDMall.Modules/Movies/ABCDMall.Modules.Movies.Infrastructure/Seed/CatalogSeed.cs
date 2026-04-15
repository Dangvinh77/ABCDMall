using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Seed;

public static class CatalogSeed
{
    public static async Task SeedAsync(MoviesCatalogDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Movies.AnyAsync(cancellationToken))
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(nowUtc);

        var genres = BuildGenres();
        var movies = BuildMovies(nowUtc, today);
        var movieGenres = BuildMovieGenres(genres, movies);
        var people = BuildPeople();
        var credits = BuildCredits(movies, people);
        var cinemas = BuildCinemas(nowUtc);
        var halls = BuildHalls(cinemas);
        var hallSeats = BuildHallSeats(halls);
        var showtimes = BuildShowtimes(movies, cinemas, halls, nowUtc, today);
        var seatInventories = BuildSeatInventories(showtimes, hallSeats, nowUtc);

        await dbContext.Genres.AddRangeAsync(genres, cancellationToken);
        await dbContext.Movies.AddRangeAsync(movies, cancellationToken);
        await dbContext.MovieGenres.AddRangeAsync(movieGenres, cancellationToken);
        await dbContext.People.AddRangeAsync(people, cancellationToken);
        await dbContext.MovieCredits.AddRangeAsync(credits, cancellationToken);
        await dbContext.Cinemas.AddRangeAsync(cinemas, cancellationToken);
        await dbContext.Halls.AddRangeAsync(halls, cancellationToken);
        await dbContext.HallSeats.AddRangeAsync(hallSeats, cancellationToken);
        await dbContext.Showtimes.AddRangeAsync(showtimes, cancellationToken);
        await dbContext.ShowtimeSeatInventories.AddRangeAsync(seatInventories, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static List<Genre> BuildGenres()
    {
        return
        [
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Action", Description = "High energy blockbuster stories" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Comedy", Description = "Lighthearted and funny stories" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Drama", Description = "Character driven emotional stories" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Sci-Fi", Description = "Science fiction adventures" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "Horror", Description = "Suspense and horror films" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "Animation", Description = "Animated stories for all ages" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "Romance", Description = "Romantic stories" },
            new Genre { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), Name = "Adventure", Description = "Adventure and journey stories" }
        ];
    }

    private static List<Movie> BuildMovies(DateTime nowUtc, DateOnly today)
    {
        return
        [
            CreateMovie("20000000-0000-0000-0000-000000000001", "Midnight Velocity", "midnight-velocity", "Action thriller across three cities.", 129, "T16", today.AddDays(-20), MovieStatus.NowShowing, LanguageType.Subtitle, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000002", "Laugh Station", "laugh-station", "Office comedy with late-night chaos.", 101, "K", today.AddDays(-12), MovieStatus.NowShowing, LanguageType.Dubbed, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000003", "Kingdom of Dust", "kingdom-of-dust", "Epic fantasy war and betrayal.", 142, "T13", today.AddDays(-8), MovieStatus.NowShowing, LanguageType.Subtitle, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000004", "Orbit Seven", "orbit-seven", "Sci-fi rescue mission near Saturn.", 118, "T13", today.AddDays(-4), MovieStatus.NowShowing, LanguageType.Subtitle, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000005", "Blooming Hearts", "blooming-hearts", "Romantic drama set in Da Lat.", 110, "T13", today.AddDays(-2), MovieStatus.NowShowing, LanguageType.Dubbed, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000006", "Shadow Classroom", "shadow-classroom", "Teen horror inside an abandoned school.", 97, "T18", today.AddDays(3), MovieStatus.ComingSoon, LanguageType.Subtitle, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000007", "Skyline Racers", "skyline-racers", "Street racing team returns for one last run.", 124, "T16", today.AddDays(5), MovieStatus.ComingSoon, LanguageType.Subtitle, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000008", "Tiny Planet", "tiny-planet", "Animated family road trip through space.", 95, "P", today.AddDays(7), MovieStatus.ComingSoon, LanguageType.Dubbed, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000009", "The Glass Violin", "the-glass-violin", "Music drama about rivalry and healing.", 113, "T13", today.AddDays(10), MovieStatus.ComingSoon, LanguageType.Subtitle, nowUtc),
            CreateMovie("20000000-0000-0000-0000-000000000010", "Ocean Gate", "ocean-gate", "Adventure mystery beneath a flooded city.", 121, "T13", today.AddDays(14), MovieStatus.ComingSoon, LanguageType.Subtitle, nowUtc)
        ];
    }

    private static Movie CreateMovie(
        string id,
        string title,
        string slug,
        string synopsis,
        int durationMinutes,
        string ratingLabel,
        DateOnly releaseDate,
        MovieStatus status,
        LanguageType defaultLanguage,
        DateTime nowUtc)
    {
        return new Movie
        {
            Id = Guid.Parse(id),
            Title = title,
            Slug = slug,
            Synopsis = synopsis,
            DurationMinutes = durationMinutes,
            PosterUrl = $"https://example.com/posters/{slug}.jpg",
            TrailerUrl = $"https://example.com/trailers/{slug}.mp4",
            ReleaseDate = releaseDate,
            RatingLabel = ratingLabel,
            DefaultLanguage = defaultLanguage,
            Status = status,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    private static List<MovieGenre> BuildMovieGenres(IReadOnlyList<Genre> genres, IReadOnlyList<Movie> movies)
    {
        var genreLookup = genres.ToDictionary(genre => genre.Name, genre => genre.Id);

        return
        [
            LinkGenre(movies[0], genreLookup, "Action"), LinkGenre(movies[0], genreLookup, "Adventure"),
            LinkGenre(movies[1], genreLookup, "Comedy"),
            LinkGenre(movies[2], genreLookup, "Drama"), LinkGenre(movies[2], genreLookup, "Adventure"),
            LinkGenre(movies[3], genreLookup, "Sci-Fi"), LinkGenre(movies[3], genreLookup, "Action"),
            LinkGenre(movies[4], genreLookup, "Romance"), LinkGenre(movies[4], genreLookup, "Drama"),
            LinkGenre(movies[5], genreLookup, "Horror"),
            LinkGenre(movies[6], genreLookup, "Action"), LinkGenre(movies[6], genreLookup, "Drama"),
            LinkGenre(movies[7], genreLookup, "Animation"), LinkGenre(movies[7], genreLookup, "Adventure"),
            LinkGenre(movies[8], genreLookup, "Drama"), LinkGenre(movies[8], genreLookup, "Romance"),
            LinkGenre(movies[9], genreLookup, "Adventure"), LinkGenre(movies[9], genreLookup, "Sci-Fi")
        ];
    }

    private static MovieGenre LinkGenre(Movie movie, IReadOnlyDictionary<string, Guid> genreLookup, string genreName)
    {
        return new MovieGenre
        {
            MovieId = movie.Id,
            GenreId = genreLookup[genreName]
        };
    }

    private static List<Person> BuildPeople()
    {
        return
        [
            CreatePerson("30000000-0000-0000-0000-000000000001", "An Nguyen"),
            CreatePerson("30000000-0000-0000-0000-000000000002", "Bao Tran"),
            CreatePerson("30000000-0000-0000-0000-000000000003", "Chi Le"),
            CreatePerson("30000000-0000-0000-0000-000000000004", "Duy Pham"),
            CreatePerson("30000000-0000-0000-0000-000000000005", "Emma Vo"),
            CreatePerson("30000000-0000-0000-0000-000000000006", "Gia Linh"),
            CreatePerson("30000000-0000-0000-0000-000000000007", "Huy Do"),
            CreatePerson("30000000-0000-0000-0000-000000000008", "Iris Truong")
        ];
    }

    private static Person CreatePerson(string id, string fullName)
    {
        return new Person
        {
            Id = Guid.Parse(id),
            FullName = fullName,
            ProfileImageUrl = $"https://example.com/people/{fullName.ToLowerInvariant().Replace(" ", "-")}.jpg",
            Biography = $"{fullName} is part of the demo movie catalog seed."
        };
    }

    private static List<MovieCredit> BuildCredits(IReadOnlyList<Movie> movies, IReadOnlyList<Person> people)
    {
        return
        [
            CreateCredit(movies[0], people[0], "Director", "Director", 1),
            CreateCredit(movies[0], people[1], "Cast", "Lead", 2),
            CreateCredit(movies[0], people[2], "Cast", "Partner", 3),
            CreateCredit(movies[1], people[3], "Director", "Director", 1),
            CreateCredit(movies[1], people[4], "Cast", "Lead", 2),
            CreateCredit(movies[2], people[5], "Director", "Director", 1),
            CreateCredit(movies[2], people[6], "Cast", "King", 2),
            CreateCredit(movies[3], people[0], "Director", "Director", 1),
            CreateCredit(movies[3], people[7], "Cast", "Commander", 2),
            CreateCredit(movies[4], people[2], "Director", "Director", 1),
            CreateCredit(movies[4], people[4], "Cast", "Lead", 2),
            CreateCredit(movies[5], people[6], "Director", "Director", 1),
            CreateCredit(movies[6], people[1], "Director", "Director", 1),
            CreateCredit(movies[7], people[3], "Director", "Director", 1),
            CreateCredit(movies[8], people[5], "Director", "Director", 1),
            CreateCredit(movies[9], people[7], "Director", "Director", 1)
        ];
    }

    private static MovieCredit CreateCredit(Movie movie, Person person, string creditType, string roleName, int displayOrder)
    {
        return new MovieCredit
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            PersonId = person.Id,
            CreditType = creditType,
            RoleName = roleName,
            DisplayOrder = displayOrder
        };
    }

    private static List<Cinema> BuildCinemas(DateTime nowUtc)
    {
        return
        [
            CreateCinema("40000000-0000-0000-0000-000000000001", "HCM-01", "ABCD Mall Sai Gon", "01 Nguyen Hue", "District 1", "Ho Chi Minh", nowUtc),
            CreateCinema("40000000-0000-0000-0000-000000000002", "HCM-02", "ABCD Mall Thu Duc", "88 Vo Van Ngan", "Linh Chieu", "Ho Chi Minh", nowUtc),
            CreateCinema("40000000-0000-0000-0000-000000000003", "DN-01", "ABCD Mall Da Nang", "10 Bach Dang", "Hai Chau", "Da Nang", nowUtc)
        ];
    }

    private static Cinema CreateCinema(
        string id,
        string code,
        string name,
        string addressLine1,
        string addressLine2,
        string city,
        DateTime nowUtc)
    {
        return new Cinema
        {
            Id = Guid.Parse(id),
            Code = code,
            Name = name,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            IsActive = true,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    private static List<Hall> BuildHalls(IReadOnlyList<Cinema> cinemas)
    {
        return
        [
            CreateHall("50000000-0000-0000-0000-000000000001", cinemas[0].Id, "A1", "Hall A1", HallType.Standard2D, 54),
            CreateHall("50000000-0000-0000-0000-000000000002", cinemas[0].Id, "IM1", "IMAX Hall", HallType.Imax, 40),
            CreateHall("50000000-0000-0000-0000-000000000003", cinemas[1].Id, "B1", "Hall B1", HallType.Standard2D, 54),
            CreateHall("50000000-0000-0000-0000-000000000004", cinemas[1].Id, "B3D", "Hall 3D", HallType.Standard3D, 40),
            CreateHall("50000000-0000-0000-0000-000000000005", cinemas[2].Id, "C1", "Hall C1", HallType.Standard2D, 54),
            CreateHall("50000000-0000-0000-0000-000000000006", cinemas[2].Id, "4DX", "4DX Hall", HallType.FourDx, 40)
        ];
    }

    private static Hall CreateHall(string id, Guid cinemaId, string hallCode, string name, HallType hallType, int seatCapacity)
    {
        return new Hall
        {
            Id = Guid.Parse(id),
            CinemaId = cinemaId,
            HallCode = hallCode,
            Name = name,
            HallType = hallType,
            SeatCapacity = seatCapacity,
            IsActive = true
        };
    }

    private static List<HallSeat> BuildHallSeats(IReadOnlyList<Hall> halls)
    {
        var seats = new List<HallSeat>();

        foreach (var hall in halls)
        {
            seats.AddRange(BuildSeatsForHall(hall));
        }

        return seats;
    }

    private static IEnumerable<HallSeat> BuildSeatsForHall(Hall hall)
    {
        var hallCodeKey = hall.HallCode.Replace("-", string.Empty);
        var seats = new List<HallSeat>();
        var rows = hall.HallType is HallType.Imax or HallType.FourDx
            ? new[] { "A", "B", "C", "D", "E" }
            : new[] { "A", "B", "C", "D", "E", "F" };
        var columns = hall.HallType is HallType.Imax or HallType.FourDx ? 8 : 9;

        foreach (var row in rows)
        {
            for (var col = 1; col <= columns; col++)
            {
                var isVipRow = row is "D" or "E";
                var isCoupleRow = row == rows[^1] && col <= columns - 1 && col % 2 == 1;
                var seatCode = $"{row}{col}";
                var seatType = isCoupleRow ? SeatType.Couple : isVipRow ? SeatType.Vip : SeatType.Regular;
                var coupleGroupCode = isCoupleRow ? $"{hallCodeKey}-{row}-{col / 2 + 1}" : null;

                seats.Add(new HallSeat
                {
                    Id = Guid.NewGuid(),
                    HallId = hall.Id,
                    SeatCode = seatCode,
                    RowLabel = row,
                    ColumnNumber = col,
                    SeatType = seatType,
                    CoupleGroupCode = coupleGroupCode,
                    IsActive = true
                });

                if (isCoupleRow)
                {
                    col++;
                    if (col > columns)
                    {
                        break;
                    }

                    seats.Add(new HallSeat
                    {
                        Id = Guid.NewGuid(),
                        HallId = hall.Id,
                        SeatCode = $"{row}{col}",
                        RowLabel = row,
                        ColumnNumber = col,
                        SeatType = SeatType.Couple,
                        CoupleGroupCode = coupleGroupCode,
                        IsActive = true
                    });
                }
            }
        }

        return seats;
    }

    private static List<Showtime> BuildShowtimes(
        IReadOnlyList<Movie> movies,
        IReadOnlyList<Cinema> cinemas,
        IReadOnlyList<Hall> halls,
        DateTime nowUtc,
        DateOnly today)
    {
        var hallByCinema = halls.GroupBy(hall => hall.CinemaId).ToDictionary(group => group.Key, group => group.ToList());
        var showtimes = new List<Showtime>();

        for (var dayOffset = 0; dayOffset < 7; dayOffset++)
        {
            var businessDate = today.AddDays(dayOffset);
            var dayStartUtc = businessDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            foreach (var cinema in cinemas)
            {
                var cinemaHalls = hallByCinema[cinema.Id];
                var movieA = movies[(dayOffset + Array.IndexOf(cinemas.ToArray(), cinema)) % 5];
                var movieB = dayOffset < 2 ? movies[5 + (dayOffset % 2)] : movies[6 + (dayOffset % 4)];

                showtimes.Add(CreateShowtime(movieA, cinema, cinemaHalls[0], businessDate, dayStartUtc.AddHours(9), 75000 + dayOffset * 5000, LanguageType.Subtitle, nowUtc));
                showtimes.Add(CreateShowtime(movieA, cinema, cinemaHalls[0], businessDate, dayStartUtc.AddHours(15), 85000 + dayOffset * 5000, LanguageType.Dubbed, nowUtc));
                showtimes.Add(CreateShowtime(movieB, cinema, cinemaHalls[1], businessDate, dayStartUtc.AddHours(19), 110000 + dayOffset * 5000, dayOffset % 2 == 0 ? LanguageType.Subtitle : LanguageType.Dubbed, nowUtc));
            }
        }

        return showtimes;
    }

    private static Showtime CreateShowtime(
        Movie movie,
        Cinema cinema,
        Hall hall,
        DateOnly businessDate,
        DateTime startAtUtc,
        decimal basePrice,
        LanguageType language,
        DateTime nowUtc)
    {
        return new Showtime
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            CinemaId = cinema.Id,
            HallId = hall.Id,
            BusinessDate = businessDate,
            StartAtUtc = DateTime.SpecifyKind(startAtUtc, DateTimeKind.Utc),
            EndAtUtc = DateTime.SpecifyKind(startAtUtc.AddMinutes(movie.DurationMinutes + 15), DateTimeKind.Utc),
            Language = language,
            BasePrice = basePrice,
            Status = ShowtimeStatus.Open,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = nowUtc
        };
    }

    private static List<ShowtimeSeatInventory> BuildSeatInventories(
        IReadOnlyList<Showtime> showtimes,
        IReadOnlyList<HallSeat> hallSeats,
        DateTime nowUtc)
    {
        var seatsByHall = hallSeats.GroupBy(seat => seat.HallId).ToDictionary(group => group.Key, group => group.ToList());
        var inventories = new List<ShowtimeSeatInventory>();

        foreach (var showtime in showtimes)
        {
            foreach (var seat in seatsByHall[showtime.HallId])
            {
                inventories.Add(new ShowtimeSeatInventory
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
                    Status = ResolveSeatStatus(showtime, seat),
                    UpdatedAtUtc = nowUtc
                });
            }
        }

        return inventories;
    }

    private static decimal CalculateSeatPrice(decimal basePrice, SeatType seatType)
    {
        return seatType switch
        {
            SeatType.Vip => basePrice + 25000,
            SeatType.Couple => basePrice * 2 + 30000,
            _ => basePrice
        };
    }

    private static SeatInventoryStatus ResolveSeatStatus(Showtime showtime, HallSeat seat)
    {
        if (seat.SeatType == SeatType.Couple && seat.ColumnNumber == 1)
        {
            return SeatInventoryStatus.Reserved;
        }

        if (showtime.BusinessDate == DateOnly.FromDateTime(DateTime.UtcNow) && seat.RowLabel == "A" && seat.ColumnNumber <= 2)
        {
            return SeatInventoryStatus.Booked;
        }

        return SeatInventoryStatus.Available;
    }
}
