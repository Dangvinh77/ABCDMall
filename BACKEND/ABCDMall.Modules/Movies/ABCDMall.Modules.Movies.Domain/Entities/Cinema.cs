namespace ABCDMall.Modules.Movies.Domain.Entities;

public class Cinema
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
