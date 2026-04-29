using System.Text.Json;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public static class BiddingTemplateSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string Serialize(object value)
        => JsonSerializer.Serialize(value, JsonOptions);

    public static ManagerCarouselBidDto ApplyTemplateDetails(ManagerCarouselBidDto dto, CarouselBid bid)
    {
        switch (bid.TemplateType)
        {
            case CarouselBidTemplateType.ShopAd:
                var shopAd = Deserialize<ShopAdTemplateData>(bid.TemplateData);
                dto.ImageUrl = shopAd?.ShopImage;
                dto.Message = shopAd?.Message;
                break;
            case CarouselBidTemplateType.DiscountAd:
                var discountAd = Deserialize<DiscountAdTemplateData>(bid.TemplateData);
                dto.ImageUrl = discountAd?.ProductImage;
                dto.OriginalPrice = discountAd?.OriginalPrice;
                dto.DiscountPrice = discountAd?.DiscountPrice;
                break;
            case CarouselBidTemplateType.EventAd:
                var eventAd = Deserialize<EventAdTemplateData>(bid.TemplateData);
                dto.ImageUrl = eventAd?.EventImage;
                dto.EventStartDate = eventAd?.StartDate;
                dto.StartTime = eventAd?.StartTime;
                break;
        }

        return dto;
    }

    public static PublicCarouselItemDto MapBidToPublicItem(
        CarouselBid bid,
        string? shopName,
        string? shopSlug)
    {
        var dto = new PublicCarouselItemDto
        {
            Id = bid.Id ?? string.Empty,
            SlotType = bid.TemplateType.ToString(),
            TargetMondayDate = bid.TargetMondayDate,
            ShopId = bid.ShopId,
            ShopName = shopName,
            ShopSlug = shopSlug
        };

        switch (bid.TemplateType)
        {
            case CarouselBidTemplateType.ShopAd:
                var shopAd = Deserialize<ShopAdTemplateData>(bid.TemplateData);
                dto.ImageUrl = shopAd?.ShopImage ?? string.Empty;
                dto.Message = shopAd?.Message;
                dto.LinkUrl = string.IsNullOrWhiteSpace(shopSlug) ? "/shops" : $"/shops/{shopSlug}";
                break;
            case CarouselBidTemplateType.DiscountAd:
                var discountAd = Deserialize<DiscountAdTemplateData>(bid.TemplateData);
                dto.ImageUrl = discountAd?.ProductImage ?? string.Empty;
                dto.OriginalPrice = discountAd?.OriginalPrice;
                dto.DiscountPrice = discountAd?.DiscountPrice;
                dto.LinkUrl = string.IsNullOrWhiteSpace(shopSlug) ? "/shops" : $"/shops/{shopSlug}";
                break;
            case CarouselBidTemplateType.EventAd:
                var eventAd = Deserialize<EventAdTemplateData>(bid.TemplateData);
                dto.ImageUrl = eventAd?.EventImage ?? string.Empty;
                dto.EventStartDate = eventAd?.StartDate;
                dto.StartTime = eventAd?.StartTime;
                dto.LinkUrl = "/events";
                break;
        }

        return dto;
    }

    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
