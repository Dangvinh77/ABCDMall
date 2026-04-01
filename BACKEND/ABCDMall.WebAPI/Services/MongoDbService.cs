using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ABCDMall.WebAPI.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(IOptions<MongoDbSetting> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            var collectionName = typeof(T).Name + "s";
            return _database.GetCollection<T>(collectionName);
        }
    }
}
