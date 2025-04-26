namespace DisasterAPI.Interfaces;

public interface IAppLogger
{
    void LogInfo(string message);
    void LogError(string message, Exception ex);
}
