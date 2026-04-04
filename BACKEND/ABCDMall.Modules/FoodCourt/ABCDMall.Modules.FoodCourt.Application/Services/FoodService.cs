using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.Interfaces;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Application.Helpers;

namespace ABCDMall.Modules.FoodCourt.Application.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _repo;

    public FoodService(IFoodRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<FoodItemDto>> GetAllAsync()
    {
        var data = await _repo.GetAllAsync();

        return data.Select(x => new FoodItemDto
        {
            Id = x.Id,
            Name = x.Name,
            //Price = x.Price,
            ImageUrl = x.ImageUrl,
            Slug = x.Slug,
            Description = x.Description
        }).ToList();
    }


    public async Task CreateAsync(FoodItem item)
    {
        item.Slug = SlugHelper.GenerateSlug(item.Name);
        await _repo.CreateAsync(item);
        
    }

    public async Task<FoodItemDto?> GetBySlugAsync(string slug)
{
    var item = await _repo.GetBySlugAsync(slug);

    if (item == null) return null;

    return new FoodItemDto
    {
        Id = item.Id,
        Name = item.Name,
        Slug = item.Slug,
        ImageUrl = item.ImageUrl,
        Description = item.Description
    };
}
}