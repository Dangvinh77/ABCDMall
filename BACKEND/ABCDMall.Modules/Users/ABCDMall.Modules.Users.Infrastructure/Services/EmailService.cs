using System.Net;
using System.Net.Mail;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace ABCDMall.Modules.Users.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendManagerRegistrationSuccessEmailAsync(string toEmail, string? fullName)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Account registration successful - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

Your account has been successfully registered in the ABCDMall system.

Login email: {toEmail}
Role: Manager

Please sign in to start using the system.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendManagerAccountUpdatedEmailAsync(string toEmail, string? fullName, string? shopName)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Account information updated - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

Your manager account information has been updated by an ABCDMall administrator.

Login email: {toEmail}
Shop name: {(string.IsNullOrWhiteSpace(shopName) ? "N/A" : shopName)}

If you did not expect this change, please contact the mall administrator.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendManagerAccountDeletedEmailAsync(string toEmail, string? fullName, string? shopName)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Account deleted - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

Your manager account in the ABCDMall system has been deleted by an administrator.

Login email: {toEmail}
Shop name: {(string.IsNullOrWhiteSpace(shopName) ? "N/A" : shopName)}

If you believe this was a mistake, please contact the mall administrator.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendResetPasswordOtpEmailAsync(string toEmail, string? fullName, string otp)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Password reset OTP - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

Your password reset OTP is: {otp}

This code is valid for 5 minutes.
If you did not make this request, please ignore this email.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendForgotPasswordOtpEmailAsync(string toEmail, string? fullName, string otp)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Forgot password OTP - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

You requested a password reset in the ABCDMall system.
Your OTP is: {otp}

This code is valid for 5 minutes.
If you did not make this request, please ignore this email.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendLoginOtpEmailAsync(string toEmail, string? fullName, string otp)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Login OTP - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

The system detected multiple consecutive failed sign-in attempts.
Your OTP to continue signing in is: {otp}

This code is valid for 5 minutes.
If you did not make this request, please ignore this email.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendManagerInitialPasswordEmailAsync(
            string toEmail,
            string? fullName,
            string oneTimePassword,
            string changePasswordUrl)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = "Your ABCDMall manager account password";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

Your ABCDMall manager account has been created.

Login email: {toEmail}
One-time password: {oneTimePassword}

Please change your password using this link:
{changePasswordUrl}

This link and one-time password are valid for 24 hours. The one-time password can only be used once.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }

        public async Task<bool> SendRentalBillUpdatedEmailAsync(
            string toEmail,
            string? fullName,
            string shopName,
            string billingMonth,
            decimal totalDue)
        {
            var settings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

            if (settings == null ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                return false;
            }

            using var message = new MailMessage();
            message.From = new MailAddress(settings.FromEmail, settings.FromName);
            message.To.Add(toEmail);
            message.Subject = $"Rental bill updated - {billingMonth} - ABCDMall";
            message.Body =
$@"Hello {(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},

Your monthly rental bill has been updated in the ABCDMall system.

Shop: {shopName}
Billing month: {billingMonth}
Total due: {totalDue:N0} VND
Status: Unpaid

Please sign in to your manager dashboard and complete the online payment.

ABCDMall";
            message.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                EnableSsl = settings.EnableSsl
            };

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            }
            else
            {
                client.UseDefaultCredentials = true;
            }

            await client.SendMailAsync(message);
            return true;
        }
    }
}
