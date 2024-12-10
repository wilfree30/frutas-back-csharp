using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Cors;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FruitDb>(opt => opt.UseInMemoryDatabase("FruitList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", 
        builder => builder
            .WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Fruit API",
        Description = "API para administrar una lista de frutas y su estatus de stock",
    });
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("FruitAPI", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://www.example.com");
});

//Build which has all of the functions and properties for creating conections and services usage.
var app = builder.Build();

//Creates a conextion to DB.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<FruitDb>();
    dbContext.Database.EnsureCreated();
}

//Retrieves every fruit in the API
 app.MapGet("/fruitlist",  async (FruitDb db) =>
    await db.Fruits.ToListAsync())
    .WithTags("Get all fruit"); 

//Retrieves every fruit in stock from the API
app.MapGet("/fruitlist/instock", async (FruitDb db) =>
    await db.Fruits.Where(t => t.Instock).ToListAsync())
    .WithTags("Get all fruit that is in stock");

//Retrieves a fruit from the fruit API
app.MapGet("/fruitlist/{id}", async (int id, FruitDb db) =>
    await db.Fruits.FindAsync(id)
        is Fruit fruit
            ? Results.Ok(fruit)
            : Results.NotFound())
    .WithTags("Get fruit by Id");

//Create a new fruit register for the API
app.MapPost("/fruitlist", async (Fruit fruit, FruitDb db) =>
{
    db.Fruits.Add(fruit);
    await db.SaveChangesAsync();

    return Results.Created($"/fruitlist/{fruit.Id}", fruit);
})
    .WithTags("Add fruit to list");

//Update a fruit from the API
app.MapPut("/fruitlist/{id}", async (int id, Fruit inputFruit, FruitDb db) =>
{
    var fruit = await db.Fruits.FindAsync(id);

    if (fruit is null) return Results.NotFound();

    fruit.Name = inputFruit.Name;
    fruit.Instock = inputFruit.Instock;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
    .WithTags("Update fruit by Id");

//Delete a fruit from the API
app.MapDelete("/fruitlist/{id}", async (int id, FruitDb db) =>
{
    if (await db.Fruits.FindAsync(id) is Fruit fruit)
    {
        db.Fruits.Remove(fruit);
        await db.SaveChangesAsync();
        return Results.Ok(fruit);
    }

    return Results.NotFound();
})
    .WithTags("Delete fruit by Id");

//Here must be every "use" function
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngularApp");

//Everything must be before this line
app.Run();