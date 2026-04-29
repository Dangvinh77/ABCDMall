namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class EventListQueryDto
{
    public string? Keyword { get; set; }
    public string? TimeFilter { get; set; }
    public string? ShopId { get; set; }
    public int? ApprovalStatus { get; set; }
    public bool IncludeAllStatuses { get; set; }
}