using ABCDMall.Modules.Users.Application.DTOs.Auth;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public sealed class UserQueryService : IUserQueryService
{
    private readonly AutoMapper.IMapper _mapper;
    private readonly IUserReadRepository _userReadRepository;

    public UserQueryService(AutoMapper.IMapper mapper, IUserReadRepository userReadRepository)
    {
        _mapper = mapper;
        _userReadRepository = userReadRepository;
    }

    public async Task<UserProfileResponseDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);
        return user is null || !user.IsActive ? null : _mapper.Map<UserProfileResponseDto>(user);
    }

    public async Task<IReadOnlyList<ProfileUpdateHistoryResponseDto>> GetProfileUpdateHistoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var history = await _userReadRepository.GetProfileUpdateHistoryAsync(userId, 20, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProfileUpdateHistoryResponseDto>>(history);
    }

    public async Task<IReadOnlyList<ProfileUpdateRequestResponseDto>> GetProfileUpdateRequestsAsync(string? status, CancellationToken cancellationToken = default)
    {
        var requests = await _userReadRepository.GetProfileUpdateRequestsAsync(status, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProfileUpdateRequestResponseDto>>(requests);
    }

    public async Task<IReadOnlyList<ProfileUpdateRequestResponseDto>> GetMyProfileUpdateRequestsAsync(string userId, string? status, CancellationToken cancellationToken = default)
    {
        var requests = await _userReadRepository.GetProfileUpdateRequestsByUserAsync(userId, status, 10, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProfileUpdateRequestResponseDto>>(requests);
    }

    public async Task<IReadOnlyList<UserSummaryResponseDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userReadRepository.GetUsersAsync(cancellationToken);
        return await MapUsersAsync(users, cancellationToken);
    }

    public async Task<IReadOnlyList<UserSummaryResponseDto>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        var users = await _userReadRepository.GetUsersByRoleAsync(role, cancellationToken);
        return await MapUsersAsync(users, cancellationToken);
    }

    private async Task<IReadOnlyList<UserSummaryResponseDto>> MapUsersAsync(
        IReadOnlyList<Domain.Entities.User> users,
        CancellationToken cancellationToken)
    {
        var shopNamesById = await _userReadRepository.GetShopNamesByIdsAsync(cancellationToken);

        var responses = _mapper.Map<List<UserSummaryResponseDto>>(users);
        foreach (var response in responses)
        {
            if (!string.IsNullOrWhiteSpace(response.ShopId) && shopNamesById.TryGetValue(response.ShopId, out var shopName))
            {
                response.ShopName = shopName;
            }
        }

        return responses;
    }
}
