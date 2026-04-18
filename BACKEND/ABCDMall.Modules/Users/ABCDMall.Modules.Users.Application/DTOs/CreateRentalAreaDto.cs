namespace ABCDMall.Modules.Users.Application.DTOs
{
    public class CreateRentalAreaDto
    {
        public string AreaCode { get; set; } = string.Empty;
        public string Floor { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal MonthlyRent { get; set; }
    }
}
