using DisasterAPI.Interfaces;

namespace DisasterAPI.Services;

public class WeatherDataProvider : IWeatherDataProvider
{
    private readonly string[] _summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public string[] GetWeatherSummaries()
    {
        try
        {
            // First layer of try-catch, hidden in a low-level component
            return _summaries;
        }
        catch (Exception ex)
        {
            // Bad practice: swallowing exception at data level
            Console.WriteLine($"Error retrieving weather summaries: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    public int GetRandomTemperature()
    {
        try
        {
            return Random.Shared.Next(-20, 55);
        }
        catch (Exception ex)
        {
            // More bad practice: swallowing exception
            Console.WriteLine($"Error generating temperature: {ex.Message}");
            return 0;
        }
    }
}
