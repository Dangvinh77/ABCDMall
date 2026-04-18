using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;

public class FoodRepository : IFoodRepository
{
    private readonly FoodCourtDbContext _dbContext;

    public FoodRepository(FoodCourtDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FoodItem>> GetFoodsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FoodItem?> GetFoodBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<FoodItem?> GetFoodByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task CreateFoodAsync(FoodItem item, CancellationToken cancellationToken = default)
    {
        await _dbContext.FoodItems.AddAsync(item, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateFoodAsync(string id, FoodItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.FoodItems.Update(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteFoodAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.FoodItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        _dbContext.FoodItems.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
