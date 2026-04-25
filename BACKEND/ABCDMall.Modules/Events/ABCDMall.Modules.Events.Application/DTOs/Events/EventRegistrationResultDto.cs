namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class EventRegistrationResultDto
{
    public Guid RegistrationId { get; set; }
    public string RedeemCode { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
