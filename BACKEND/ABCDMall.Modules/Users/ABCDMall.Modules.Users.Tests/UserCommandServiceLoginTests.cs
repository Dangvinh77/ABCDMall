using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class UserCommandServiceLoginTests
{
    [Fact]
    public async Task LoginAsync_returns_password_change_metadata_for_manager_in_initial_password_flow()
    {
        var user = new User
        {
            Id = "manager-1",
            Email = "manager@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Manager",
            FullName = "Mall Manager",
            IsActive = true,
            MustChangePassword = true,
            OneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            OneTimePasswordExpiresAt = DateTime.UtcNow.AddHours(1),
            PasswordSetupToken = "setup-token"
        };

        var repository = new FakeUserCommandRepository(user);
        var configuration = new ConfigurationBuilder().Build();
        var service = new UserCommandService(
            null!,
            repository,
            new FakeEmailNotificationService(),
            new FakeFileStorageService(),
            new FakeTokenService(),
            configuration);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = "manager@example.com",
            Password = "123456"
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.True(result.Value!.RequiresPasswordChange);
        Assert.Equal("setup-token", result.Value.PasswordSetupToken);
        Assert.Equal("You must change your password before continuing", result.Value.Message);
    }

    [Fact]
    public async Task CompleteInitialPasswordChangeAsync_clears_password_setup_state_and_sets_new_password()
    {
        var user = new User
        {
            Id = "manager-2",
            Email = "manager2@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Manager",
            FullName = "Mall Manager Two",
            IsActive = true,
            MustChangePassword = true,
            OneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            OneTimePasswordExpiresAt = DateTime.UtcNow.AddHours(1),
            PasswordSetupToken = "setup-token-2",
            PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        var repository = new FakeUserCommandRepository(user);
        var configuration = new ConfigurationBuilder().Build();
        var service = new UserCommandService(
            null!,
            repository,
            new FakeEmailNotificationService(),
            new FakeFileStorageService(),
            new FakeTokenService(),
            configuration);

        var result = await service.CompleteInitialPasswordChangeAsync(new CompleteInitialPasswordChangeDto
        {
            Token = "setup-token-2",
            NewPassword = "NewPassword!1"
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.False(user.MustChangePassword);
        Assert.Null(user.OneTimePasswordHash);
        Assert.Null(user.PasswordSetupToken);
        Assert.NotNull(user.PasswordSetupCompletedAt);
        Assert.True(BCrypt.Net.BCrypt.Verify("NewPassword!1", user.Password));
    }

    [Fact]
    public async Task RegisterAsync_creates_manager_in_initial_password_state()
    {
        var repository = new FakeUserCommandRepository(null);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:FrontendBaseUrl"] = "http://localhost:5173"
            })
            .Build();
        var emailService = new FakeEmailNotificationService();
        var service = new UserCommandService(
            null!,
            repository,
            emailService,
            new FakeFileStorageService(),
            new FakeTokenService(),
            configuration);

        var result = await service.RegisterAsync(new RegisterDto
        {
            Email = "newmanager@example.com",
            FullName = "New Manager",
            Role = "Manager",
            ShopName = "New Shop",
            CCCD = "123456789"
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.NotNull(repository.AddedUser);
        Assert.True(repository.AddedUser!.MustChangePassword);
        Assert.False(string.IsNullOrWhiteSpace(repository.AddedUser.OneTimePasswordHash));
        Assert.False(string.IsNullOrWhiteSpace(repository.AddedUser.PasswordSetupToken));
        Assert.NotNull(repository.AddedUser.PasswordSetupTokenExpiresAt);
        Assert.True(emailService.ManagerInitialPasswordEmailSent);
    }

    private sealed class FakeUserCommandRepository : IUserCommandRepository
    {
        private readonly User? _user;
        public User? AddedUser { get; private set; }
        public ShopInfo? AddedShopInfo { get; private set; }

        public FakeUserCommandRepository(User? user)
        {
            _user = user;
        }

        public Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && string.Equals(_user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) ? _user : null);

        public Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && _user.Id == userId ? _user : null);

        public Task<User?> GetUserByPasswordSetupTokenAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && string.Equals(_user.PasswordSetupToken, token, StringComparison.Ordinal) ? _user : null);

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
            => Task.FromResult<ForgotPasswordOtp?>(null);

        public Task RemoveUnusedPasswordResetOtpsAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddPasswordResetOtpAsync(PasswordResetOtp otp, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<PasswordResetOtp?> GetPasswordResetOtpAsync(string userId, string otp, CancellationToken cancellationToken = default)
            => Task.FromResult<PasswordResetOtp?>(null);

        public Task<PasswordResetOtp?> GetLatestPasswordResetOtpByUserIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<PasswordResetOtp?>(null);

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
        {
            AddedShopInfo = shopInfo;
            return Task.CompletedTask;
        }

        public Task AddUserAsync(User user, CancellationToken cancellationToken = default)
        {
            AddedUser = user;
            return Task.CompletedTask;
        }

        public Task RevokeUserRefreshTokensAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveUserAsync(User user, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeEmailNotificationService : IEmailNotificationService
    {
        public bool ManagerInitialPasswordEmailSent { get; private set; }

        public Task<bool> SendManagerRegistrationSuccessEmailAsync(string toEmail, string? fullName) => Task.FromResult(true);
        public Task<bool> SendManagerInitialPasswordEmailAsync(string toEmail, string? fullName, string oneTimePassword, string changePasswordUrl)
        {
            ManagerInitialPasswordEmailSent = true;
            return Task.FromResult(true);
        }
        public Task<bool> SendManagerAccountUpdatedEmailAsync(string toEmail, string? fullName, string? shopName) => Task.FromResult(true);
        public Task<bool> SendManagerAccountDeletedEmailAsync(string toEmail, string? fullName, string? shopName) => Task.FromResult(true);
        public Task<bool> SendResetPasswordOtpEmailAsync(string toEmail, string? fullName, string otp) => Task.FromResult(true);
        public Task<bool> SendForgotPasswordOtpEmailAsync(string toEmail, string? fullName, string otp) => Task.FromResult(true);
        public Task<bool> SendLoginOtpEmailAsync(string toEmail, string? fullName, string otp) => Task.FromResult(true);
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("avatar");
        public Task<string> SaveCccdImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("cccd");
        public Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("contract");
        public Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("logo");
        public Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("cover");
        public Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("product");
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateAccessToken(User user) => "access-token";
        public string GenerateRefreshToken() => "refresh-token";
    }
}
