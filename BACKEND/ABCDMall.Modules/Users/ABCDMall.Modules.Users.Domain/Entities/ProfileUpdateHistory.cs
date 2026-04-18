namespace ABCDMall.Modules.Users.Domain.Entities
{
    public class ProfileUpdateHistory
    {
        public string? Id { get; set; } = Guid.NewGuid().ToString("N");

        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PreviousFullName { get; set; }

        public string? PreviousAddress { get; set; }

        public string? PreviousImage { get; set; }

        public string? PreviousCCCD { get; set; }

        public string? UpdatedFullName { get; set; }

        public string? UpdatedAddress { get; set; }

        public string? UpdatedImage { get; set; }

        public string? UpdatedCCCD { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
