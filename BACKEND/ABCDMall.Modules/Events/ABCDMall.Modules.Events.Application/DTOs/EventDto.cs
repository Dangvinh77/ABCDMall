namespace ABCDMall.Modules.Events.Application.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string LocationType { get; set; } = string.Empty;
    public string? ShopId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public bool HasGiftRegistration { get; set; }
    public string? GiftDescription { get; set; }
    public bool IsOngoing { get; set; }
    public bool IsUpcoming { get; set; }
    public DateTime CreatedAt { get; set; }
}
