using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public interface IUserCommandRepository
{
    Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<User?> GetUserByPasswordSetupTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<bool> ExistsUserByEmailAsync(string normalizedEmail, string? excludedUserId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsUserByCccdAsync(string normalizedCccd, string? excludedUserId = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsShopInfoByCccdAsync(string normalizedCccd, string? excludedShopId = null, CancellationToken cancellationToken = default);

    Task<ShopInfo?> GetShopInfoByIdAsync(string shopId, CancellationToken cancellationToken = default);

    Task<bool> HasActiveRentalAreaAsync(string? shopId, CancellationToken cancellationToken = default);

    Task RemoveUnusedForgotPasswordOtpsAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task AddForgotPasswordOtpAsync(ForgotPasswordOtp otp, CancellationToken cancellationToken = default);

    Task<ForgotPasswordOtp?> GetForgotPasswordOtpAsync(string normalizedEmail, string otp, CancellationToken cancellationToken = default);

    Task RemoveUnusedPasswordResetOtpsAsync(string userId, CancellationToken cancellationToken = default);

    Task AddPasswordResetOtpAsync(PasswordResetOtp otp, CancellationToken cancellationToken = default);

    Task<PasswordResetOtp?> GetPasswordResetOtpAsync(string userId, string otp, CancellationToken cancellationToken = default);

    Task AddProfileUpdateHistoryAsync(ProfileUpdateHistory history, CancellationToken cancellationToken = default);

    Task<bool> HasPendingProfileUpdateRequestAsync(string userId, CancellationToken cancellationToken = default);

    Task AddProfileUpdateRequestAsync(ProfileUpdateRequest request, CancellationToken cancellationToken = default);

    Task<ProfileUpdateRequest?> GetProfileUpdateRequestByIdAsync(string requestId, CancellationToken cancellationToken = default);

    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default);

    Task AddUserAsync(User user, CancellationToken cancellationToken = default);

    Task RevokeUserRefreshTokensAsync(string userId, CancellationToken cancellationToken = default);

    Task RemoveUserAsync(User user, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
