using E_Commerce_With_MongoDb.Data;
using E_Commerce_With_MongoDb.Enums;
using E_Commerce_With_MongoDb.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace E_Commerce_With_MongoDb.Controllers
{
    public class ProductController : Controller
    {
        private readonly MongoDBContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(MongoDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories.Find(_ => true).ToListAsync();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormCollection form)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    product.ProductProperties = await ProcessDynamicProperties(product.CategoryId, form);

                    await _context.Products.InsertOneAsync(product);
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                Console.WriteLine($"Error creating product: {ex}");
            }

            ViewBag.Categories = await _context.Categories.Find(_ => true).ToListAsync();
            return View(product);
        }

        private async Task<Dictionary<string, string>> ProcessDynamicProperties(ObjectId categoryId, IFormCollection form)
        {
            var properties = new Dictionary<string, string>();
            var category = await _context.Categories.Find(c => c.Id == categoryId).FirstOrDefaultAsync();

            if (category?.Properties != null)
            {
                foreach (var prop in category.Properties)
                {
                    try
                    {
                        var formValue = form[$"Properties[{prop.Name}]"];
                        var file = Request.Form.Files[$"Properties[{prop.Name}]"];

                        if (file != null && file.Length > 0 && (prop.Type == PropertyType.Image || prop.Type == PropertyType.File))
                        {
                            properties[prop.Name] = await SaveUploadedFile(file);
                        }
                        else if (!string.IsNullOrEmpty(formValue))
                        {
                            properties[prop.Name] = ConvertPropertyValue(prop.Type, formValue).ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError($"Properties[{prop.Name}]", $"Invalid {prop.Type} value: {ex.Message}");
                        Console.WriteLine($"Error processing property {prop.Name}: {ex}");
                    }
                }
            }

            return properties;
        }

        private object ConvertPropertyValue(PropertyType type, string value)
        {
            return type switch
            {
                PropertyType.Int => int.Parse(value),
                PropertyType.Float => double.Parse(value),
                PropertyType.Bool => bool.Parse(value),
                PropertyType.Date => DateTime.Parse(value),
                _ => value
            };
        }

        private async Task<string> SaveUploadedFile(IFormFile file)
        {
            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.Find(_ => true).ToListAsync();
            var categories = await _context.Categories.Find(_ => true).ToListAsync();

            ViewBag.CategoryNames = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);
            return View(products);
        }

        [HttpGet]
        [Route("Product/GetCategoryProperties")]
        public IActionResult GetCategoryProperties(string categoryId)
        {
            Console.WriteLine($"Request received for category ID: {categoryId}");

            if (!ObjectId.TryParse(categoryId, out var objectId))
            {
                Console.WriteLine("Invalid category ID format");
                return BadRequest("Invalid category ID");
            }

            var category = _context.Categories.Find(c => c.Id == objectId).FirstOrDefault();

            if (category == null)
            {
                Console.WriteLine("Category not found");
                return NotFound("Category not found");
            }

            Console.WriteLine($"Found {category.Properties?.Count} properties");

            var result = category.Properties?.Select(p => new
            {
                name = p.Name,
                type = p.Type.ToString()
            }).ToList();

            return Ok(result);
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id) || !ObjectId.TryParse(id, out var objectId))
                return NotFound();

            var product = await _context.Products.Find(p => p.Id == objectId).FirstOrDefaultAsync();
            if (product == null)
                return NotFound();

            var categories = await _context.Categories.Find(_ => true).ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.ExistingPropertiesJson = JsonConvert.SerializeObject(product.ProductProperties ?? new Dictionary<string, string>());

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product product, IFormCollection form)
        {
            if (!ObjectId.TryParse(id, out var objectId) || objectId != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    product.ProductProperties = await ProcessDynamicProperties(product.CategoryId, form);

                    var result = await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product);
                    if (result.ModifiedCount == 0)
                        return NotFound();

                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return StatusCode(500, "Error occurred while updating the product");
                }
            }

            var categories = await _context.Categories.Find(_ => true).ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.ExistingPropertiesJson = JsonConvert.SerializeObject(product.ProductProperties ?? new Dictionary<string, string>());

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                return BadRequest("Invalid Product ID");
            }

            var product = await _context.Products.Find(p => p.Id == objectId).FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }

            await _context.Products.DeleteOneAsync(p => p.Id == objectId);
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

    }
}