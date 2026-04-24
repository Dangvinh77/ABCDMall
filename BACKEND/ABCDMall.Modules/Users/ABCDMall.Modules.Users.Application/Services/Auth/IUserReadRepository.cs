using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public interface IUserReadRepository
{
    Task<User?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateHistory>> GetProfileUpdateHistoryAsync(string userId, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateRequest>> GetProfileUpdateRequestsAsync(string? status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProfileUpdateRequest>> GetProfileUpdateRequestsByUserAsync(string userId, string? status, int take, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> GetShopNamesByIdsAsync(CancellationToken cancellationToken = default);
}
