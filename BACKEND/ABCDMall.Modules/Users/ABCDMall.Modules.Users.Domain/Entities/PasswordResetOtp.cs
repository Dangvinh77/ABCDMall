namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class PasswordResetOtp
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public string UserId { get; set; } = string.Empty;

        public string Otp { get; set; } = string.Empty;

        public string NewPasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime? UsedAt { get; set; }
    }
}
