namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions
{
    public sealed class EvaluatePromotionRequestDto
    {
        public Guid? PromotionId { get; set; }
        public string PromotionCode { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public int SeatCount { get; set; }
        public string SeatType { get; set; } = string.Empty;
        public DateTimeOffset? ShowtimeAtUtc { get; set; }
        public DateOnly? BusinessDate { get; set; }
        public string PaymentProvider { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public IReadOnlyCollection<EvaluatePromotionComboDto> Combos { get; set; } =
            Array.Empty<EvaluatePromotionComboDto>();
    }
}
