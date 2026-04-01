using MongoDB.Driver;

namespace ABCDMall.WebAPI.Services
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService(IConfiguration config)
        {
            var connection = config["ConnectDB:ConnectionString"];
            var databaseName = config["ConnectDB:DatabaseName"];

            var client = new MongoClient(connection);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }
    }
}
