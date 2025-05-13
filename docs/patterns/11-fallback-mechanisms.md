# Fallback Mechanisms

## Overview
The fallback mechanisms pattern provides alternative actions or responses when a primary operation fails. Instead of immediately propagating errors to users or dependent services, the system falls back to alternative data sources, simplified functionality, cached results, or sensible default values. This approach improves resilience by maintaining at least partial functionality during failures.

## Problem Statement
Systems that rely on a single path of execution can fail completely when any component in that path encounters an error. This creates a brittle architecture where minor issues can lead to complete service outages. Without fallback options, systems face binary outcomes: either everything works perfectly, or nothing works at all.

## Solution
Implement alternative execution paths that activate when primary operations fail. Design your system to detect failures and automatically switch to predetermined fallback strategies based on the nature of the failure. These strategies may include using cached data, simplified algorithms, alternative services, or gracefully degraded functionality.

## When to Use
- For systems requiring high availability
- When partial functionality is better than complete failure
- In user-facing applications where continuity of experience is important
- When integrating with unreliable external services
- For operations that can succeed with reduced quality or older data

## Benefits
- Improves system resilience during partial failures
- Enhances user experience by avoiding complete outages
- Provides time for primary systems to recover
- Reduces cascading failures across dependent services
- Enables graceful degradation instead of abrupt failures

## Code Samples: C#

**Before pattern implementation**
```csharp
public class WeatherService
{
    private readonly IWeatherApi _weatherApi;
    
    public WeatherService(IWeatherApi weatherApi)
    {
        _weatherApi = weatherApi;
    }
    
    public async Task<WeatherForecast> GetForecastAsync(string city)
    {
        // Single point of failure - if the API call fails, the entire operation fails
        return await _weatherApi.GetForecastAsync(city);
    }
}
```

**Pattern implementation**
```csharp
public class WeatherServiceWithFallback
{
    private readonly IWeatherApi _primaryApi;
    private readonly IWeatherApi _secondaryApi;
    private readonly ICache _cache;
    private readonly ILogger _logger;
    
    public WeatherServiceWithFallback(
        IWeatherApi primaryApi,
        IWeatherApi secondaryApi,
        ICache cache,
        ILogger logger)
    {
        _primaryApi = primaryApi;
        _secondaryApi = secondaryApi;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<WeatherForecast> GetForecastAsync(string city)
    {
        // Fallback Strategy 1: Try primary API
        try
        {
            var forecast = await _primaryApi.GetForecastAsync(city);
            
            // Cache successful result for later use
            await _cache.SetAsync($"weather_{city}", forecast, TimeSpan.FromHours(1));
            
            return forecast;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary weather API failed for {City}", city);
        }
        
        // Fallback Strategy 2: Try secondary API
        try
        {
            _logger.LogInformation("Trying secondary API for {City}", city);
            return await _secondaryApi.GetForecastAsync(city);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Secondary weather API failed for {City}", city);
        }
        
        // Fallback Strategy 3: Try cached data
        var cachedForecast = await _cache.GetAsync<WeatherForecast>($"weather_{city}");
        if (cachedForecast != null)
        {
            _logger.LogInformation("Using cached weather data for {City}", city);
            return cachedForecast;
        }
        
        // Fallback Strategy 4: Return default values as last resort
        _logger.LogWarning("All weather sources failed, using default forecast for {City}", city);
        return new WeatherForecast
        {
            City = city,
            Temperature = 72,
            Description = "Unknown (fallback data)",
            ForecastDate = DateTime.Now,
            IsFallbackData = true
        };
    }
}
```

## Code Examples: Python

**Before pattern implementation**
```python
class ProductService:
    def __init__(self, database_client):
        self.database_client = database_client
    
    async def get_product_details(self, product_id):
        # Single point of failure - if the database query fails, the entire operation fails
        product = await self.database_client.get_product(product_id)
        
        if product is None:
            raise ProductNotFoundException(f"Product with ID {product_id} not found")
            
        return product
```

**Pattern implementation**
```python
import logging
import asyncio
from datetime import datetime, timedelta

logger = logging.getLogger(__name__)

class ProductServiceWithFallback:
    def __init__(self, primary_db, replica_db, cache_client):
        self.primary_db = primary_db
        self.replica_db = replica_db
        self.cache_client = cache_client
    
    async def get_product_details(self, product_id):
        # Fallback Strategy 1: Try primary database
        try:
            logger.info(f"Attempting to fetch product {product_id} from primary database")
            product = await self.primary_db.get_product(product_id)
            
            # Cache successful result for later use
            await self.cache_client.set(f"product_{product_id}", product, expire=3600)
            
            return product
        except Exception as e:
            logger.warning(f"Primary database failed: {str(e)}")
        
        # Fallback Strategy 2: Try replica database
        try:
            logger.info(f"Attempting to fetch product {product_id} from replica database")
            product = await self.replica_db.get_product(product_id)
            return product
        except Exception as e:
            logger.warning(f"Replica database failed: {str(e)}")
        
        # Fallback Strategy 3: Try cache
        try:
            logger.info(f"Attempting to fetch product {product_id} from cache")
            product = await self.cache_client.get(f"product_{product_id}")
            if product:
                logger.info(f"Retrieved product {product_id} from cache")
                # Mark data as potentially stale
                product["from_cache"] = True
                return product
        except Exception as e:
            logger.warning(f"Cache retrieval failed: {str(e)}")
        
        # Fallback Strategy 4: Return basic product information if we have it stored locally
        try:
            basic_products = {
                "12345": {"id": "12345", "name": "Basic Widget", "price": 19.99},
                "67890": {"id": "67890", "name": "Standard Gadget", "price": 24.99}
            }
            
            if product_id in basic_products:
                logger.info(f"Using basic product information for {product_id}")
                product = basic_products[product_id]
                product["is_fallback"] = True
                return product
        except Exception:
            pass
        
        # If all fallbacks fail, raise an exception
        logger.error(f"All fallbacks failed for product {product_id}")
        raise ProductUnavailableException(f"Product {product_id} is currently unavailable")
```

## Related Patterns
- [Azure Architecture Center: Retry Pattern](https://learn.microsoft.com/azure/architecture/patterns/retry)
- [Azure Architecture Center: Circuit Breaker Pattern](https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker)
- [Azure Architecture Center: Compensating Transaction Pattern](https://learn.microsoft.com/azure/architecture/patterns/compensating-transaction)


