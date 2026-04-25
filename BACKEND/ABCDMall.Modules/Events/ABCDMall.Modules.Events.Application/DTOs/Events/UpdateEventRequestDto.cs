namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class UpdateEventRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int LocationType { get; set; }
    public string? ShopId { get; set; }
    public int ApprovalStatus { get; set; } = 1;
    public bool HasGiftRegistration { get; set; }
    public string? GiftDescription { get; set; }
}
