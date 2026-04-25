namespace ABCDMall.Modules.Events.Application.DTOs.Events;

public sealed class RegisterEventRequestDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
}
