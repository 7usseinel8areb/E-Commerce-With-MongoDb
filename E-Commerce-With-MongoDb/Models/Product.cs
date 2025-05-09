using MongoDB.Bson;

namespace E_Commerce_With_MongoDb.Models;

public class Product
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public double Price { get; set; }
    public ObjectId CategoryId { get; set; }
    public Dictionary<string, string> ProductProperties { get; set; }
}
