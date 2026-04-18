using System.ComponentModel.DataAnnotations;

namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? Otp { get; set; }
    }
}
