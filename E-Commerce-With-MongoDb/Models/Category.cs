using MongoDB.Bson;

namespace E_Commerce_With_MongoDb.Models;

public class Category
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public List<Property> Properties { get; set; }
}

public class Property
{
    public string Name { get; set; }
    public string Type { get; set; }
}