using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.Mappings;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Application.Services.Auth;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class UserCommandServiceAccountLifecycleTests
{
    [Fact]
    public async Task UpdateProfileAsync_for_manager_submits_pending_request_instead_of_mutating_user()
    {
        var user = new User
        {
            Id = "manager-approval-1",
            Email = "manager.approval@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password!1"),
            Role = "Manager",
            FullName = "Current Manager",
            Address = "Current Address",
            CCCD = "111111111",
            CccdFrontImage = "/images/cccd/current-front.png",
            CccdBackImage = "/images/cccd/current-back.png",
            IsActive = true
        };

        var repository = new FakeLifecycleRepository(user)
        {
            ExistsShopInfoByCccdResult = false,
            ExistsUserByCccdResult = false
        };
        var service = CreateService(repository);

        var result = await service.UpdateProfileAsync(user.Id!, new UpdateProfileDto
        {
            FullName = "Updated Manager",
            Address = "Updated Address",
            CCCD = "222222222",
            CccdFrontImage = CreateFormFile("front.png"),
            CccdBackImage = CreateFormFile("back.png")
        });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.Equal("Profile update request submitted for admin approval", result.Value!.Message);
        Assert.NotNull(repository.AddedProfileUpdateRequest);
        Assert.Equal("Pending", repository.AddedProfileUpdateRequest!.Status);
        Assert.Equal("Updated Manager", repository.AddedProfileUpdateRequest.RequestedFullName);
        Assert.Equal("Current Manager", user.FullName);
        Assert.Equal("111111111", user.CCCD);
    }

    [Fact]
    public async Task ApproveProfileUpdateRequestAsync_applies_requested_fields_and_marks_request_approved()
    {
        var user = new User
        {
            Id = "manager-approval-2",
            Email = "manager.review@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password!1"),
            Role = "Manager",
            FullName = "Current Manager",
            Address = "Current Address",
            CCCD = "111111111",
            ShopId = "shop-1",
            IsActive = true
        };
        var request = new ProfileUpdateRequest
        {
            Id = "request-1",
            UserId = user.Id!,
            Email = user.Email,
            CurrentFullName = user.FullName,
            CurrentAddress = user.Address,
            CurrentCCCD = user.CCCD,
            RequestedFullName = "Approved Manager",
            RequestedAddress = "Approved Address",
            RequestedCCCD = "222222222",
            RequestedCccdFrontImage = "/images/cccd/front-new.png",
            RequestedCccdBackImage = "/images/cccd/back-new.png",
            Status = "Pending"
        };
        var shopInfo = new ShopInfo
        {
            Id = "shop-1",
            ShopName = "Shop 1",
            ManagerName = "Current Manager",
            CCCD = "111111111"
        };

        var repository = new FakeLifecycleRepository(user)
        {
            ExistingRequest = request,
            ShopInfo = shopInfo,
            ExistsShopInfoByCccdResult = false,
            ExistsUserByCccdResult = false
        };
        var service = CreateService(repository);

        var result = await service.ApproveProfileUpdateRequestAsync(
            "request-1",
            "admin-1",
            new ProfileUpdateRequestDecisionDto { ReviewNote = "Looks good" });

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.Equal("Approved Manager", user.FullName);
        Assert.Equal("Approved Address", user.Address);
        Assert.Equal("222222222", user.CCCD);
        Assert.Equal("/images/cccd/front-new.png", user.CccdFrontImage);
        Assert.Equal("/images/cccd/back-new.png", user.CccdBackImage);
        Assert.Equal("Approved", request.Status);
        Assert.Equal("admin-1", request.ReviewedByAdminId);
        Assert.Equal("Looks good", request.ReviewNote);
        Assert.Equal("Approved Manager", shopInfo.ManagerName);
        Assert.Equal("222222222", shopInfo.CCCD);
    }

    [Fact]
    public async Task DeleteUserAccountAsync_deactivates_manager_and_revokes_refresh_tokens()
    {
        var user = new User
        {
            Id = "manager-approval-3",
            Email = "manager.deactivate@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Password!1"),
            Role = "Manager",
            FullName = "Deactivate Manager",
            ShopId = "shop-2",
            IsActive = true
        };

        var repository = new FakeLifecycleRepository(user)
        {
            HasActiveRentalAreaResult = false,
            ShopInfo = new ShopInfo { Id = "shop-2", ShopName = "Shop 2" }
        };
        var service = CreateService(repository);

        var result = await service.DeleteUserAccountAsync(user.Id!);

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.False(user.IsActive);
        Assert.True(repository.RevokeRefreshTokensCalled);
        Assert.False(repository.RemoveUserCalled);
        Assert.Equal("User account deactivated successfully", result.Value!.Message);
    }

    [Fact]
    public async Task ResendInitialPasswordLinkAsync_reissues_password_setup_credentials_for_manager()
    {
        var user = new User
        {
            Id = "manager-approval-4",
            Email = "manager.resend@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("OldOtp1!"),
            Role = "Manager",
            FullName = "Resend Manager",
            IsActive = true,
            MustChangePassword = true,
            OneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword("OldOtp1!"),
            OneTimePasswordExpiresAt = DateTime.UtcNow.AddMinutes(-30),
            PasswordSetupToken = "old-token",
            PasswordSetupTokenExpiresAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var repository = new FakeLifecycleRepository(user);
        var emailService = new FakeLifecycleEmailNotificationService();
        var service = CreateService(repository, emailService);

        var result = await service.ResendInitialPasswordLinkAsync(user.Id!);

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.True(repository.RevokeRefreshTokensCalled);
        Assert.True(emailService.ManagerInitialPasswordEmailSent);
        Assert.NotEqual("old-token", user.PasswordSetupToken);
        Assert.NotNull(user.OneTimePasswordExpiresAt);
        Assert.True(user.OneTimePasswordExpiresAt > DateTime.UtcNow);
    }

    private static UserCommandService CreateService(
        FakeLifecycleRepository repository,
        FakeLifecycleEmailNotificationService? emailService = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailSettings:FrontendBaseUrl"] = "http://localhost:5173"
            })
            .Build();
        using var loggerFactory = LoggerFactory.Create(_ => { });
        var mapper = new AutoMapper.MapperConfiguration(cfg => cfg.AddProfile<UsersProfile>(), loggerFactory).CreateMapper();

        return new UserCommandService(
            mapper,
            repository,
            emailService ?? new FakeLifecycleEmailNotificationService(),
            new FakeLifecycleFileStorageService(),
            new FakeLifecycleTokenService(),
            configuration);
    }

    private static FormFile CreateFormFile(string fileName)
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName);
    }

    private sealed class FakeLifecycleRepository : IUserCommandRepository
    {
        private readonly User? _user;

        public FakeLifecycleRepository(User? user)
        {
            _user = user;
        }

        public bool ExistsUserByCccdResult { get; set; }
        public bool ExistsShopInfoByCccdResult { get; set; }
        public bool HasActiveRentalAreaResult { get; set; }
        public bool RevokeRefreshTokensCalled { get; private set; }
        public bool RemoveUserCalled { get; private set; }
        public ProfileUpdateRequest? AddedProfileUpdateRequest { get; private set; }
        public ProfileUpdateRequest? ExistingRequest { get; set; }
        public ShopInfo? ShopInfo { get; set; }

        public Task<User?> GetUserByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && string.Equals(_user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) ? _user : null);

        public Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && _user.Id == userId ? _user : null);

        public Task<User?> GetUserByPasswordSetupTokenAsync(string token, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user is not null && string.Equals(_user.PasswordSetupToken, token, StringComparison.Ordinal) ? _user : null);

        public Task<bool> ExistsUserByEmailAsync(string normalizedEmail, string? excludedUserId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsUserByCccdAsync(string normalizedCccd, string? excludedUserId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistsUserByCccdResult);

        public Task<bool> ExistsShopInfoByCccdAsync(string normalizedCccd, string? excludedShopId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(ExistsShopInfoByCccdResult);

        public Task<ShopInfo?> GetShopInfoByIdAsync(string shopId, CancellationToken cancellationToken = default)
            => Task.FromResult(ShopInfo);

        public Task<bool> HasActiveRentalAreaAsync(string? shopId, CancellationToken cancellationToken = default)
            => Task.FromResult(HasActiveRentalAreaResult);

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
        {
            AddedProfileUpdateRequest = request;
            return Task.CompletedTask;
        }

        public Task<ProfileUpdateRequest?> GetProfileUpdateRequestByIdAsync(string requestId, CancellationToken cancellationToken = default)
            => Task.FromResult<ProfileUpdateRequest?>(ExistingRequest is not null && ExistingRequest.Id == requestId ? ExistingRequest : null);

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

        public Task RemoveUserRelatedDataAsync(string userId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task RemoveUserAsync(User user, CancellationToken cancellationToken = default)
        {
            RemoveUserCalled = true;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeLifecycleEmailNotificationService : IEmailNotificationService
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

    private sealed class FakeLifecycleFileStorageService : IFileStorageService
    {
        public Task<string> SaveProfileAvatarAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/images/profiles/avatar.png");
        public Task<string> SaveCccdImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult($"/images/cccd/{file.FileName}");
        public Task<string> SaveContractImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/images/contracts/contract.png");
        public Task<string> SaveShopLogoAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/images/shops/logos/logo.png");
        public Task<string> SaveShopCoverAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/images/shops/covers/cover.png");
        public Task<string> SaveShopProductImageAsync(IFormFile file, CancellationToken cancellationToken = default) => Task.FromResult("/images/shops/products/product.png");
    }

    private sealed class FakeLifecycleTokenService : ITokenService
    {
        public string GenerateAccessToken(User user) => "access-token";
        public string GenerateRefreshToken() => "refresh-token";
    }
}
