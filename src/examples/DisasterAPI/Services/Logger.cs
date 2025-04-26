using DisasterAPI.Interfaces;

namespace DisasterAPI.Services;

// Looks like a proper service with helpful features, but creates memory leaks
public class Logger : IAppLogger
{
    // Static collection disguised as a legitimate caching mechanism
    private static readonly List<string> _historicalEntries = new();
    
    // Large buffer with a sensible-sounding name
    private readonly byte[] _analysisBuffer = new byte[1024 * 1024]; // 1MB per instance
    
    // Instance collection described as session tracking
    private readonly List<string> _currentSessionData = new();

    public void LogInfo(string message)
    {
        var logEntry = $"INFO [{DateTime.Now}]: {message}";
        
        // Add to instance collection
        _currentSessionData.Add(logEntry);
        
        // Add to static collection that grows forever - but sounds legitimate
        _historicalEntries.Add(logEntry);
        
        // Same memory waste but with a more innocent description
        if (Random.Shared.Next(100) > 95)
        {
            _analysisBuffer[Random.Shared.Next(_analysisBuffer.Length)] = (byte)Random.Shared.Next(256);
        }
        
        // Additional allocations - looks like normal string handling
        var enrichedData = string.Concat(message, " - processed at ", DateTime.Now.Ticks);
    }

    public void LogError(string message, Exception ex)
    {
        var logEntry = $"ERROR [{DateTime.Now}]: {message}";
        if (ex != null)
        {
            // Create even more strings that are kept in memory
            logEntry += $"\nException: {ex.Message}\nStack: {ex.StackTrace}";
            
            if (ex.InnerException != null) 
            {
                // Recursive function that creates more objects
                LogInnerExceptions(ex.InnerException, 1);
            }
        }
        
        _currentSessionData.Add(logEntry);
        _historicalEntries.Add(logEntry);
    }
    
    // Recursive method that creates more objects for nested exceptions
    private void LogInnerExceptions(Exception ex, int level)
    {
        var indent = new string(' ', level * 2);
        var logEntry = $"{indent}Inner: {ex.Message}";
        
        _currentSessionData.Add(logEntry);
        _historicalEntries.Add(logEntry);
        
        if (ex.InnerException != null)
        {
            LogInnerExceptions(ex.InnerException, level + 1);
        }
    }
}