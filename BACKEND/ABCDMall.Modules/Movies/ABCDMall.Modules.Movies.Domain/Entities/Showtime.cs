using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class Showtime
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid CinemaId { get; set; }
    public Guid HallId { get; set; }
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public DateTime? EndAtUtc { get; set; }
    public LanguageType Language { get; set; } = LanguageType.Subtitle;
    public decimal BasePrice { get; set; }
    public ShowtimeStatus Status { get; set; } = ShowtimeStatus.Draft;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public Movie? Movie { get; set; }
    public Cinema? Cinema { get; set; }
    public Hall? Hall { get; set; }
    public ICollection<ShowtimeSeatInventory> SeatInventories { get; set; } = new List<ShowtimeSeatInventory>();
}
