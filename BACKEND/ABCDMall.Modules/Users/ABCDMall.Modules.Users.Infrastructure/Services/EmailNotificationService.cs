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

    public Task<bool> SendCarouselBidWonEmailAsync(string toEmail, string? fullName, string shopName, decimal bidAmount, DateTime targetMondayDate)
        => _emailService.SendCarouselBidWonEmailAsync(toEmail, fullName, shopName, bidAmount, targetMondayDate);

    public Task<bool> SendCarouselBidLostEmailAsync(string toEmail, string? fullName, string shopName, decimal bidAmount, DateTime targetMondayDate)
        => _emailService.SendCarouselBidLostEmailAsync(toEmail, fullName, shopName, bidAmount, targetMondayDate);

    public Task<bool> SendCarouselBidPaymentSuccessEmailAsync(string toEmail, string? fullName, string shopName, decimal bidAmount, DateTime targetMondayDate)
        => _emailService.SendCarouselBidPaymentSuccessEmailAsync(toEmail, fullName, shopName, bidAmount, targetMondayDate);

    public Task<bool> SendEventRegistrationSuccessEmailAsync(string toEmail, string? fullName, string subject, string htmlBody)
        => _emailService.SendEventRegistrationSuccessEmailAsync(toEmail, fullName, subject, htmlBody);
}
