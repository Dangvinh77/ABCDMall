using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;

namespace ABCDMall.Modules.Users.Application.Services.RentalAreas;

public sealed class RentalAreaQueryService : IRentalAreaQueryService
{
    private readonly AutoMapper.IMapper _mapper;
    private readonly IRentalAreaReadRepository _rentalAreaReadRepository;

    public RentalAreaQueryService(AutoMapper.IMapper mapper, IRentalAreaReadRepository rentalAreaReadRepository)
    {
        _mapper = mapper;
        _rentalAreaReadRepository = rentalAreaReadRepository;
    }

    public async Task<IReadOnlyList<RentalAreaResponseDto>> GetRentalAreasAsync(CancellationToken cancellationToken = default)
    {
        var rentalAreas = await _rentalAreaReadRepository.GetRentalAreasAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<RentalAreaResponseDto>>(rentalAreas);
    }

    public Task<RentalAreaDetailResponseDto?> GetRentalAreaDetailAsync(string rentalAreaId, CancellationToken cancellationToken = default)
        => _rentalAreaReadRepository.GetRentalAreaDetailByIdAsync(rentalAreaId, cancellationToken);

    public async Task<ManagerLookupResponseDto?> CheckManagerByCccdAsync(string cccd, CancellationToken cancellationToken = default)
    {
        var normalizedCccd = cccd.Trim();
        var manager = await _rentalAreaReadRepository.GetManagerByCccdAsync(normalizedCccd, cancellationToken);
        if (manager is null)
        {
            return null;
        }

        var shopInfo = await _rentalAreaReadRepository.GetShopInfoByManagerAsync(manager, normalizedCccd, cancellationToken);

        return new ManagerLookupResponseDto
        {
            ManagerName = manager.FullName,
            ShopName = shopInfo?.ShopName ?? BuildDefaultShopName(manager.FullName),
            ShopId = shopInfo?.Id,
            CCCD = normalizedCccd
        };
    }

    internal static string BuildDefaultShopName(string? managerName)
        => string.IsNullOrWhiteSpace(managerName)
            ? "Pending Shop Registration"
            : $"{managerName.Trim()}'s Shop";
}
