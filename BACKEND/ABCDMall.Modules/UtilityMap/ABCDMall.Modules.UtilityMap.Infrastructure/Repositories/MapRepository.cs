using ABCDMall.Shared.MongoDB;
using ABCDMall.Modules.UtilityMap.Domain.Entities;
using ABCDMall.Modules.UtilityMap.Domain.Interfaces; // Phải using cái này để thấy IMapRepository
using MongoDB.Driver;

// THAY ĐỔI: Namespace phải trỏ về Infrastructure.Repositories
namespace ABCDMall.Modules.UtilityMap.Infrastructure.Repositories;

public class MapRepository : IMapRepository
{
    private readonly IMongoCollection<FloorPlan> _floorPlans;

    public MapRepository(MongoContext context) 
    {
        _floorPlans = context.GetCollection<FloorPlan>("FloorPlans");
    }

    public async Task<FloorPlan?> GetFloorPlanAsync(string floorLevel)
    {
        return await _floorPlans.Find(x => x.FloorLevel == floorLevel).FirstOrDefaultAsync();
    }

    public async Task<List<FloorPlan>> GetAllFloorsAsync()
    {
        return await _floorPlans.Find(_ => true).ToListAsync();
    }

    public async Task CreateFloorPlanAsync(FloorPlan floorPlan)
    {
        await _floorPlans.InsertOneAsync(floorPlan);
    }
}