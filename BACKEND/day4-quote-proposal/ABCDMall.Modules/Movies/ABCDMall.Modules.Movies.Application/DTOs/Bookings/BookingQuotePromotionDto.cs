namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingQuotePromotionDto
{
    public Guid? PromotionId { get; set; }
    public string PromotionCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsEligible { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
}
