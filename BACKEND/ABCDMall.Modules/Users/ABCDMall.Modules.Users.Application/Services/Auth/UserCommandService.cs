using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.DTOs.Common;
using ABCDMall.Modules.Users.Application.DTOs;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public sealed class UserCommandService : IUserCommandService
{
    private readonly AutoMapper.IMapper _mapper;
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public UserCommandService(
        AutoMapper.IMapper mapper,
        IUserCommandRepository userCommandRepository,
        IEmailNotificationService emailNotificationService,
        IFileStorageService fileStorageService,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _mapper = mapper;
        _userCommandRepository = userCommandRepository;
        _emailNotificationService = emailNotificationService;
        _fileStorageService = fileStorageService;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<ApplicationResult<LoginResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var user = await _userCommandRepository.GetUserByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<LoginResponseDto>.Unauthorized("Email does not exist");
        }

        if (!user.IsActive)
        {
            return ApplicationResult<LoginResponseDto>.Unauthorized("This account is inactive");
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

        if (user.MustChangePassword)
        {
            if (user.OneTimePasswordExpiresAt is null || user.OneTimePasswordExpiresAt < DateTime.UtcNow)
            {
                return ApplicationResult<LoginResponseDto>.Unauthorized("The one-time password has expired. Please ask an admin to resend the password setup link");
            }

            if (user.OneTimePasswordUsedAt is not null)
            {
                return ApplicationResult<LoginResponseDto>.Unauthorized("The one-time password has already been used. Please use the password setup link from your email or ask an admin to resend it");
            }

            if (string.IsNullOrWhiteSpace(user.OneTimePasswordHash) || !BCrypt.Net.BCrypt.Verify(dto.Password, user.OneTimePasswordHash))
            {
                return ApplicationResult<LoginResponseDto>.Unauthorized("Invalid one-time password");
            }

            user.OneTimePasswordUsedAt = DateTime.UtcNow;
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
            RefreshToken = refreshTokenValue,
            RequiresPasswordChange = user.MustChangePassword,
            PasswordSetupToken = user.MustChangePassword ? user.PasswordSetupToken : null,
            Message = user.MustChangePassword ? "You must change your password before continuing" : null
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

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
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

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
        }

        user.Password = forgotPasswordOtp.NewPasswordHash;
        ClearPasswordSetupState(user);
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

        if (!user.IsActive)
        {
            return ApplicationResult<UpdateProfileResponseDto>.Unauthorized("This account is inactive");
        }

        var normalizedFullName = NormalizeOptionalValue(dto.FullName);
        var normalizedAddress = NormalizeOptionalValue(dto.Address);
        var normalizedCccd = NormalizeOptionalValue(dto.CCCD);
        var resolvedCccdFrontImage = dto.CccdFrontImage is not null
            ? await _fileStorageService.SaveCccdImageAsync(dto.CccdFrontImage, cancellationToken)
            : user.CccdFrontImage;
        var resolvedCccdBackImage = dto.CccdBackImage is not null
            ? await _fileStorageService.SaveCccdImageAsync(dto.CccdBackImage, cancellationToken)
            : user.CccdBackImage;
        var cccdChanged = !string.Equals(user.CCCD, normalizedCccd, StringComparison.Ordinal);

        if (cccdChanged && (string.IsNullOrWhiteSpace(resolvedCccdFrontImage) || string.IsNullOrWhiteSpace(resolvedCccdBackImage)))
        {
            return ApplicationResult<UpdateProfileResponseDto>.BadRequest("CCCD front and back images are required when CCCD is changed");
        }

        var hasChanges = !string.Equals(user.FullName, normalizedFullName, StringComparison.Ordinal)
            || !string.Equals(user.Address, normalizedAddress, StringComparison.Ordinal)
            || cccdChanged
            || !string.Equals(user.CccdFrontImage, resolvedCccdFrontImage, StringComparison.Ordinal)
            || !string.Equals(user.CccdBackImage, resolvedCccdBackImage, StringComparison.Ordinal);

        if (!hasChanges)
        {
            return ApplicationResult<UpdateProfileResponseDto>.BadRequest("No profile changes were detected");
        }

        if (string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(normalizedCccd)
                && (await _userCommandRepository.ExistsUserByCccdAsync(normalizedCccd, user.Id, cancellationToken)
                    || await _userCommandRepository.ExistsShopInfoByCccdAsync(normalizedCccd, user.ShopId, cancellationToken)))
            {
                return ApplicationResult<UpdateProfileResponseDto>.BadRequest("CCCD already exists");
            }

            if (await _userCommandRepository.HasPendingProfileUpdateRequestAsync(user.Id ?? string.Empty, cancellationToken))
            {
                return ApplicationResult<UpdateProfileResponseDto>.BadRequest("You already have a pending profile update request");
            }

            await _userCommandRepository.AddProfileUpdateRequestAsync(new ProfileUpdateRequest
            {
                UserId = user.Id ?? string.Empty,
                Email = user.Email,
                CurrentFullName = user.FullName,
                CurrentAddress = user.Address,
                CurrentCCCD = user.CCCD,
                CurrentCccdFrontImage = user.CccdFrontImage,
                CurrentCccdBackImage = user.CccdBackImage,
                RequestedFullName = normalizedFullName,
                RequestedAddress = normalizedAddress,
                RequestedCCCD = normalizedCccd,
                RequestedCccdFrontImage = resolvedCccdFrontImage,
                RequestedCccdBackImage = resolvedCccdBackImage,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            }, cancellationToken);

            await _userCommandRepository.SaveChangesAsync(cancellationToken);

            return ApplicationResult<UpdateProfileResponseDto>.Ok(new UpdateProfileResponseDto
            {
                Message = "Profile update request submitted for admin approval",
                Profile = _mapper.Map<UserProfileResponseDto>(user)
            });
        }

        await ApplyApprovedProfileUpdateAsync(
            user,
            normalizedFullName,
            normalizedAddress,
            normalizedCccd,
            resolvedCccdFrontImage,
            resolvedCccdBackImage,
            cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<UpdateProfileResponseDto>.Ok(new UpdateProfileResponseDto
        {
            Message = "Profile updated successfully",
            Profile = _mapper.Map<UserProfileResponseDto>(user)
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> ApproveProfileUpdateRequestAsync(
        string requestId,
        string adminUserId,
        ProfileUpdateRequestDecisionDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _userCommandRepository.GetProfileUpdateRequestByIdAsync(requestId, cancellationToken);
        if (request is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Profile update request does not exist");
        }

        if (!string.Equals(request.Status, "Pending", StringComparison.Ordinal))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This profile update request has already been reviewed");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("User does not exist");
        }

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
        }

        if (!string.IsNullOrWhiteSpace(request.RequestedCCCD)
            && (await _userCommandRepository.ExistsUserByCccdAsync(request.RequestedCCCD, user.Id, cancellationToken)
                || await _userCommandRepository.ExistsShopInfoByCccdAsync(request.RequestedCCCD, user.ShopId, cancellationToken)))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("CCCD already exists");
        }

        await ApplyApprovedProfileUpdateAsync(
            user,
            request.RequestedFullName,
            request.RequestedAddress,
            request.RequestedCCCD,
            request.RequestedCccdFrontImage,
            request.RequestedCccdBackImage,
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.ShopId))
        {
            var shopInfo = await _userCommandRepository.GetShopInfoByIdAsync(user.ShopId, cancellationToken);
            if (shopInfo is not null)
            {
                shopInfo.ManagerName = request.RequestedFullName;
                shopInfo.CCCD = request.RequestedCCCD;
            }
        }

        request.Status = "Approved";
        request.ReviewedByAdminId = adminUserId;
        request.ReviewNote = NormalizeOptionalValue(dto.ReviewNote);
        request.ReviewedAt = DateTime.UtcNow;

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Profile update request approved"
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> RejectProfileUpdateRequestAsync(
        string requestId,
        string adminUserId,
        ProfileUpdateRequestDecisionDto dto,
        CancellationToken cancellationToken = default)
    {
        var request = await _userCommandRepository.GetProfileUpdateRequestByIdAsync(requestId, cancellationToken);
        if (request is null)
        {
            return ApplicationResult<MessageResponseDto>.NotFound("Profile update request does not exist");
        }

        if (!string.Equals(request.Status, "Pending", StringComparison.Ordinal))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This profile update request has already been reviewed");
        }

        request.Status = "Rejected";
        request.ReviewedByAdminId = adminUserId;
        request.ReviewNote = NormalizeOptionalValue(dto.ReviewNote);
        request.ReviewedAt = DateTime.UtcNow;

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Profile update request rejected"
        });
    }

    private async Task ApplyApprovedProfileUpdateAsync(
        User user,
        string? normalizedFullName,
        string? normalizedAddress,
        string? normalizedCccd,
        string? cccdFrontImage,
        string? cccdBackImage,
        CancellationToken cancellationToken)
    {
        await _userCommandRepository.AddProfileUpdateHistoryAsync(new ProfileUpdateHistory
        {
            UserId = user.Id ?? string.Empty,
            Email = user.Email,
            PreviousFullName = user.FullName,
            PreviousAddress = user.Address,
            PreviousImage = user.Image,
            PreviousCCCD = user.CCCD,
            PreviousCccdFrontImage = user.CccdFrontImage,
            PreviousCccdBackImage = user.CccdBackImage,
            UpdatedFullName = normalizedFullName,
            UpdatedAddress = normalizedAddress,
            UpdatedImage = user.Image,
            UpdatedCCCD = normalizedCccd,
            UpdatedCccdFrontImage = cccdFrontImage,
            UpdatedCccdBackImage = cccdBackImage,
            UpdatedAt = DateTime.UtcNow
        }, cancellationToken);

        user.FullName = normalizedFullName;
        user.Address = normalizedAddress;
        user.CCCD = normalizedCccd;
        user.CccdFrontImage = cccdFrontImage;
        user.CccdBackImage = cccdBackImage;
        user.UpdatedAt = DateTime.UtcNow;
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

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
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

    public async Task<ApplicationResult<MessageResponseDto>> ResetPasswordAsync(string userId, ResetPasswordDto dto, CancellationToken cancellationToken = default)
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

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("The current password is incorrect");
        }

        if (dto.CurrentPassword == dto.NewPassword)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("The new password must be different from the current password");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        ClearPasswordSetupState(user);
        user.UpdatedAt = DateTime.UtcNow;
        await _userCommandRepository.RemoveUnusedPasswordResetOtpsAsync(userId, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Password reset successful"
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

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
        }

        user.Password = resetOtp.NewPasswordHash;
        ClearPasswordSetupState(user);
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
            || string.IsNullOrWhiteSpace(dto.FullName)
            || string.IsNullOrWhiteSpace(dto.ShopName)
            || string.IsNullOrWhiteSpace(dto.CCCD))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Email, full name, shop name, and CCCD are required");
        }

        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.NotFound("User does not exist");
        }

        if (user.Role == "Admin")
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Admin accounts cannot be updated here");
        }

        var normalizedEmail = dto.Email.Trim();
        var normalizedFullName = dto.FullName.Trim();
        var normalizedShopName = dto.ShopName.Trim();
        var normalizedAddress = NormalizeOptionalValue(dto.Address);
        var resolvedImagePath = user.Image;
        var normalizedCccd = dto.CCCD.Trim();

        if (dto.Avatar is not null)
        {
            resolvedImagePath = await _fileStorageService.SaveProfileAvatarAsync(dto.Avatar, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(dto.Image))
        {
            resolvedImagePath = NormalizeOptionalValue(dto.Image);
        }

        if (await _userCommandRepository.ExistsUserByEmailAsync(normalizedEmail.ToLowerInvariant(), userId, cancellationToken))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Email already exists");
        }

        if (await _userCommandRepository.ExistsUserByCccdAsync(normalizedCccd, userId, cancellationToken)
            || await _userCommandRepository.ExistsShopInfoByCccdAsync(normalizedCccd, user.ShopId, cancellationToken))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("CCCD already exists");
        }

        user.Email = normalizedEmail;
        user.FullName = normalizedFullName;
        user.Address = normalizedAddress;
        user.Image = resolvedImagePath;
        user.CCCD = normalizedCccd;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(user.ShopId))
        {
            var shopInfo = await _userCommandRepository.GetShopInfoByIdAsync(user.ShopId, cancellationToken);
            if (shopInfo is not null)
            {
                shopInfo.ShopName = normalizedShopName;
                shopInfo.ManagerName = normalizedFullName;
                shopInfo.CCCD = normalizedCccd;
            }
        }

        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        var emailSent = await TrySendAsync(() =>
            _emailNotificationService.SendManagerAccountUpdatedEmailAsync(normalizedEmail, normalizedFullName, normalizedShopName));

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

        if (!user.IsActive)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("User account is already inactive");
        }

        if (user.Role == "Admin")
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Admin accounts cannot be deactivated here");
        }

        if (await _userCommandRepository.HasActiveRentalAreaAsync(user.ShopId, cancellationToken))
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("This user is still renting a mall area and cannot be set inactive");
        }

        var shopInfo = !string.IsNullOrWhiteSpace(user.ShopId)
            ? await _userCommandRepository.GetShopInfoByIdAsync(user.ShopId, cancellationToken)
            : null;

        var emailSent = await TrySendAsync(() =>
            _emailNotificationService.SendManagerAccountDeletedEmailAsync(user.Email, user.FullName, shopInfo?.ShopName));

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.LoginOtpCode = null;
        user.LoginOtpExpiresAt = null;
        await _userCommandRepository.RevokeUserRefreshTokensAsync(userId, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<UserAccountMutationResponseDto>.Ok(new UserAccountMutationResponseDto
        {
            Message = "User account deactivated successfully",
            EmailSent = emailSent
        });
    }

    public async Task<ApplicationResult<UserAccountMutationResponseDto>> ActivateUserAccountAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.NotFound("User does not exist");
        }

        if (user.IsActive)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("User account is already active");
        }

        if (user.Role == "Admin")
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Admin accounts cannot be activated here");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.LoginOtpCode = null;
        user.LoginOtpExpiresAt = null;
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<UserAccountMutationResponseDto>.Ok(new UserAccountMutationResponseDto
        {
            Message = "User account activated successfully",
            EmailSent = false
        });
    }

    public async Task<ApplicationResult<UserAccountMutationResponseDto>> ResendInitialPasswordLinkAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userCommandRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.NotFound("User does not exist");
        }

        if (!user.IsActive)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.Unauthorized("This account is inactive");
        }

        if (user.Role == "Admin")
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("Admin accounts do not use manager password setup links");
        }

        if (!user.MustChangePassword)
        {
            return ApplicationResult<UserAccountMutationResponseDto>.BadRequest("This account has already changed the initial password");
        }

        var oneTimePassword = CreateOtp();
        var oneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword(oneTimePassword);
        var passwordSetupToken = CreateSecureToken();
        var passwordSetupExpiresAt = DateTime.UtcNow.AddHours(24);

        user.Password = oneTimePasswordHash;
        user.OneTimePasswordHash = oneTimePasswordHash;
        user.OneTimePasswordExpiresAt = passwordSetupExpiresAt;
        user.OneTimePasswordUsedAt = null;
        user.PasswordSetupToken = passwordSetupToken;
        user.PasswordSetupTokenExpiresAt = passwordSetupExpiresAt;
        user.UpdatedAt = DateTime.UtcNow;

        await _userCommandRepository.RevokeUserRefreshTokensAsync(userId, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        var emailSent = await TrySendAsync(() =>
            _emailNotificationService.SendManagerInitialPasswordEmailAsync(
                user.Email,
                user.FullName,
                oneTimePassword,
                CreatePasswordSetupUrl(passwordSetupToken)));

        return ApplicationResult<UserAccountMutationResponseDto>.Ok(new UserAccountMutationResponseDto
        {
            Message = "Password setup link resent successfully",
            EmailSent = emailSent
        });
    }

    public async Task<ApplicationResult<MessageResponseDto>> CompleteInitialPasswordChangeAsync(CompleteInitialPasswordChangeDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Token and new password are required");
        }

        var user = await _userCommandRepository.GetUserByPasswordSetupTokenAsync(dto.Token.Trim(), cancellationToken);
        if (user is null)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("Invalid password setup link");
        }

        if (!user.IsActive)
        {
            return ApplicationResult<MessageResponseDto>.Unauthorized("This account is inactive");
        }

        if (!user.MustChangePassword)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This password setup link has already been used");
        }

        if (user.PasswordSetupTokenExpiresAt is null || user.PasswordSetupTokenExpiresAt < DateTime.UtcNow)
        {
            return ApplicationResult<MessageResponseDto>.BadRequest("This password setup link has expired");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.MustChangePassword = false;
        user.OneTimePasswordHash = null;
        user.OneTimePasswordExpiresAt = null;
        user.OneTimePasswordUsedAt = null;
        user.PasswordSetupToken = null;
        user.PasswordSetupTokenExpiresAt = null;
        user.PasswordSetupCompletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userCommandRepository.RevokeUserRefreshTokensAsync(user.Id ?? string.Empty, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);

        return ApplicationResult<MessageResponseDto>.Ok(new MessageResponseDto
        {
            Message = "Password changed successfully"
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

        if (!user.IsActive)
        {
            return ApplicationResult<AccessTokenResponseDto>.Unauthorized("This account is inactive");
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
            || string.IsNullOrWhiteSpace(dto.FullName)
            || string.IsNullOrWhiteSpace(dto.ShopName)
            || string.IsNullOrWhiteSpace(dto.CCCD)
            || string.IsNullOrWhiteSpace(dto.Floor)
            || string.IsNullOrWhiteSpace(dto.LocationSlot)
            || dto.LeaseStartDate is null
            || dto.LeaseTermDays is null
            || dto.ElectricityFee is null
            || dto.WaterFee is null
            || dto.ServiceFee is null)
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Missing required registration information.");
        }
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var normalizedFullName = dto.FullName.Trim();
        var normalizedShopName = dto.ShopName.Trim();
        var normalizedCccd = dto.CCCD.Trim();
        var normalizedFloor = dto.Floor.Trim();
        var normalizedLocationSlot = dto.LocationSlot.Trim();
        var normalizedLeaseStartDate = DateTime.SpecifyKind(dto.LeaseStartDate.Value.Date, DateTimeKind.Utc);
        var normalizedLeaseTermDays = dto.LeaseTermDays.Value;
        var normalizedElectricityFee = dto.ElectricityFee.Value;
        var normalizedWaterFee = dto.WaterFee.Value;
        var normalizedServiceFee = dto.ServiceFee.Value;
        if (normalizedLeaseStartDate < DateTime.UtcNow.Date.AddDays(1))
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Start date must be from tomorrow onward.");
        }
        if (normalizedLeaseTermDays < 30 || normalizedLeaseTermDays % 30 != 0)
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Lease term must be at least 30 days and divisible by 30.");
        }

        if (normalizedElectricityFee <= 0 || normalizedWaterFee <= 0 || normalizedServiceFee <= 0)
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Electricity fee, water fee, and service fee must be greater than 0.");
        }
        if (await _userCommandRepository.ExistsUserByEmailAsync(normalizedEmail, null, cancellationToken))
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("Email already exists.");
        if (await _userCommandRepository.ExistsUserByCccdAsync(normalizedCccd, null, cancellationToken)
            || await _userCommandRepository.ExistsShopInfoByCccdAsync(normalizedCccd, null, cancellationToken))
        {
            return ApplicationResult<RegisterUserResponseDto>.BadRequest("CCCD already exists.");
        }
        var avatarPath = dto.Avatar is not null
            ? await _fileStorageService.SaveProfileAvatarAsync(dto.Avatar, cancellationToken)
            : null;
        var cccdFrontImagePath = dto.CccdFrontImage is not null
            ? await _fileStorageService.SaveCccdImageAsync(dto.CccdFrontImage, cancellationToken)
            : null;
        var cccdBackImagePath = dto.CccdBackImage is not null
            ? await _fileStorageService.SaveCccdImageAsync(dto.CccdBackImage, cancellationToken)
            : null;
        var contractImagePath = dto.ContractImage is not null
            ? await _fileStorageService.SaveContractImageAsync(dto.ContractImage, cancellationToken)
            : null;
        var newShopId = Guid.NewGuid().ToString("N");
        var shopInfo = new ShopInfo
        {
            Id = newShopId,
            ShopName = normalizedShopName,
            Slug = normalizedShopName.ToLowerInvariant().Replace(" ", "-"),
            Category = "Coming Soon",
            Floor = normalizedFloor,
            LocationSlot = normalizedLocationSlot,
            CCCD = normalizedCccd,
            ManagerName = normalizedFullName,
            RentalLocation = normalizedLocationSlot,
            LeaseStartDate = normalizedLeaseStartDate,
            ElectricityUsage = "0 kWh",
            ElectricityFee = normalizedElectricityFee,
            WaterUsage = "0 m3",
            WaterFee = normalizedWaterFee,
            ServiceFee = normalizedServiceFee,
            LeaseTermDays = normalizedLeaseTermDays,
            TotalDue = 0,
            ContractImage = contractImagePath,
            ContractImages = contractImagePath,
            OpeningDate = null,
            CreatedAt = DateTime.UtcNow
        };
        await _userCommandRepository.AddShopInfoAsync(shopInfo, cancellationToken);
        var oneTimePassword = CreateOtp();
        var oneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword(oneTimePassword);
        var passwordSetupToken = CreateSecureToken();
        var passwordSetupExpiresAt = DateTime.UtcNow.AddHours(24);
        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = normalizedEmail,
            Password = oneTimePasswordHash,
            FullName = normalizedFullName,
            Role = "Manager",
            ShopId = newShopId,
            Image = avatarPath,
            CCCD = normalizedCccd,
            CccdFrontImage = cccdFrontImagePath,
            CccdBackImage = cccdBackImagePath,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            MustChangePassword = true,
            OneTimePasswordHash = oneTimePasswordHash,
            OneTimePasswordExpiresAt = passwordSetupExpiresAt,
            OneTimePasswordUsedAt = null,
            PasswordSetupToken = passwordSetupToken,
            PasswordSetupTokenExpiresAt = passwordSetupExpiresAt
        };
        await _userCommandRepository.AddUserAsync(user, cancellationToken);
        await _userCommandRepository.SaveChangesAsync(cancellationToken);
        var emailSent = await TrySendAsync(() =>
            _emailNotificationService.SendManagerInitialPasswordEmailAsync(
                user.Email,
                user.FullName,
                oneTimePassword,
                CreatePasswordSetupUrl(passwordSetupToken)));
        return ApplicationResult<RegisterUserResponseDto>.Ok(new RegisterUserResponseDto
        {
            Message = "User and Shop created successfully",
            EmailSent = emailSent,
            Email = user.Email,
            Role = user.Role,
            ShopId = newShopId,
            CCCD = user.CCCD,
            ShopName = shopInfo.ShopName,
            CreatedAt = user.CreatedAt
        });
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

    private static string CreateSecureToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private string CreatePasswordSetupUrl(string token)
    {
        var frontendBaseUrl = _configuration["EmailSettings:FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            frontendBaseUrl = "http://localhost:5173";
        }

        return $"{frontendBaseUrl.TrimEnd('/')}/change-initial-password?token={Uri.EscapeDataString(token)}";
    }

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

    private static void ClearPasswordSetupState(User user)
    {
        user.MustChangePassword = false;
        user.OneTimePasswordHash = null;
        user.OneTimePasswordExpiresAt = null;
        user.OneTimePasswordUsedAt = null;
        user.PasswordSetupToken = null;
        user.PasswordSetupTokenExpiresAt = null;
        user.PasswordSetupCompletedAt = DateTime.UtcNow;
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

