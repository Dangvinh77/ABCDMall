using ABCDMall.Modules.Users.Application.DTOs.Auth;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public interface IUserQueryService
{
    Task<UserProfileResponseDto?> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateHistoryResponseDto>> GetProfileUpdateHistoryAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateRequestResponseDto>> GetProfileUpdateRequestsAsync(string? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateRequestResponseDto>> GetMyProfileUpdateRequestsAsync(string userId, string? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSummaryResponseDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSummaryResponseDto>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
}
