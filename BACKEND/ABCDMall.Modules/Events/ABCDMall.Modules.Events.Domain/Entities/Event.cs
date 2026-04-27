using ABCDMall.Modules.Events.Domain.Enums;

namespace ABCDMall.Modules.Events.Domain.Entities;

public sealed class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public EventLocationType LocationType { get; set; }
    public string? ShopId { get; set; }
    public EventApprovalStatus ApprovalStatus { get; set; } = EventApprovalStatus.Pending;
    public string? RejectionReason { get; set; }
    public bool HasGiftRegistration { get; set; }
    public string? GiftDescription { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public ICollection<EventRegistration> Registrations { get; set; } = [];
}
