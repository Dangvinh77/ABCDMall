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

        public Task<bool> SendCarouselBidWonEmailAsync(string toEmail, string? fullName, string shopName, decimal bidAmount, DateTime targetMondayDate)
            => SendHtmlEmailAsync(
                toEmail,
                "Your carousel bid won - ABCDMall",
                $"""
                <div style="font-family:Arial,sans-serif;background:#f7f8fc;padding:24px;color:#111827">
                  <div style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:20px;overflow:hidden;border:1px solid #e5e7eb">
                    <div style="background:linear-gradient(135deg,#111827,#1f2937);padding:28px 32px;color:#ffffff">
                      <div style="font-size:12px;letter-spacing:0.18em;text-transform:uppercase;opacity:0.78">ABCD Mall Bidding</div>
                      <h1 style="margin:12px 0 0;font-size:28px;line-height:1.2">Congratulations, your bid won.</h1>
                    </div>
                    <div style="padding:28px 32px">
                      <p>Hello {WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},</p>
                      <p>Your shop <strong>{WebUtility.HtmlEncode(shopName)}</strong> has secured a homepage carousel slot for the week of <strong>{targetMondayDate:dd/MM/yyyy}</strong>.</p>
                      <div style="margin:20px 0;padding:18px;border-radius:16px;background:#f3f4f6">
                        <div><strong>Winning bid:</strong> ${bidAmount:N2}</div>
                        <div><strong>Reminder:</strong> Please complete payment before Sunday so the ad can be published on time.</div>
                      </div>
                      <p>Once payment is completed successfully, your placement will be eligible for Monday publishing.</p>
                      <p style="margin-top:24px">ABCDMall</p>
                    </div>
                  </div>
                </div>
                """);

        public Task<bool> SendCarouselBidLostEmailAsync(string toEmail, string? fullName, string shopName, decimal bidAmount, DateTime targetMondayDate)
            => SendHtmlEmailAsync(
                toEmail,
                "Carousel bidding update - ABCDMall",
                $"""
                <div style="font-family:Arial,sans-serif;background:#f7f8fc;padding:24px;color:#111827">
                  <div style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:20px;overflow:hidden;border:1px solid #e5e7eb">
                    <div style="background:linear-gradient(135deg,#7c2d12,#9a3412);padding:28px 32px;color:#ffffff">
                      <div style="font-size:12px;letter-spacing:0.18em;text-transform:uppercase;opacity:0.82">ABCD Mall Bidding</div>
                      <h1 style="margin:12px 0 0;font-size:28px;line-height:1.2">This week's bid was not selected.</h1>
                    </div>
                    <div style="padding:28px 32px">
                      <p>Hello {WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},</p>
                      <p>We are sorry to let you know that the bid from <strong>{WebUtility.HtmlEncode(shopName)}</strong> for the week of <strong>{targetMondayDate:dd/MM/yyyy}</strong> was not among the final winners.</p>
                      <div style="margin:20px 0;padding:18px;border-radius:16px;background:#fff7ed">
                        <div><strong>Your submitted bid:</strong> ${bidAmount:N2}</div>
                        <div>Thank you for participating in the ABCD Mall homepage carousel bidding round.</div>
                      </div>
                      <p>You can still join the next bidding cycle with a new creative and pricing strategy.</p>
                      <p style="margin-top:24px">ABCDMall</p>
                    </div>
                  </div>
                </div>
                """);

        public Task<bool> SendCarouselBidPaymentSuccessEmailAsync(string toEmail, string? fullName, string shopName, decimal bidAmount, DateTime targetMondayDate)
            => SendHtmlEmailAsync(
                toEmail,
                "Payment successful - ABCDMall carousel bid",
                $"""
                <div style="font-family:Arial,sans-serif;background:#f7f8fc;padding:24px;color:#111827">
                  <div style="max-width:640px;margin:0 auto;background:#ffffff;border-radius:20px;overflow:hidden;border:1px solid #e5e7eb">
                    <div style="background:linear-gradient(135deg,#065f46,#059669);padding:28px 32px;color:#ffffff">
                      <div style="font-size:12px;letter-spacing:0.18em;text-transform:uppercase;opacity:0.82">ABCD Mall Bidding</div>
                      <h1 style="margin:12px 0 0;font-size:28px;line-height:1.2">Payment successful.</h1>
                    </div>
                    <div style="padding:28px 32px">
                      <p>Hello {WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? toEmail : fullName)},</p>
                      <p>We have received the payment for <strong>{WebUtility.HtmlEncode(shopName)}</strong>'s winning carousel bid.</p>
                      <div style="margin:20px 0;padding:18px;border-radius:16px;background:#ecfdf5">
                        <div><strong>Amount paid:</strong> ${bidAmount:N2}</div>
                        <div><strong>Target week:</strong> {targetMondayDate:dd/MM/yyyy}</div>
                      </div>
                      <p>Your ad is now marked as paid and will be included when the admin publishes the weekly carousel.</p>
                      <p style="margin-top:24px">ABCDMall</p>
                    </div>
                  </div>
                </div>
                """);

        public Task<bool> SendEventRegistrationSuccessEmailAsync(string toEmail, string? fullName, string subject, string htmlBody)
            => SendHtmlEmailAsync(toEmail, subject, htmlBody);

        private async Task<bool> SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
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
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

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
