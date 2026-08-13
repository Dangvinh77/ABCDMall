using ABCDMall.Modules.Users.Application.DTOs.ShopInfos;

namespace ABCDMall.Modules.Users.Application.Services.ShopInfos;

public sealed class ShopInfoQueryService : IShopInfoQueryService
{
    private readonly AutoMapper.IMapper _mapper;
    private readonly IShopMonthlyBillReadRepository _shopMonthlyBillReadRepository;

    public ShopInfoQueryService(
        AutoMapper.IMapper mapper,
        IShopMonthlyBillReadRepository shopMonthlyBillReadRepository)
    {
        _mapper = mapper;
        _shopMonthlyBillReadRepository = shopMonthlyBillReadRepository;
    }

    public async Task<IReadOnlyList<ShopMonthlyBillResponseDto>> GetBillsAsync(string? shopId, CancellationToken cancellationToken = default)
    {
        var bills = await _shopMonthlyBillReadRepository.GetBillsAsync(shopId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ShopMonthlyBillResponseDto>>(bills);
    }

    public async Task<ShopRentalInfoResponseDto?> GetRentalInfoAsync(string? shopId, CancellationToken cancellationToken = default)
    {
        var rentalInfo = await _shopMonthlyBillReadRepository.GetRentalInfoAsync(shopId, cancellationToken);
        return rentalInfo is null ? null : _mapper.Map<ShopRentalInfoResponseDto>(rentalInfo);
    }
}
