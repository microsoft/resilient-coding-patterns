# Cache-Aside

## Overview
This pattern focuses on improving application performance and scalability by loading data into a cache only when necessary, rather than querying the data store for every request.

## Problem Statement
Applications that query a database or external data source for every request can experience high latency and increased load, especially for frequently accessed data. Without caching, these bottlenecks can degrade performance and increase costs.

## Solution
Implement the cache-aside pattern by having the application check the cache for requested data first. If the data is not present (a cache miss), retrieve it from the data store, store it in the cache, and return it. The application manages cache updates and invalidations as needed.

## When to Use
- When data is read frequently but updated less often.
- To reduce load on databases or external services.
- For improving response times for common queries.

## Benefits
- Reduces latency for frequently accessed data.
- Decreases load on the primary data store.
- Scales easily with increased read traffic.

## Code Samples: C#

**Before pattern implementation**
```csharp
public WeatherForecast GetForecast(string city)
{
    // Direct call with no caching
    return _database.GetWeatherForecast(city);
}


## Linked Artifacts
- [Azure Architecture Center: Cache-Aside Pattern](https://learn.microsoft.com/azure/architecture/patterns/cache-aside)

**Pattern implementation**
```csharp
public WeatherForecast GetForecastWithCache(string city)
{
    if (!_cache.TryGetValue(city, out WeatherForecast forecast))
    {
        forecast = _database.GetWeatherForecast(city);
        _cache.Set(city, forecast, TimeSpan.FromMinutes(10));
    }
    return forecast;
}
```

## Code Samples: Python

**Before pattern implementation**
```python
def get_weather_forecast(city):
    # Direct call with no caching
    return db.get_weather_forecast(city)
```

**Pattern implementation**
```python
cache = {}

def get_weather_forecast(city):
    if city not in cache:
        cache[city] = db.get_weather_forecast(city)
    return cache[city]
```


