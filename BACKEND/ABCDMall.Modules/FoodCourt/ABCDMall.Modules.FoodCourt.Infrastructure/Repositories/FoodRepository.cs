using ABCDMall.Modules.FoodCourt.Domain.Interfaces;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;

public class FoodRepository : IFoodRepository
{
    private readonly AppDbContext _context;

    public FoodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FoodItem>> GetAllAsync()
    {
        return await _context.FoodItems.ToListAsync();
    }

    public async Task<FoodItem?> GetByIdAsync(int id)
    {
        return await _context.FoodItems.FindAsync(id);
    }

    public async Task<FoodItem?> GetBySlugAsync(string slug)
    {
        return await _context.FoodItems
            .FirstOrDefaultAsync(x => x.Slug == slug);
    }

    public async Task CreateAsync(FoodItem item)
    {
        _context.FoodItems.Add(item);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, FoodItem item)
    {
        var existing = await _context.FoodItems.FindAsync(id);
        if (existing == null) return;

        existing.Name = item.Name;
        existing.ImageUrl = item.ImageUrl;
        existing.Slug = item.Slug;
        existing.Description = item.Description;
        existing.MallSlug = item.MallSlug;
        existing.CategorySlug = item.CategorySlug;
        

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _context.FoodItems.FindAsync(id);
        if (item != null)
        {
            _context.FoodItems.Remove(item);
            await _context.SaveChangesAsync();
        }
    }
}