using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class DevOtpDebugServiceTests
{
    [Fact]
    public async Task GetOtpAsync_returns_current_login_forgot_and_reset_otps_without_revealing_existing_initial_password_otp()
    {
        var user = new User
        {
            Id = "manager-debug-1",
            Email = "manager.debug@example.com",
            FullName = "Manager Debug",
            Role = "Manager",
            IsActive = true,
            MustChangePassword = true,
            LoginOtpCode = "654321",
            LoginOtpExpiresAt = DateTime.UtcNow.AddMinutes(5),
            PasswordSetupToken = "existing-setup-token",
            PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddHours(8)
        };

        var repository = new FakeDebugRepository(user)
        {
            ForgotPasswordOtp = new ForgotPasswordOtp
            {
                Email = user.Email,
                Otp = "111111",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            },
            PasswordResetOtp = new PasswordResetOtp
            {
                UserId = user.Id!,
                Otp = "222222",
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            }
        };
        var service = CreateService(repository);

        var result = await service.GetOtpAsync(new DebugOtpLookupRequestDto
        {
            Email = user.Email
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("654321", result.Value!.LoginOtp);
        Assert.Equal("111111", result.Value.ForgotPasswordOtp);
        Assert.Equal("222222", result.Value.ResetPasswordOtp);
        Assert.True(result.Value.MustChangePassword);
        Assert.Equal("existing-setup-token", result.Value.PasswordSetupToken);
        Assert.Null(result.Value.InitialPasswordOtp);
        Assert.False(result.Value.InitialPasswordOtpReissued);
        Assert.Contains("existing-setup-token", result.Value.ChangePasswordUrl);
    }

    [Fact]
    public async Task GetOtpAsync_regenerates_initial_password_otp_for_manager_when_requested()
    {
        var user = new User
        {
            Id = "manager-debug-2",
            Email = "manager.reissue@example.com",
            FullName = "Manager Reissue",
            Role = "Manager",
            IsActive = true,
            MustChangePassword = true,
            Password = BCrypt.Net.BCrypt.HashPassword("OldOtp1!"),
            OneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword("OldOtp1!"),
            OneTimePasswordExpiresAt = DateTime.UtcNow.AddMinutes(-30),
            PasswordSetupToken = "old-token",
            PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var repository = new FakeDebugRepository(user);
        var service = CreateService(repository);

        var result = await service.GetOtpAsync(new DebugOtpLookupRequestDto
        {
            UserId = user.Id,
            RegenerateInitialPasswordOtp = true
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.True(result.Value!.InitialPasswordOtpReissued);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.InitialPasswordOtp));
        Assert.NotEqual("old-token", result.Value.PasswordSetupToken);
        Assert.True(repository.RevokeRefreshTokensCalled);
        Assert.NotNull(user.OneTimePasswordExpiresAt);
        Assert.True(user.OneTimePasswordExpiresAt > DateTime.UtcNow);
        Assert.NotNull(user.PasswordSetupTokenExpiresAt);
        Assert.True(user.PasswordSetupTokenExpiresAt > DateTime.UtcNow);
    }

    private static DevOtpDebugService CreateService(FakeDebugRepository repository)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:FrontendBaseUrl"] = "http://localhost:5173"
            })
            .Build();

        return new DevOtpDebugService(repository, configuration);
    }

    private sealed class FakeDebugRepository : IUserCommandRepository
    {
        private readonly User? _user;

        public FakeDebugRepository(User? user)
        {
            _user = user;
        }

        public ForgotPasswordOtp? ForgotPasswordOtp { get; set; }
        public PasswordResetOtp? PasswordResetOtp { get; set; }
        public bool RevokeRefreshTokensCalled { get; private set; }

        public Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && string.Equals(_user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) ? _user : null);

        public Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && _user.Id == userId ? _user : null);

        public Task<User?> GetUserByPasswordSetupTokenAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(null);

        public Task<bool> ExistsUserByEmailAsync(string normalizedEmail, string? excludedUserId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsUserByCccdAsync(string normalizedCccd, string? excludedUserId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsShopInfoByCccdAsync(string normalizedCccd, string? excludedShopId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<ShopInfo?> GetShopInfoByIdAsync(string shopId, CancellationToken cancellationToken = default)
            => Task.FromResult<ShopInfo?>(null);

        public Task<bool> HasActiveRentalAreaAsync(string? shopId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task RemoveUnusedForgotPasswordOtpsAsync(string normalizedEmail, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddForgotPasswordOtpAsync(ForgotPasswordOtp otp, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<ForgotPasswordOtp?> GetForgotPasswordOtpAsync(string normalizedEmail, string otp, CancellationToken cancellationToken = default)
            => Task.FromResult<ForgotPasswordOtp?>(null);

        public Task<ForgotPasswordOtp?> GetLatestForgotPasswordOtpByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
            => Task.FromResult(ForgotPasswordOtp);

        public Task RemoveUnusedPasswordResetOtpsAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddPasswordResetOtpAsync(PasswordResetOtp otp, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PasswordResetOtp?> GetPasswordResetOtpAsync(string userId, string otp, CancellationToken cancellationToken = default)
            => Task.FromResult<PasswordResetOtp?>(null);

        public Task<PasswordResetOtp?> GetLatestPasswordResetOtpByUserIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(PasswordResetOtp);

        public Task AddProfileUpdateHistoryAsync(ProfileUpdateHistory history, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<bool> HasPendingProfileUpdateRequestAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task AddProfileUpdateRequestAsync(ProfileUpdateRequest request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<ProfileUpdateRequest?> GetProfileUpdateRequestByIdAsync(string requestId, CancellationToken cancellationToken = default)
            => Task.FromResult<ProfileUpdateRequest?>(null);

        public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
            => Task.FromResult<RefreshToken?>(null);

        public Task AddShopInfoAsync(ShopInfo shopInfo, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RevokeUserRefreshTokensAsync(string userId, CancellationToken cancellationToken = default)
        {
            RevokeRefreshTokensCalled = true;
            return Task.CompletedTask;
        }

        public Task RemoveUserAsync(User user, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
