namespace ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

public sealed class ManagerBusinessRouteDto
{
    public string BusinessType { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public bool HasEligibleRental { get; set; }
}
