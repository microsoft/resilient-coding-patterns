using DisasterAPI.Interfaces;
using DisasterAPI.Models;

namespace DisasterAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherProcessor _weatherProcessor;

    public WeatherService(IWeatherProcessor weatherProcessor)
    {
        _weatherProcessor = weatherProcessor;
    }

    public WeatherForecast[] GetForecasts()
    {
        try
        {
            // Top-level try-catch that calls a method which also has try-catch
            return _weatherProcessor.GenerateForecasts(5);
        }
        catch (Exception ex)
        {
            // Bad practice: catching general Exception and swallowing it
            Console.WriteLine($"Error occurred in weather service: {ex.Message}");
            return Array.Empty<WeatherForecast>();
        }
    }
}
