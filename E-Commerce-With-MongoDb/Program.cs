using E_Commerce_With_MongoDb.Data;
using E_Commerce_With_MongoDb.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
// ????? ????? MongoDbSettings ?? ??? ?????????
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

// ????? MongoDBContext ???? ????? ?? IMongoDatabase
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoDbSettings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    var client = new MongoClient(mongoDbSettings.ConnectionString);
    return client.GetDatabase(mongoDbSettings.DatabaseName);
});

// ????? MongoDBContext ?????
builder.Services.AddSingleton<MongoDBContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
