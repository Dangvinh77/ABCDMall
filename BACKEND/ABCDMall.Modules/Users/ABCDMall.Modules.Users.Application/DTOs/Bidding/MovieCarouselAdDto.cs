namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class MovieCarouselAdDto
{
    public string Id { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime TargetMondayDate { get; set; }

    public bool IsActive { get; set; }
}
