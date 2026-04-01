using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ABCDMall.Shared.MongoDB
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSetting> settings)
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
