namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions
{
    public sealed class PromotionResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? PercentageValue { get; set; }
        public decimal? FlatDiscountValue { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public decimal? MinimumSpendAmount { get; set; }
        public bool IsAutoApplied { get; set; }
    }
}
