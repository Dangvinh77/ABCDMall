using System.ComponentModel.DataAnnotations;

namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class User
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? ShopId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? Image { get; set; }

        public string? Address { get; set; }

        public string? CCCD { get; set; }

        public bool IsActive { get; set; } = true;

        public int FailedLoginAttempts { get; set; }

        public string? LoginOtpCode { get; set; }

        public DateTime? LoginOtpExpiresAt { get; set; }
    }
}
