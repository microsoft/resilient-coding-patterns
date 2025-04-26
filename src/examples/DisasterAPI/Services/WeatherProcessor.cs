using DisasterAPI.Interfaces;
using DisasterAPI.Models;

namespace DisasterAPI.Services;

public class WeatherProcessor : IWeatherProcessor
{
    private readonly IWeatherDataProvider _dataProvider;
    private readonly SharedCacheService _cacheService;
    private readonly IAppLogger _logger; // Added logger dependency

    public WeatherProcessor(
        IWeatherDataProvider dataProvider, 
        SharedCacheService cacheService,
        IAppLogger logger) // Inject the transient logger
    {
        _dataProvider = dataProvider;
        _cacheService = cacheService;
        _logger = logger;
    }

    public WeatherForecast[] GenerateForecasts(int days)
    {
        try
        {
            // Log each request - creates memory pressure
            _logger.LogInfo($"Generating {days} days of forecasts");
            
            var summaries = _dataProvider.GetWeatherSummaries();
            
            var forecasts = Enumerable.Range(1, days).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    _dataProvider.GetRandomTemperature(),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();

            // Bad Practice: Fire and forget - not awaiting an async method
            // This will cause race conditions and potential task completion issues
            _ = _cacheService.ProcessForecasts(forecasts);
            
            // Bad Practice: Blocking async code with .Result
            // This can cause deadlocks in UI or ASP.NET contexts
            var processedData = Task.Run(() => ProcessData(forecasts)).Result;
            
            // Artificially introduce potential for exceptions
            if (forecasts.Length == 0)
            {
                throw new InvalidOperationException("No forecasts could be generated");
            }
            
            // Log more information - more memory allocations
            _logger.LogInfo($"Successfully created {forecasts.Length} forecasts");
            
            return forecasts;
        }
        catch (Exception ex)
        {
            // Log error with full exception details - even more memory
            _logger.LogError($"Error in forecast processing: {ex.Message}", ex);
            throw; // This will propagate to the service layer
        }
    }
    
    // An async method without the proper Async suffix - bad practice
    private async Task<bool> ProcessData(WeatherForecast[] forecasts)
    {
        // Simulate processing
        await Task.Delay(1000);
        
        // Thread safety issue - multiple threads can update this concurrently
        foreach (var forecast in forecasts)
        {
            _cacheService.StoreInCache($"forecast_{forecast.Date}", forecast);
        }
        
        return true;
    }
}
