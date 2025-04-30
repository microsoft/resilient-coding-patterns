using DisasterAPI.Interfaces;
using DisasterAPI.Services;
using DisasterAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton<DisasterAPI.Services.WeatherService>();

// Register all services in the DI container
builder.Services.AddSingleton<IWeatherDataProvider, WeatherDataProvider>();
builder.Services.AddSingleton<SharedCacheService>();
builder.Services.AddSingleton<IWeatherProcessor, WeatherProcessor>();
builder.Services.AddSingleton<IWeatherService, WeatherService>();

// Register logger as transient - innocent looking but will cause memory leaks
builder.Services.AddTransient<IAppLogger, Logger>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", (DisasterAPI.Services.WeatherService weatherService) =>
{
    try
    {
        // This calls the service method that already has its own try-catch
        var forecast = weatherService.GetForecasts();
        
        // Additional artificial complexity that could lead to problems
        if (forecast == null)
        {
            throw new ArgumentNullException(nameof(forecast));
        }
        
        return forecast;
    }
    catch (Exception ex)
    {
        // Bad practice: Another layer of general exception handling
        // This creates a nested try-catch scenario as the service already has try-catch
        Console.WriteLine($"API endpoint error: {ex.Message}");
        return Enumerable.Empty<WeatherForecast>();
    }
})
.WithName("GetWeatherForecast");

app.Run();
