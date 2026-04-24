namespace ABCDMall.Modules.Users.Domain.Entities;

public class ProfileUpdateRequest
{
    public string? Id { get; set; } = Guid.NewGuid().ToString("N");

    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? CurrentFullName { get; set; }

    public string? CurrentAddress { get; set; }

    public string? CurrentCCCD { get; set; }

    public string? CurrentCccdFrontImage { get; set; }

    public string? CurrentCccdBackImage { get; set; }

    public string? RequestedFullName { get; set; }

    public string? RequestedAddress { get; set; }

    public string? RequestedCCCD { get; set; }

    public string? RequestedCccdFrontImage { get; set; }

    public string? RequestedCccdBackImage { get; set; }

    public string Status { get; set; } = "Pending";

    public string? ReviewedByAdminId { get; set; }

    public string? ReviewNote { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }
}
