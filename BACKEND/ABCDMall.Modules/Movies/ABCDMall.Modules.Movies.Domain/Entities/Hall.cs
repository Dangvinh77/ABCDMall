using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Domain.Entities;

public class Hall
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string HallCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public HallType HallType { get; set; } = HallType.Standard2D;
    public int SeatCapacity { get; set; }
    public bool IsActive { get; set; } = true;

    public Cinema? Cinema { get; set; }
    public ICollection<HallSeat> HallSeats { get; set; } = new List<HallSeat>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
