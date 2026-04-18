namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class UpdateUserAccountDto
    {
        public string Email { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string ShopName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string CCCD { get; set; } = string.Empty;
    }
}
