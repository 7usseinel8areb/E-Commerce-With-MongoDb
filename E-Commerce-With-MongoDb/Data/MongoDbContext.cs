using E_Commerce_With_MongoDb.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace E_Commerce_With_MongoDb.Data;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(IOptions<MongoDbSettings> mongoDbSettings)
    {
        var settings = mongoDbSettings.Value;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
}
