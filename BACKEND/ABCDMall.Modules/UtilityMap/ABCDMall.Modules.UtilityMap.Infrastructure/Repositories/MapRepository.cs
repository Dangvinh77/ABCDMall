using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces;
using ABCDMall.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.UtilityMap.Infrastructure.Repositories;

public class MapRepository : IMapRepository
{
    private readonly AppDbContext _context;

    public MapRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdateFloorPlanAsync(int id, string blueprintImageUrl, string description)
    {
        var floor = await _context.FloorPlans.FindAsync(id);
        if (floor == null) return false;

        floor.BlueprintImageUrl = blueprintImageUrl;
        floor.Description = description;
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<FloorPlan>> GetAllFloorsAsync()
    {
        return await _context.FloorPlans
            .Include(f => f.Locations)   // ✅ Load kèm tọa độ shop
            .OrderBy(f => f.FloorLevel)
            .ToListAsync();
    }

    public async Task<FloorPlan?> GetFloorPlanAsync(string floorLevel)
    {
        return await _context.FloorPlans
            .Include(f => f.Locations)
            .FirstOrDefaultAsync(f => f.FloorLevel == floorLevel);
    }

    public async Task CreateFloorPlanAsync(FloorPlan floorPlan)
    {
        _context.FloorPlans.Add(floorPlan);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AddLocationAsync(int floorPlanId, MapLocation location)
    {
        var floor = await _context.FloorPlans.FindAsync(floorPlanId);
        if (floor == null) return false;

        location.FloorPlanId = floorPlanId;
        _context.MapLocations.Add(location);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLocationAsync(int locationId)
    {
        var loc = await _context.MapLocations.FindAsync(locationId);
        if (loc == null) return false;

        _context.MapLocations.Remove(loc);
        await _context.SaveChangesAsync();
        return true;
    }
}