using Microsoft.AspNetCore.Http;

namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class SubmitCarouselBidRequestDto
{
    public decimal BidAmount { get; set; }

    public string TemplateType { get; set; } = string.Empty;

    public IFormFile? ShopImageFile { get; set; }

    public string? Message { get; set; }

    public IFormFile? ProductImageFile { get; set; }

    public decimal? OriginalPrice { get; set; }

    public decimal? DiscountPrice { get; set; }

    public IFormFile? EventImageFile { get; set; }

    public DateTime? StartDate { get; set; }

    public string? StartTime { get; set; }
}
