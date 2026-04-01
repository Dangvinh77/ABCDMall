using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ABCDMall.Shared.MongoDB
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        public MongoContext(IConfiguration config)
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
        // Hàm xác nhận kết nối
        public async Task<bool> CheckConnection()
        {
            try
            {
                // Lệnh Ping gửi tới MongoDB
                await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
