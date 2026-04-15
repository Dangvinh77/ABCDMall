using ABCDMall.Modules.Shops.Domain.Entities;
using ABCDMall.Modules.Shops.Domain.Interfaces;
using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Shops.Infrastructure.Repositories;

public class ShopRepository : IShopRepository
{
    private readonly AppDbContext _context;

    public ShopRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Shop>> GetAllShopsAsync()
    {
        // Lấy danh sách shop, không cần kéo theo Product để tối ưu tốc độ
        return await _context.Shops.ToListAsync();
    }

    public async Task<Shop?> GetShopBySlugAsync(string slug)
    {
        // Khi xem chi tiết, kéo theo cả Products và Vouchers của shop đó
        return await _context.Shops
            .Include(s => s.Products)
            .Include(s => s.Vouchers)
            .FirstOrDefaultAsync(s => s.Slug.ToLower() == slug.ToLower());
    }
}