namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class ConfirmForgotPasswordOtpDto
    {
        public string Email { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;
    }
}
