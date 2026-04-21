using ABCDMall.Modules.UtilityMap.Application.Services.Maps;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Infrastructure.Persistence.UtilityMap;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Repositories;

public class MapRepository : IMapRepository
{
    private readonly UtilityMapDbContext _context;

    public MapRepository(UtilityMapDbContext context)
    {
        _context = context;
    }

    // ── Queries ──────────────────────────────────────────────────────────

    public async Task<List<FloorPlan>> GetAllFloorsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FloorPlans
            .Include(f => f.Locations)
            .OrderBy(f => f.FloorLevel)
            .ToListAsync(cancellationToken);
    }

    public async Task<FloorPlan?> GetFloorPlanAsync(string floorLevel, CancellationToken cancellationToken = default)
    {
        return await _context.FloorPlans
            .Include(f => f.Locations)
            .FirstOrDefaultAsync(f => f.FloorLevel == floorLevel, cancellationToken);
    }

    public async Task<FloorPlan?> GetFloorPlanByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FloorPlans
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<MapLocation?> GetLocationByIdAsync(int locationId, CancellationToken cancellationToken = default)
    {
        return await _context.MapLocations
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);
    }

    public async Task<MapLocation?> GetLocationByShopInfoIdAsync(string shopInfoId, CancellationToken cancellationToken = default)
    {
        return await _context.MapLocations
            .FirstOrDefaultAsync(l => l.ShopInfoId == shopInfoId, cancellationToken);
    }

    // ── Commands ─────────────────────────────────────────────────────────

    public async Task CreateFloorPlanAsync(FloorPlan floorPlan, CancellationToken cancellationToken = default)
    {
        _context.FloorPlans.Add(floorPlan);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateFloorPlanAsync(int id, FloorPlan floorPlan, CancellationToken cancellationToken = default)
    {
        _context.FloorPlans.Update(floorPlan);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AddLocationAsync(int floorPlanId, MapLocation location, CancellationToken cancellationToken = default)
    {
        var floor = await _context.FloorPlans.FindAsync(new object[] { floorPlanId }, cancellationToken);
        if (floor is null) return false;

        location.FloorPlanId = floorPlanId;
        _context.MapLocations.Add(location);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteLocationAsync(int locationId, CancellationToken cancellationToken = default)
    {
        var loc = await _context.MapLocations.FindAsync(new object[] { locationId }, cancellationToken);
        if (loc is null) return false;

        _context.MapLocations.Remove(loc);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateLocationSlotAsync(
        int locationId,
        string status,
        string? shopInfoId,
        CancellationToken cancellationToken = default)
    {
        var loc = await _context.MapLocations
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);

        if (loc is null) return false;

        loc.Status = status;
        loc.ShopInfoId = shopInfoId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateLocationStatusByShopInfoIdAsync(
        string shopInfoId,
        string status,
        CancellationToken cancellationToken = default)
    {
        var loc = await _context.MapLocations
            .FirstOrDefaultAsync(l => l.ShopInfoId == shopInfoId, cancellationToken);

        if (loc is null) return false;

        loc.Status = status;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}