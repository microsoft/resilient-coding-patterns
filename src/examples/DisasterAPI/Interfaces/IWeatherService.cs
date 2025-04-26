using DisasterAPI.Models;

namespace DisasterAPI.Interfaces;

public interface IWeatherService
{
    WeatherForecast[] GetForecasts();
}
