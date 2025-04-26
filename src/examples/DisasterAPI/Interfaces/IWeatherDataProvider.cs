namespace DisasterAPI.Interfaces;

public interface IWeatherDataProvider
{
    string[] GetWeatherSummaries();
    int GetRandomTemperature();
}
