using ABCDMall.Modules.Users.Application.DTOs.Auth;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public interface IUserQueryService
{
    Task<UserProfileResponseDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateHistoryResponseDto>> GetProfileUpdateHistoryAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSummaryResponseDto>> GetUsersAsync(CancellationToken cancellationToken = default);
}
