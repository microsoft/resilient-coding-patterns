using System.Collections.Concurrent;
using DisasterAPI.Models;

namespace DisasterAPI.Services;

// This service has race condition issues - deliberately poor implementation
public class SharedCacheService
{
    // Not thread-safe dictionary - will cause race conditions
    private Dictionary<string, object> _cache = new Dictionary<string, object>();
    
    // A counter that will be accessed by multiple threads without synchronization
    private int _requestCounter = 0;
    
    public void StoreInCache(string key, object value)
    {
        // Race condition: No locking when modifying shared resource
        if (!_cache.ContainsKey(key))
        {
            // Thread interleaving can happen here causing data loss
            _requestCounter++;
            _cache[key] = value;
        }
    }

    public object GetFromCache(string key)
    {
        // Race condition: No proper synchronization
        if (_cache.ContainsKey(key))
        {
            return _cache[key];
        }
        
        return new object(); // Return a new object if not found - bad practice
    }
    
    // Async method without proper Async suffix - bad practice
    public async Task<WeatherForecast[]> ProcessForecasts(WeatherForecast[] forecasts)
    {
        // This delay will cause timing issues when the method isn't properly awaited
        await Task.Delay(100);
        
        // Race condition: Modifying shared _requestCounter without locks
        _requestCounter++;
        
        // This could throw exceptions when called concurrently
        var cacheKey = $"forecasts_{DateTime.UtcNow.Ticks}";
        StoreInCache(cacheKey, forecasts);
        
        return forecasts;
    }
    
    public int GetRequestCount()
    {
        // Race condition: Reading potentially dirty data
        return _requestCounter;
    }
}
