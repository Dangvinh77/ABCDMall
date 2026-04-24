namespace ABCDMall.Modules.Users.Domain.Entities;

public class MovieCarouselAd
{
    public string? Id { get; set; } = Guid.NewGuid().ToString("N");

    public string ImageUrl { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime TargetMondayDate { get; set; }

    public bool IsActive { get; set; }
}
