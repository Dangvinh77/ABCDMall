using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class RegisterTenantDto
    {
        public string CCCD { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public decimal ElectricityFee { get; set; }
        public decimal WaterFee { get; set; }
        public decimal ServiceFee { get; set; }
        public int LeaseTermDays { get; set; }
        public IFormFile? ContractImage { get; set; }
    }
}
