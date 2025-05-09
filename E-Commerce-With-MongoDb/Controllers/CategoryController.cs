using E_Commerce_With_MongoDb.Data;
using E_Commerce_With_MongoDb.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace E_Commerce_With_MongoDb.Controllers;

public class CategoryController(MongoDBContext _context) : Controller
{
    public IActionResult Index()
    {
        var categories = _context.Categories.Find(c => true).ToList();
        return View(categories);
    }
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (ModelState.IsValid)
        {
            _context.Categories.InsertOne(category);
            return RedirectToAction("Index");
        }

        return View(category);
    }
    public IActionResult Edit(string id)
    {
        var category = _context.Categories.Find(c => c.Id == ObjectId.Parse(id)).FirstOrDefault();
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    [HttpPost]
    public IActionResult Edit(Category model)
    {
        if (!ModelState.IsValid) return View(model);

        var filter = Builders<Category>.Filter.Eq(c => c.Id, model.Id);
        var update = Builders<Category>.Update
            .Set(c => c.Name, model.Name)
            .Set(c => c.Properties, model.Properties);

        _context.Categories.UpdateOne(filter, update);

        return RedirectToAction("Index");
    }

    // حذف الكاتيجوري
    [HttpPost]
    public IActionResult Delete(string categoryId)
    {
        if (string.IsNullOrEmpty(categoryId))
        {
            return BadRequest("Category ID is required.");
        }

        var filter = Builders<Category>.Filter.Eq(c => c.Id, ObjectId.Parse(categoryId));
        _context.Categories.DeleteOne(filter);
        return RedirectToAction("Index");
    }

}
