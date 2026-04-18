namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class RequestForgotPasswordOtpDto
    {
        public string Email { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
