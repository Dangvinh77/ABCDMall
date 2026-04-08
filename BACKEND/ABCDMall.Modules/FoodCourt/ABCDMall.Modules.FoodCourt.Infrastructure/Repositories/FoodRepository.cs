using ABCDMall.Modules.FoodCourt.Domain.Interfaces;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using MongoDB.Driver;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;

public class FoodRepository : IFoodRepository
{
    private readonly IMongoCollection<FoodItem> _collection;

    public FoodRepository()
    {
          var client = new MongoClient("mongodb+srv://nguyenkiminh1999_db_user:Kimchicucai1@cluster0.eh1snop.mongodb.net/ABCDMall?retryWrites=true&w=majority");
          var database = client.GetDatabase("ABCDMall");

        _collection = database.GetCollection<FoodItem>("FoodItems");
    }

    public async Task<List<FoodItem>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

//Slug
    public async Task<FoodItem?> GetBySlugAsync(string slug)
    {
        return await _collection
            .Find(x => x.Slug == slug)
            .FirstOrDefaultAsync();
    }

public async Task<FoodItem?> GetByIdAsync(string id)
{
    return await _collection
        .Find(x => x.Id == id)
        .FirstOrDefaultAsync();
}

//Create
    public async Task CreateAsync(FoodItem item)
    {
        await _collection.InsertOneAsync(item);
    }

// Update
    public async Task UpdateAsync(string id, FoodItem item)
    {
    await _collection.ReplaceOneAsync(x => x.Id == id, item);
    }

//Delete
    public async Task DeleteAsync(string id)
    {
    await _collection.DeleteOneAsync(x => x.Id == id);
    }


}