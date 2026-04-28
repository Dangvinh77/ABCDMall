using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Auth;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace ABCDMall.Modules.Users.Application.Services.Auth;

public sealed class DevOtpDebugService : IDevOtpDebugService
{
    private readonly IUserCommandRepository _userCommandRepository;
    private readonly IConfiguration _configuration;

    public DevOtpDebugService(
        IUserCommandRepository userCommandRepository,
        IConfiguration configuration)
    {
        _userCommandRepository = userCommandRepository;
        _configuration = configuration;
    }

    public async Task<ApplicationResult<DebugOtpLookupResponseDto>> GetOtpAsync(
        DebugOtpLookupRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) && string.IsNullOrWhiteSpace(dto.UserId))
        {
            return ApplicationResult<DebugOtpLookupResponseDto>.BadRequest("Email or userId is required.");
        }

        var user = !string.IsNullOrWhiteSpace(dto.UserId)
            ? await _userCommandRepository.GetUserByIdAsync(dto.UserId.Trim(), cancellationToken)
            : await _userCommandRepository.GetUserByNormalizedEmailAsync(dto.Email!.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return ApplicationResult<DebugOtpLookupResponseDto>.NotFound("User does not exist.");
        }

        string? initialPasswordOtp = null;
        var initialPasswordOtpReissued = false;

        if (dto.RegenerateInitialPasswordOtp)
        {
            if (!string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return ApplicationResult<DebugOtpLookupResponseDto>.BadRequest("Only Manager accounts support initial password OTP regeneration.");
            }

            if (!user.MustChangePassword)
            {
                return ApplicationResult<DebugOtpLookupResponseDto>.BadRequest("This account has already completed the initial password change.");
            }

            initialPasswordOtp = CreateOtp();
            var oneTimePasswordHash = BCrypt.Net.BCrypt.HashPassword(initialPasswordOtp);
            var passwordSetupToken = CreateSecureToken();
            var expiresAt = DateTime.UtcNow.AddHours(24);

            user.Password = oneTimePasswordHash;
            user.OneTimePasswordHash = oneTimePasswordHash;
            user.OneTimePasswordExpiresAt = expiresAt;
            user.OneTimePasswordUsedAt = null;
            user.PasswordSetupToken = passwordSetupToken;
            user.PasswordSetupTokenExpiresAt = expiresAt;
            user.UpdatedAt = DateTime.UtcNow;

            await _userCommandRepository.RevokeUserRefreshTokensAsync(user.Id ?? string.Empty, cancellationToken);
            await _userCommandRepository.SaveChangesAsync(cancellationToken);
            initialPasswordOtpReissued = true;
        }

        var forgotPasswordOtp = await _userCommandRepository.GetLatestForgotPasswordOtpByEmailAsync(user.Email.Trim().ToLowerInvariant(), cancellationToken);
        var resetPasswordOtp = await _userCommandRepository.GetLatestPasswordResetOtpByUserIdAsync(user.Id ?? string.Empty, cancellationToken);

        return ApplicationResult<DebugOtpLookupResponseDto>.Ok(new DebugOtpLookupResponseDto
        {
            UserId = user.Id ?? string.Empty,
            Email = user.Email,
            Role = user.Role,
            LoginOtp = user.LoginOtpCode,
            LoginOtpExpiresAt = user.LoginOtpExpiresAt,
            ForgotPasswordOtp = forgotPasswordOtp?.Otp,
            ForgotPasswordOtpExpiresAt = forgotPasswordOtp?.ExpiresAt,
            ResetPasswordOtp = resetPasswordOtp?.Otp,
            ResetPasswordOtpExpiresAt = resetPasswordOtp?.ExpiresAt,
            MustChangePassword = user.MustChangePassword,
            PasswordSetupToken = user.PasswordSetupToken,
            ChangePasswordUrl = string.IsNullOrWhiteSpace(user.PasswordSetupToken) ? null : CreatePasswordSetupUrl(user.PasswordSetupToken),
            InitialPasswordExpiresAt = user.OneTimePasswordExpiresAt ?? user.PasswordSetupTokenExpiresAt,
            InitialPasswordOtp = initialPasswordOtp,
            InitialPasswordOtpReissued = initialPasswordOtpReissued
        });
    }

    private string CreatePasswordSetupUrl(string token)
    {
        var frontendBaseUrl = _configuration["EmailSettings:FrontendBaseUrl"];
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            frontendBaseUrl = "http://localhost:5173";
        }

        return $"{frontendBaseUrl.TrimEnd('/')}/change-initial-password?token={Uri.EscapeDataString(token)}";
    }

    private static string CreateOtp()
        => Random.Shared.Next(100000, 999999).ToString();

    private static string CreateSecureToken()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}
