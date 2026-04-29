using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using Microsoft.EntityFrameworkCore;
using UsersInfrastructure = ABCDMall.Modules.Users.Infrastructure;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;

public class FoodRepository : IFoodRepository
{
    private readonly FoodCourtDbContext _dbContext;
    private readonly UsersInfrastructure.MallDbContext _mallDbContext;

    public FoodRepository(
        FoodCourtDbContext dbContext,
        UsersInfrastructure.MallDbContext mallDbContext)
    {
        _dbContext = dbContext;
        _mallDbContext = mallDbContext;
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

    public async Task<FoodItem?> GetFoodDetailByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .AsNoTracking()
            .Include(x => x.MenuItems.OrderBy(menuItem => menuItem.DisplayOrder))
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<FoodItem?> GetFoodDetailBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .AsNoTracking()
            .Include(x => x.MenuItems.OrderBy(menuItem => menuItem.DisplayOrder))
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<FoodItem>> GetManagedFoodStallsAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .AsNoTracking()
            .Where(x => x.OwnerShopId == ownerShopId)
            .Include(x => x.MenuItems.OrderBy(menuItem => menuItem.DisplayOrder))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<FoodItem?> GetManagedFoodStallByIdAsync(string ownerShopId, string id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodItems
            .Include(x => x.MenuItems.OrderBy(menuItem => menuItem.DisplayOrder))
            .FirstOrDefaultAsync(x => x.OwnerShopId == ownerShopId && x.Id == id, cancellationToken);
    }

    public Task<int> CountFoodCourtRentalsAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        return _mallDbContext.RentalAreas
            .AsNoTracking()
            .Where(x => x.ShopInfoId == ownerShopId && x.Status == "Rented" && x.BusinessType == "FoodCourt")
            .CountAsync(cancellationToken);
    }

    public Task<int> CountManagedStallsAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        return _dbContext.FoodItems
            .AsNoTracking()
            .Where(x => x.OwnerShopId == ownerShopId)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableFoodCourtLocationDto>> GetAvailableFoodCourtLocationsAsync(string ownerShopId, CancellationToken cancellationToken = default)
    {
        var usedLocations = await _dbContext.FoodItems
            .AsNoTracking()
            .Where(x => x.OwnerShopId == ownerShopId)
            .Select(x => x.Location)
            .ToListAsync(cancellationToken);

        return await _mallDbContext.RentalAreas
            .AsNoTracking()
            .Where(x => x.ShopInfoId == ownerShopId && x.Status == "Rented" && x.BusinessType == "FoodCourt" && !usedLocations.Contains(x.AreaCode))
            .OrderBy(x => x.AreaCode)
            .Select(x => new AvailableFoodCourtLocationDto
            {
                RentalAreaId = x.Id ?? string.Empty,
                LocationSlot = x.AreaCode,
                Floor = x.Floor,
                AreaName = x.AreaName
            })
            .ToListAsync(cancellationToken);
    }

    public Task<bool> SlugExistsAsync(string slug, string? excludingFoodId = null, CancellationToken cancellationToken = default)
    {
        return _dbContext.FoodItems
            .AsNoTracking()
            .AnyAsync(x => x.Slug == slug && x.Id != excludingFoodId, cancellationToken);
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
