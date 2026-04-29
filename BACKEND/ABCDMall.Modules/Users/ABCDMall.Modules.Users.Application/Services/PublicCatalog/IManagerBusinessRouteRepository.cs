namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public interface IManagerBusinessRouteRepository
{
    Task<ManagerBusinessRouteSnapshot?> GetSnapshotAsync(string ownerShopId, CancellationToken cancellationToken = default);
}

public sealed class ManagerBusinessRouteSnapshot
{
    public string OwnerShopId { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty;
    public bool HasEligibleRental { get; set; }
}
