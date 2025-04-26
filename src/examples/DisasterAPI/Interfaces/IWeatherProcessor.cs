using DisasterAPI.Models;

namespace DisasterAPI.Interfaces;

public interface IWeatherProcessor
{
    WeatherForecast[] GenerateForecasts(int days);
}
