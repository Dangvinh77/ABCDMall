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

    public async Task<FloorPlan?> GetFloorPlanAsync(string floorLevel)
    {
        return await _context.FloorPlans
            .FirstOrDefaultAsync(x => x.FloorLevel == floorLevel);
    }

    public async Task<List<FloorPlan>> GetAllFloorsAsync()
    {
        return await _context.FloorPlans.ToListAsync();
    }

    public async Task CreateFloorPlanAsync(FloorPlan floorPlan)
    {
        _context.FloorPlans.Add(floorPlan);
        await _context.SaveChangesAsync();
    }
}