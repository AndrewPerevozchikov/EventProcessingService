using EventProcessingService.Data;
using EventProcessingService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.AddApplicationServices();

var app = builder.Build();

app.MapGet("/stats", async (IDataStorage storage) =>
{
    var stats = await storage.GetStatistics();
    return Results.Ok(stats);
});

app.Run();