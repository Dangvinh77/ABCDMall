using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;

public class MoviesCatalogDbContext : DbContext
{
    public MoviesCatalogDbContext(DbContextOptions<MoviesCatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<MovieCredit> MovieCredits => Set<MovieCredit>();
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<HallSeat> HallSeats => Set<HallSeat>();
    public DbSet<Showtime> Showtimes => Set<Showtime>();
    public DbSet<ShowtimeSeatInventory> ShowtimeSeatInventories => Set<ShowtimeSeatInventory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MovieConfiguration());
        modelBuilder.ApplyConfiguration(new GenreConfiguration());
        modelBuilder.ApplyConfiguration(new MovieGenreConfiguration());
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new MovieCreditConfiguration());
        modelBuilder.ApplyConfiguration(new CinemaConfiguration());
        modelBuilder.ApplyConfiguration(new HallConfiguration());
        modelBuilder.ApplyConfiguration(new HallSeatConfiguration());
        modelBuilder.ApplyConfiguration(new ShowtimeConfiguration());
        modelBuilder.ApplyConfiguration(new ShowtimeSeatInventoryConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
