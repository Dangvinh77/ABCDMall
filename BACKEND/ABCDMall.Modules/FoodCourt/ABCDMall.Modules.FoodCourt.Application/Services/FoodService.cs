using ABCDMall.Shared.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Application.Helpers;
using ABCDMall.Modules.FoodCourt.Domain.Interfaces;       
using ABCDMall.Modules.FoodCourt.Application.Interfaces;

namespace ABCDMall.Modules.FoodCourt.Application.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _repo;

    public FoodService(IFoodRepository repo)
    {
        _repo = repo;
    }

    // ================= GET ALL =================
    public async Task<List<FoodItemDto>> GetAllAsync()
    {
        var data = await _repo.GetAllAsync();

        return data.Select(Map).ToList();
    }

    // ================= GET BY ID =================
    public async Task<FoodItemDto?> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return null;

        return Map(item);
    }

    // ================= CREATE =================
    public async Task CreateAsync(FoodItem item)
    {
        if (string.IsNullOrEmpty(item.Name))
            throw new Exception("Name is required");

        item.Slug = SlugHelper.GenerateSlug(item.Name);

        await _repo.CreateAsync(item);
    }

    // ================= GET BY SLUG =================
    public async Task<FoodItemDto?> GetBySlugAsync(string slug)
    {
        var item = await _repo.GetBySlugAsync(slug);
        if (item == null) return null;

        return Map(item);
    }

    // ================= UPDATE =================
    public async Task<bool> UpdateAsync(int id, FoodItemDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Name = dto.Name;
        existing.ImageUrl = dto.ImageUrl;
        existing.Description = dto.Description ?? string.Empty;

        // 🔥 FIX CHÍNH: luôn update slug theo name mới
        // existing.Slug = SlugHelper.GenerateSlug(dto.Name);

        existing.Slug = string.IsNullOrEmpty(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : SlugHelper.GenerateSlug(dto.Slug);

        await _repo.UpdateAsync(id, existing);
        return true;
    }

    // ================= DELETE =================
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return false;

        await _repo.DeleteAsync(id);
        return true;
    }

    // ================= SEARCH =================
    public async Task<List<FoodItemDto>> SearchAsync(string keyword)
    {
        var data = await _repo.GetAllAsync();

        keyword = keyword?.Trim().ToLower() ?? "";

        return data
            .Where(x => !string.IsNullOrEmpty(x.Name) &&
                        x.Name.ToLower().Contains(keyword))
            .Select(Map)
            .ToList();
    }

    // ================= MAPPER =================
    private static FoodItemDto Map(FoodItem x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        ImageUrl = x.ImageUrl,
        Slug = x.Slug,
        Description = x.Description
    };
}