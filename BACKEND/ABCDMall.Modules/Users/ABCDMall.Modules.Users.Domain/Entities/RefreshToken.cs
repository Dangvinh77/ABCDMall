namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class RefreshToken
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        public string UserId { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedAt { get; set; }
    }
}
