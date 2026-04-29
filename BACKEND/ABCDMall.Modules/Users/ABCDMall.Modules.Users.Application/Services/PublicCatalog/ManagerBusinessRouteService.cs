using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public sealed class ManagerBusinessRouteService : IManagerBusinessRouteService
{
    private const string RentalAreasPath = "/rental-areas";
    private const string ShopManagerPath = "/manager-shops";
    private const string FoodCourtManagerPath = "/food-court-manager";

    private readonly IManagerBusinessRouteRepository _repository;

    public ManagerBusinessRouteService(IManagerBusinessRouteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ManagerBusinessRouteDto> GetRouteAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ownerShopId))
        {
            return CreateNoRentalRoute();
        }

        var snapshot = await _repository.GetSnapshotAsync(ownerShopId.Trim(), cancellationToken);
        if (snapshot is null || !snapshot.HasEligibleRental || string.IsNullOrWhiteSpace(snapshot.BusinessType))
        {
            return CreateNoRentalRoute();
        }

        var normalizedBusinessType = snapshot.BusinessType.Trim();
        return new ManagerBusinessRouteDto
        {
            BusinessType = normalizedBusinessType,
            TargetPath = normalizedBusinessType == "FoodCourt" ? FoodCourtManagerPath : ShopManagerPath,
            HasEligibleRental = true
        };
    }

    private static ManagerBusinessRouteDto CreateNoRentalRoute()
    {
        return new ManagerBusinessRouteDto
        {
            BusinessType = string.Empty,
            TargetPath = RentalAreasPath,
            HasEligibleRental = false
        };
    }
}
