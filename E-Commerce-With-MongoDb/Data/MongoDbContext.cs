using E_Commerce_With_MongoDb.Models;
using MongoDB.Driver;

namespace E_Commerce_With_MongoDb.Data;

public class MongoDBContext
{
    private readonly IMongoDatabase _database;

    public MongoDBContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
}
