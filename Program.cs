using LogiTrack.Model;

var sampleItem = new InventoryItem
{
  ItemId = 1,
  Name = "Pallet Jack",
  Quantity = 12,
  Location = "Warehouse A"
};
sampleItem.DisplayInfo();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
  "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
  var forecast = Enumerable.Range(1, 5).Select(index =>
    new WeatherForecast
    (
      DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
      Random.Shared.Next(-20, 55),
      summaries[Random.Shared.Next(summaries.Length)]
    ))
    .ToArray();
  return forecast;
})
.WithName("GetWeatherForecast");

using (var context = new LogiTrackContext())
{
  // Add test inventory item if none exist
  if (!context.InventoryItems.Any())
  {
    context.InventoryItems.Add(new InventoryItem
    {
      Name = "Pallet Jack",
      Quantity = 12,
      Location = "Warehouse A"
    });

    context.SaveChanges();
  }

  // Retrieve and print inventory to confirm
  var items = context.InventoryItems.ToList();
  foreach (var item in items)
  {
    item.DisplayInfo(); // Should print: Item: Pallet Jack | Quantity: 12 | Location: Warehouse A
  }
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
