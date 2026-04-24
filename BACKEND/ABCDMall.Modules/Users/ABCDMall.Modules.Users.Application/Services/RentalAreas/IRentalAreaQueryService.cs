using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;

namespace ABCDMall.Modules.Users.Application.Services.RentalAreas;

public interface IRentalAreaQueryService
{
    Task<IReadOnlyList<RentalAreaResponseDto>> GetRentalAreasAsync(CancellationToken cancellationToken = default);
    Task<RentalAreaDetailResponseDto?> GetRentalAreaDetailAsync(string rentalAreaId, CancellationToken cancellationToken = default);
    Task<ManagerLookupResponseDto?> CheckManagerByCccdAsync(string cccd, CancellationToken cancellationToken = default);
}
