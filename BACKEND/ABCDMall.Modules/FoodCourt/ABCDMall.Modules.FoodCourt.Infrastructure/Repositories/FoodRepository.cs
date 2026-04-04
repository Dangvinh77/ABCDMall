// using ABCDMall.Modules.FoodCourt.Application.Interfaces;
// using ABCDMall.Modules.FoodCourt.Domain.Entities;
// using MongoDB.Driver;

// namespace ABCDMall.Modules.FoodCourt.Infrastructure.Repositories;

// public class FoodRepository : IFoodRepository
// {
//     private readonly IMongoCollection<FoodItem> _collection;

//     public FoodRepository()
//     {
//         var client = new MongoClient("mongodb+srv://nguyenkiminh1999_db_user:Kimchicucai1@cluster0.eh1snop.mongodb.net/ABCDMall?retryWrites=true&w=majority");
//         var database = client.GetDatabase("ABCDMall");

//         _collection = database.GetCollection<FoodItem>("FoodItems");
//     }

//     public async Task<List<FoodItem>> GetAllAsync()
//     {
//         return await _collection.Find(_ => true).ToListAsync();
//     }
    

//     public async Task CreateAsync(FoodItem item)
//     {
//         await _collection.InsertOneAsync(item);
//     }
// }

using ABCDMall.Modules.FoodCourt.Application.Interfaces;
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

    public async Task<FoodItem?> GetBySlugAsync(string slug)
    {
        return await _collection
            .Find(x => x.Slug == slug)
            .FirstOrDefaultAsync();
    }

    public async Task CreateAsync(FoodItem item)
    {
        await _collection.InsertOneAsync(item);
    }
}