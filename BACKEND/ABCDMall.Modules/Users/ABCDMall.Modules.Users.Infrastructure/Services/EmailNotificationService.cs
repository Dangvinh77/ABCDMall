using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Services;

namespace ABCDMall.Modules.Users.Infrastructure.Services;

public sealed class EmailNotificationService : IEmailNotificationService
{
    private readonly EmailService _emailService;

    public EmailNotificationService(EmailService emailService)
    {
        _emailService = emailService;
    }

    public Task<bool> SendManagerRegistrationSuccessEmailAsync(string toEmail, string? fullName)
        => _emailService.SendManagerRegistrationSuccessEmailAsync(toEmail, fullName);

    public Task<bool> SendManagerInitialPasswordEmailAsync(
        string toEmail,
        string? fullName,
        string oneTimePassword,
        string changePasswordUrl)
        => _emailService.SendManagerInitialPasswordEmailAsync(toEmail, fullName, oneTimePassword, changePasswordUrl);

    public Task<bool> SendManagerAccountUpdatedEmailAsync(string toEmail, string? fullName, string? shopName)
        => _emailService.SendManagerAccountUpdatedEmailAsync(toEmail, fullName, shopName);

    public Task<bool> SendManagerAccountDeletedEmailAsync(string toEmail, string? fullName, string? shopName)
        => _emailService.SendManagerAccountDeletedEmailAsync(toEmail, fullName, shopName);

    public Task<bool> SendResetPasswordOtpEmailAsync(string toEmail, string? fullName, string otp)
        => _emailService.SendResetPasswordOtpEmailAsync(toEmail, fullName, otp);

    public Task<bool> SendForgotPasswordOtpEmailAsync(string toEmail, string? fullName, string otp)
        => _emailService.SendForgotPasswordOtpEmailAsync(toEmail, fullName, otp);

    public Task<bool> SendLoginOtpEmailAsync(string toEmail, string? fullName, string otp)
        => _emailService.SendLoginOtpEmailAsync(toEmail, fullName, otp);

    public Task<bool> SendEventRegistrationSuccessEmailAsync(string toEmail, string? fullName, string subject, string body)
        => _emailService.SendEventRegistrationSuccessEmailAsync(toEmail, fullName, subject, body);

    public Task<bool> SendRentalBillUpdatedEmailAsync(
        string toEmail,
        string? fullName,
        string shopName,
        string billingMonth,
        decimal totalDue)
        => _emailService.SendRentalBillUpdatedEmailAsync(toEmail, fullName, shopName, billingMonth, totalDue);

    public Task<bool> SendCarouselBidWonEmailAsync(string toEmail, string? fullName, string? shopName, decimal amount, DateTime targetMondayDate)
        => _emailService.SendCarouselBidWonEmailAsync(toEmail, fullName, shopName, amount, targetMondayDate);

    public Task<bool> SendCarouselBidLostEmailAsync(string toEmail, string? fullName, string? shopName, decimal amount, DateTime targetMondayDate)
        => _emailService.SendCarouselBidLostEmailAsync(toEmail, fullName, shopName, amount, targetMondayDate);

    public Task<bool> SendCarouselBidPaymentSuccessEmailAsync(string toEmail, string? fullName, string? shopName, decimal amount, DateTime targetMondayDate)
        => _emailService.SendCarouselBidPaymentSuccessEmailAsync(toEmail, fullName, shopName, amount, targetMondayDate);
}
