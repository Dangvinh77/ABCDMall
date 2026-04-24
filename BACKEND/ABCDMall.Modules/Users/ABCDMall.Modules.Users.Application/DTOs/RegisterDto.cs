using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? Role { get; set; }

        public string ShopName { get; set; } = string.Empty;

        public string CCCD { get; set; } = string.Empty;

        public string? Floor { get; set; }

        public string? LocationSlot { get; set; }

        public DateTime? LeaseStartDate { get; set; }

        public int? LeaseTermDays { get; set; }

        public decimal? ElectricityFee { get; set; }

        public decimal? WaterFee { get; set; }

        public decimal? ServiceFee { get; set; }

        public IFormFile? Avatar { get; set; }

        public IFormFile? CccdFrontImage { get; set; }

        public IFormFile? CccdBackImage { get; set; }

        public IFormFile? ContractImage { get; set; }

        public int? MapLocationId { get; set; }
    }
}

