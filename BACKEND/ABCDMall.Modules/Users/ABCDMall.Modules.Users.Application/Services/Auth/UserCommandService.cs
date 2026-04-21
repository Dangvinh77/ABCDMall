using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public sealed class UserCommandService : IUserCommandService
{
    private const string AdminRole = "Admin";
    private const string ManagerRole = "Manager";
    private const string MoviesAdminRole = "MoviesAdmin";
    private readonly AutoMapper.IMapper _mapper;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITokenService _tokenService;

    public UserCommandService(
        AutoMapper.IMapper mapper,
        IUserCommandRepository userCommandRepository,
        IEmailNotificationService emailNotificationService,
        IFileStorageService fileStorageService,
        ITokenService tokenService)
    {
        _mapper = mapper;
        _userCommandRepository = userCommandRepository;
        _emailNotificationService = emailNotificationService;
        _fileStorageService = fileStorageService;
        _tokenService = tokenService;
    }

    public async Task<ApplicationResult<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userCommandRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<LoginResponseDto>.Unauthorized("Email does not exist");
        }

        if (user.FailedLoginAttempts >= 5)
        {
            var requiresNewOtp = string.IsNullOrWhiteSpace(user.LoginOtpCode)
                || user.LoginOtpExpiresAt is null
                || user.LoginOtpExpiresAt < DateTime.UtcNow;

            if (requiresNewOtp)
            {
                var otpSent = await IssueLoginOtpAsync(user, cancellationToken);
                if (!otpSent)
                {
                    return ApplicationResult<LoginResponseDto>.BadRequest(
                        "Unable to send OTP by email",
                        new LoginResponseDto { RequiresOtp = true, Message = "Unable to send OTP by email" });
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Otp))
            {
                const string message = "You entered incorrect credentials 5 times in a row. Please enter the OTP sent to your email to continue signing in";
                return ApplicationResult<LoginResponseDto>.BadRequest(
                    message,
                    new LoginResponseDto { RequiresOtp = true, Message = message });
            }

            if (user.LoginOtpExpiresAt < DateTime.UtcNow)
            {
                var otpSent = await IssueLoginOtpAsync(user, cancellationToken);
                var message = otpSent
                    ? "The OTP has expired. A new code has been sent to your email"
                    : "The OTP has expired and a new one could not be sent";

                return ApplicationResult<LoginResponseDto>.BadRequest(
                    message,
                    new LoginResponseDto { RequiresOtp = true, Message = message });
            }

            if (!string.Equals(user.LoginOtpCode, dto.Otp, StringComparison.Ordinal))
            {
                return ApplicationResult<LoginResponseDto>.BadRequest(
                    "Invalid OTP",
                    new LoginResponseDto { RequiresOtp = true, Message = "Invalid OTP" });
            }
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            user.FailedLoginAttempts += 1;
            user.UpdatedAt = DateTime.UtcNow;
            await _userCommandRepository.SaveChangesAsync(cancellationToken);

            if (user.FailedLoginAttempts >= 5)
            {
                var otpSent = await IssueLoginOtpAsync(user, cancellationToken);
                var message = otpSent
                    ? "You entered incorrect credentials 5 times in a row. An OTP has been sent to your email. From the 6th attempt, you must enter the OTP to continue signing in"
                    : "You entered incorrect credentials 5 times in a row. From the 6th attempt, you must enter the OTP to continue signing in";

                return ApplicationResult<LoginResponseDto>.Unauthorized(
                    message,
                    new LoginResponseDto { RequiresOtp = true, Message = message });
            }

            return ApplicationResult<LoginResponseDto>.Unauthorized("Incorrect password");
        }

        if (user.FailedLoginAttempts >= 5)
        {
            ClearLoginOtpState(user);
        }
        else if (user.FailedLoginAttempts > 0)
        {
            ResetFailedLoginAttempts(user);
        }

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        await _userCommandRepository.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.Id ?? string.Empty,
            Token = refreshTokenValue,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        }, cancellationToken);

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<LoginResponseDto>.Ok(new LoginResponseDto
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = refreshTokenValue
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> RequestForgotPasswordOtpAsync(RequestForgotPasswordOtpDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Email and new password are required");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userCommandRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Email does not exist");
        }

        await _userCommandRepository.RemoveUnusedForgotPasswordOtpsAsync(normalizedEmail, cancellationToken);
        var otp = CreateOtp();
        await _userCommandRepository.AddForgotPasswordOtpAsync(new ForgotPasswordOtp
        {
            UserId = user.Id ?? string.Empty,
            Email = user.Email,
            Otp = otp,
            NewPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        }, cancellationToken);

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        var emailSent = await _emailNotificationService.SendForgotPasswordOtpEmailAsync(user.Email, user.FullName, otp);
        if (!emailSent)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Unable to send OTP by email");
        }

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "The OTP has been sent to your email"
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> ConfirmForgotPasswordOtpAsync(ConfirmForgotPasswordOtpDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Email and OTP are required");
        }

        var forgotPasswordOtp = await _userCommandRepository.GetForgotPasswordOtpAsync(dto.Email.Trim().ToLowerInvariant(), dto.Otp, cancellationToken);
        if (forgotPasswordOtp is null)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Invalid OTP");
        }

        if (forgotPasswordOtp.ExpiresAt < DateTime.UtcNow)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("The OTP has expired");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(forgotPasswordOtp.UserId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("User does not exist");
        }

        user.Password = forgotPasswordOtp.NewPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;
        forgotPasswordOtp.IsUsed = true;
        forgotPasswordOtp.UsedAt = DateTime.UtcNow;

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Password reset successful"
        });
    }

    public async Task<ApplicationResult<UpdateProfileResponseDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<UpdateProfileResponseDto>.Unauthorized("Invalid token");
        }

        var normalizedFullName = NormalizeOptionalValue(dto.FullName);
        var normalizedAddress = NormalizeOptionalValue(dto.Address);
        var normalizedCccd = NormalizeOptionalValue(dto.CCCD);
        var resolvedImagePath = user.Image;

        if (dto.Avatar is not null)
        {
            resolvedImagePath = await _fileStorageService.SaveProfileAvatarAsync(dto.Avatar, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(dto.Image))
        {
            resolvedImagePath = NormalizeOptionalValue(dto.Image);
        }

        var hasChanges = !string.Equals(user.FullName, normalizedFullName, StringComparison.Ordinal)
            || !string.Equals(user.Address, normalizedAddress, StringComparison.Ordinal)
            || !string.Equals(user.Image, resolvedImagePath, StringComparison.Ordinal)
            || !string.Equals(user.CCCD, normalizedCccd, StringComparison.Ordinal);

        if (!hasChanges)
        {
            return ApplicationResult<UpdateProfileResponseDto>.BadRequest("No profile changes were detected");
        }

        await _userCommandRepository.AddProfileUpdateHistoryAsync(new ProfileUpdateHistory
        {
            UserId = user.Id ?? string.Empty,
            Email = user.Email,
            PreviousFullName = user.FullName,
            PreviousAddress = user.Address,
            PreviousImage = user.Image,
            PreviousCCCD = user.CCCD,
            UpdatedFullName = normalizedFullName,
            UpdatedAddress = normalizedAddress,
            UpdatedImage = resolvedImagePath,
            UpdatedCCCD = normalizedCccd,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        user.FullName = normalizedFullName;
        user.Address = normalizedAddress;
        user.Image = resolvedImagePath;
        user.CCCD = normalizedCccd;
        user.UpdatedAt = DateTime.UtcNow;

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<UpdateProfileResponseDto>.Ok(new UpdateProfileResponseDto
        {
            Message = "Profile updated successfully",
            Profile = _mapper.Map<UserProfileResponseDto>(user)
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> RequestResetPasswordOtpAsync(string userId, RequestResetPasswordOtpDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Current password and new password are required");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("User does not exist");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("The current password is incorrect");
        }

        await _userCommandRepository.RemoveUnusedPasswordResetOtpsAsync(userId, cancellationToken);
        var otp = CreateOtp();
        await _userCommandRepository.AddPasswordResetOtpAsync(new PasswordResetOtp
        {
            UserId = user.Id ?? string.Empty,
            Otp = otp,
            NewPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        }, cancellationToken);

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        var emailSent = await _emailNotificationService.SendResetPasswordOtpEmailAsync(user.Email, user.FullName, otp);
        if (!emailSent)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Unable to send OTP by email");
        }

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "The OTP has been sent to your email"
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> ConfirmResetPasswordOtpAsync(string userId, ConfirmResetPasswordOtpDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Otp))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("OTP is required");
        }

        var resetOtp = await _userCommandRepository.GetPasswordResetOtpAsync(userId, dto.Otp, cancellationToken);
        if (resetOtp is null)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Invalid OTP");
        }

        if (resetOtp.ExpiresAt < DateTime.UtcNow)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("The OTP has expired");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("User does not exist");
        }

        user.Password = resetOtp.NewPasswordHash;
        user.UpdatedAt = DateTime.UtcNow;
        resetOtp.IsUsed = true;
        resetOtp.UsedAt = DateTime.UtcNow;

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Password reset successful"
        });
    }

    public async Task<ApplicationResult<UserAccountMutationResponseDto>> UpdateUserAccountAsync(string userId, UpdateUserAccountDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.FullName))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Email and full name are required");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.NotFound("User does not exist");
        }

        if (user.Role == AdminRole)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Admin accounts cannot be updated here");
        }

        var resolvedRole = NormalizeManagedRole(dto.Role, user.Role);
        if (resolvedRole is null)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Only Manager and MoviesAdmin roles are supported");
        }

        if (!string.Equals(resolvedRole, user.Role, StringComparison.Ordinal))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Changing account role between Manager and MoviesAdmin is not supported here");
        }

        var normalizedEmail = dto.Email.Trim();
        var normalizedFullName = dto.FullName.Trim();
        var normalizedAddress = NormalizeOptionalValue(dto.Address);
        var normalizedShopName = NormalizeOptionalValue(dto.ShopName);
        var normalizedCccd = NormalizeOptionalValue(dto.CCCD);

        if (await _userCommandRepository.ExistsUserByEmailAsync(normalizedEmail.ToLowerInvariant(), userId, cancellationToken))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Email already exists");
        }

        if (resolvedRole == ManagerRole
            && (string.IsNullOrWhiteSpace(normalizedShopName) || string.IsNullOrWhiteSpace(normalizedCccd)))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Shop name and CCCD are required for Manager accounts");
        }

        if (!string.IsNullOrWhiteSpace(normalizedCccd)
            && (await _userCommandRepository.ExistsUserByCccdAsync(normalizedCccd, userId, cancellationToken)
                || await _userCommandRepository.ExistsShopInfoByCccdAsync(normalizedCccd, user.ShopId, cancellationToken)))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("CCCD already exists");
        }

        user.Email = normalizedEmail;
        user.FullName = normalizedFullName;
        user.Address = normalizedAddress;
        user.CCCD = normalizedCccd;
        user.Role = resolvedRole;
        user.UpdatedAt = DateTime.UtcNow;

        if (resolvedRole == ManagerRole && !string.IsNullOrWhiteSpace(user.ShopId))
        {
            var shopInfo = await _userCommandRepository.GetShopInfoByIdAsync(user.ShopId, cancellationToken);
            if (shopInfo is not null)
            {
                shopInfo.ShopName = normalizedShopName!;
                shopInfo.ManagerName = normalizedFullName;
                shopInfo.CCCD = normalizedCccd!;
            }
        }

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        var emailSent = resolvedRole == ManagerRole
            ? await TrySendAsync(() =>
                _emailNotificationService.SendManagerAccountUpdatedEmailAsync(normalizedEmail, normalizedFullName, normalizedShopName!))
            : false;

        return ApplicationResult<UserAccountMutationResponseDto>.Ok(new UserAccountMutationResponseDto
        {
            Message = "User account updated successfully",
            EmailSent = emailSent
        });
    }

    public async Task<ApplicationResult<UserAccountMutationResponseDto>> DeleteUserAccountAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.NotFound("User does not exist");
        }

        if (user.Role == AdminRole)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Admin accounts cannot be deleted here");
        }

        var shopInfo = !string.IsNullOrWhiteSpace(user.ShopId)
            ? await _userCommandRepository.GetShopInfoByIdAsync(user.ShopId, cancellationToken)
            : null;

        var emailSent = user.Role == ManagerRole
            ? await TrySendAsync(() =>
                _emailNotificationService.SendManagerAccountDeletedEmailAsync(user.Email, user.FullName, shopInfo?.ShopName))
            : false;

        await _userCommandRepository.RemoveUserRelatedDataAsync(userId, cancellationToken);
        await _userCommandRepository.RemoveUserAsync(user, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<UserAccountMutationResponseDto>.Ok(new UserAccountMutationResponseDto
        {
            Message = "User account deleted successfully",
            EmailSent = emailSent
        });
    }

    public async Task<ApplicationResult<AccessTokenResponseDto>> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _userCommandRepository.GetRefreshTokenAsync(dto.RefreshToken, cancellationToken);
        if (refreshToken is null)
        {
            return ApplicationResult<AccessTokenResponseDto>.Unauthorized("Refresh token does not exist");
        }

        if (refreshToken.IsRevoked)
        {
            return ApplicationResult<AccessTokenResponseDto>.Unauthorized("Refresh token has been revoked");
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            return ApplicationResult<AccessTokenResponseDto>.Unauthorized("Refresh token has expired");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(refreshToken.UserId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<AccessTokenResponseDto>.Unauthorized("User does not exist");
        }

        return ApplicationResult<AccessTokenResponseDto>.Ok(new AccessTokenResponseDto
        {
            AccessToken = _tokenService.GenerateAccessToken(user)
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> LogoutAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _userCommandRepository.GetRefreshTokenAsync(dto.RefreshToken, cancellationToken);
        if (refreshToken is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Refresh token does not exist");
        }

        if (refreshToken.IsRevoked)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("The token has already been revoked");
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Logout successful"
        });
    }

    public async Task<ApplicationResult<RegisterUserResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email)
            || string.IsNullOrWhiteSpace(dto.Password)
            || string.IsNullOrWhiteSpace(dto.FullName))
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Email, password, and full name are required");
        }

        var resolvedRole = NormalizeManagedRole(dto.Role, ManagerRole);
        if (resolvedRole is null)
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Only Manager and MoviesAdmin roles are supported");
        }

        var normalizedEmail = dto.Email.Trim();
        var normalizedAddress = NormalizeOptionalValue(dto.Address);
        var normalizedShopName = NormalizeOptionalValue(dto.ShopName);
        var normalizedCccd = NormalizeOptionalValue(dto.CCCD);

        if (await _userCommandRepository.ExistsUserByEmailAsync(normalizedEmail.ToLowerInvariant(), null, cancellationToken))
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Email already exists");
        }

        if (resolvedRole == ManagerRole
            && (string.IsNullOrWhiteSpace(normalizedShopName) || string.IsNullOrWhiteSpace(normalizedCccd)))
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Shop name and CCCD are required for Manager accounts");
        }

        if (!string.IsNullOrWhiteSpace(normalizedCccd)
            && (await _userCommandRepository.ExistsUserByCccdAsync(normalizedCccd, null, cancellationToken)
                || await _userCommandRepository.ExistsShopInfoByCccdAsync(normalizedCccd, null, cancellationToken)))
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("CCCD already exists");
        }

        ShopInfo? shopInfo = null;
        if (resolvedRole == ManagerRole)
        {
            shopInfo = new ShopInfo
            {
                ShopName = normalizedShopName!,
                CCCD = normalizedCccd!,
                CreatedAt = DateTime.UtcNow
            };
            await _userCommandRepository.AddShopInfoAsync(shopInfo, cancellationToken);
        }

        var user = new User
        {
            Email = normalizedEmail,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName.Trim(),
            Address = normalizedAddress,
            Role = resolvedRole,
            ShopId = shopInfo?.Id,
            CCCD = normalizedCccd,
            CreatedAt = DateTime.UtcNow
        };
        await _userCommandRepository.AddUserAsync(user, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        var emailSent = resolvedRole == ManagerRole
            ? await TrySendAsync(() =>
                _emailNotificationService.SendManagerRegistrationSuccessEmailAsync(user.Email, user.FullName))
            : false;

        return ApplicationResult<RegisterUserResponseDto>.Ok(new RegisterUserResponseDto
        {
            Message = "User created successfully",
            EmailSent = emailSent,
            Email = user.Email,
            Role = user.Role,
            ShopId = user.ShopId,
            CCCD = user.CCCD,
            ShopName = shopInfo?.ShopName ?? string.Empty,
            CreatedAt = user.CreatedAt
        });
    }

    private static string? NormalizeManagedRole(string? requestedRole, string fallbackRole)
    {
        var normalizedRole = string.IsNullOrWhiteSpace(requestedRole)
            ? fallbackRole
            : requestedRole.Trim();

        return normalizedRole switch
        {
            ManagerRole => ManagerRole,
            MoviesAdminRole => MoviesAdminRole,
            _ => null
        };
    }

    private async Task<bool> IssueLoginOtpAsync(User user, CancellationToken cancellationToken)
    {
        var otp = CreateOtp();
        user.LoginOtpCode = otp;
        user.LoginOtpExpiresAt = DateTime.UtcNow.AddMinutes(5);
        user.UpdatedAt = DateTime.UtcNow;
        await _userCommandRepository.SaveChangesAsync(cancellationToken);
        return await _emailNotificationService.SendLoginOtpEmailAsync(user.Email, user.FullName, otp);
    }

    private static string CreateOtp()
        => Random.Shared.Next(100000, 999999).ToString();

    private static string? NormalizeOptionalValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ResetFailedLoginAttempts(User user)
    {
        user.FailedLoginAttempts = 0;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private static void ClearLoginOtpState(User user)
    {
        user.FailedLoginAttempts = 0;
        user.LoginOtpCode = null;
        user.LoginOtpExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;
    }

    private static async Task<bool> TrySendAsync(Func<Task<bool>> send)
    {
        try
        {
            return await send();
        }
        catch
        {
            return false;
        }
    }
}
