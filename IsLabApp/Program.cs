var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.Use(async (context, next) => {
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");
    await next();
});

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
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }));
app.MapGet("/version", () => Results.Ok(new { message = "L12: Updated version v3.0" }));

var notes = new List<Note>();
var nextId = 1;

app.MapGet("/api/notes", () => notes);
app.MapGet("/api/notes/{id}", (int id) =>
    notes.FirstOrDefault(n => n.Id == id) is Note n ? Results.Ok(n) : Results.NotFound());
app.MapPost("/api/notes", (Note dto) => {
    if (string.IsNullOrWhiteSpace(dto.Title)) return Results.BadRequest("Title required");
    var note = new Note(nextId++, dto.Title, dto.Text, DateTime.UtcNow);
    notes.Add(note);
    return Results.Created($"/api/notes/{note.Id}", note);
});
app.MapDelete("/api/notes/{id}", (int id) => {
    var n = notes.FirstOrDefault(n => n.Id == id);
    if (n is null) return Results.NotFound();
    notes.Remove(n);
    return Results.NoContent();
});

app.MapGet("/db/ping", (IConfiguration config) => {
    try {
        var conn = new Microsoft.Data.SqlClient.SqlConnection(config.GetConnectionString("Mssql"));
        conn.Open();
        return Results.Ok(new { status = "ok" });
    } catch (Exception ex) {
        return Results.Ok(new { status = "error", message = ex.Message });
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
