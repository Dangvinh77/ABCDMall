namespace ABCDMall.Modules.Events.Domain.Enums;

/// <summary>
/// Computed từ StartDate/EndDate — không lưu vào DB.
/// </summary>
public enum EventStatus
{
    Upcoming = 1,
    Ongoing  = 2,
    Ended    = 3
}