using ABCDMall.Modules.Users.Application.Services.ShopInfos;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure;

public sealed class ShopMonthlyBillReadRepository : IShopMonthlyBillReadRepository
{
    private readonly MallDbContext _context;

    public ShopMonthlyBillReadRepository(MallDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ShopMonthlyBill>> GetBillsAsync(string? shopId, CancellationToken cancellationToken = default)
    {
        var query = _context.ShopMonthlyBills.AsQueryable();

        if (!string.IsNullOrWhiteSpace(shopId))
        {
            query = query.Where(x => x.ShopInfoId == shopId);
        }

        return await query
            .OrderByDescending(x => x.BillingMonthKey)
            .ToListAsync(cancellationToken);
    }
}
