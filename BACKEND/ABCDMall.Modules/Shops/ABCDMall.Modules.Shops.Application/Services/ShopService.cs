using ABCDMall.Modules.Shops.Application.DTOs;
using ABCDMall.Modules.Shops.Application.Interfaces;
using ABCDMall.Modules.Shops.Domain.Interfaces;

namespace ABCDMall.Modules.Shops.Application.Services;

public class ShopService : IShopService
{
    private readonly IShopRepository _repository;

    public ShopService(IShopRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ShopDto>> GetAllShopsAsync()
    {
        var shops = await _repository.GetAllShopsAsync();
        // Map tay từ Entity sang DTO
        return shops.Select(s => new ShopDto
        {
            Id = s.Id,
            Name = s.Name,
            Slug = s.Slug,
            LogoUrl = s.LogoUrl,
            Floor = s.Floor,
            LocationSlot = s.LocationSlot
        });
    }

    public async Task<ShopDto?> GetShopBySlugAsync(string slug)
    {
        var shop = await _repository.GetShopBySlugAsync(slug);
        if (shop == null) return null;

        return new ShopDto
        {
            Id = shop.Id,
            Name = shop.Name,
            Slug = shop.Slug,
            LogoUrl = shop.LogoUrl,
            CoverImageUrl = shop.CoverImageUrl,
            Description = shop.Description,
            Slogan = shop.Slogan,
            Floor = shop.Floor,
            LocationSlot = shop.LocationSlot,
            OpenTime = shop.OpenTime,
            CloseTime = shop.CloseTime,
            Products = shop.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                OldPrice = p.OldPrice,
                DiscountPercent = p.DiscountPercent,
                IsFeatured = p.IsFeatured,
                IsDiscounted = p.IsDiscounted
            }).ToList(),
            Vouchers = shop.Vouchers.Select(v => new VoucherDto
            {
                Id = v.Id,
                Code = v.Code,
                Title = v.Title,
                Description = v.Description,
                ValidUntil = v.ValidUntil
            }).ToList()
        };
    }
}