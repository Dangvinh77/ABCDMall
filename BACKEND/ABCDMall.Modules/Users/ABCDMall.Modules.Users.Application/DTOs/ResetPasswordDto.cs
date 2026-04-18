namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class ResetPasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
