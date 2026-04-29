using ABCDMall.Modules.Users.Application.DTOs.PublicCatalog;

namespace ABCDMall.Modules.Users.Application.Services.PublicCatalog;

public interface IManagerBusinessRouteService
{
    Task<ManagerBusinessRouteDto> GetRouteAsync(string ownerShopId, CancellationToken cancellationToken = default);
}
