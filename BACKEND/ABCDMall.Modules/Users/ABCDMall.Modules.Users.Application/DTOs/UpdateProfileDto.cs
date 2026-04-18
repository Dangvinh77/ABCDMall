using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }

        public string? Address { get; set; }

        public string? Image { get; set; }

        public string? CCCD { get; set; }

        public IFormFile? Avatar { get; set; }
    }
}
