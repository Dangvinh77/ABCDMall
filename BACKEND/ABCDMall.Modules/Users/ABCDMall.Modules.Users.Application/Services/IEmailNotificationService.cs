namespace ABCDMall.Modules.Users.Application.Services;

public interface IEmailNotificationService
{
    Task<bool> SendManagerRegistrationSuccessEmailAsync(string toEmail, string? fullName);

    Task<bool> SendManagerInitialPasswordEmailAsync(
        string toEmail,
        string? fullName,
        string oneTimePassword,
        string changePasswordUrl);

    Task<bool> SendManagerAccountUpdatedEmailAsync(string toEmail, string? fullName, string? shopName);

    Task<bool> SendManagerAccountDeletedEmailAsync(string toEmail, string? fullName, string? shopName);

    Task<bool> SendResetPasswordOtpEmailAsync(string toEmail, string? fullName, string otp);

    Task<bool> SendForgotPasswordOtpEmailAsync(string toEmail, string? fullName, string otp);

    Task<bool> SendLoginOtpEmailAsync(string toEmail, string? fullName, string otp);
}
