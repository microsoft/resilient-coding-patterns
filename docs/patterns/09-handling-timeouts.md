# Handling Timeouts

## Overview
The timeout handling pattern focuses on setting appropriate time limits for operations that interact with external resources or services. By defining clear timeouts, applications can avoid waiting indefinitely for responses that may never arrive, improve reliability by failing fast, and provide better user experiences even during transient failures or performance degradations.

## Problem Statement
When applications communicate with external systems such as databases, web services, or message queues, these dependencies may become slow or unresponsive. Without proper timeout handling, requests can block indefinitely, potentially exhausting resources like connection pools, threads, and memory. This can lead to cascading failures, where slow responses in one component cause the entire system to become unresponsive.

## Solution
Implement explicit timeouts for all operations that involve external systems or long-running processes. When a timeout occurs, gracefully handle the exception, release resources, and take appropriate recovery actions such as retrying with backoff, falling back to alternative data sources, or returning cached data. Configure timeout values based on the operation context and criticality to balance responsiveness with success rate.

## When to Use
- For any operation that depends on an external system or service
- When making network requests to APIs or services
- During database queries or operations
- For long-running background tasks
- In user-facing applications where responsiveness is critical

## Benefits
- Prevents resource exhaustion from hung operations
- Improves system resilience and stability
- Enables graceful degradation during dependency failures
- Provides predictable response times
- Facilitates better user experience even during partial system failures

## Code Samples: C#

**Before pattern implementation**
```csharp
public class ExternalServiceClient
{
    private readonly HttpClient _httpClient = new HttpClient();
    
    public async Task<string> GetDataAsync(string endpoint)
    {
        // No timeout specified - could wait indefinitely
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Pattern implementation**
```csharp
public class TimeoutAwareServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TimeoutAwareServiceClient> _logger;
    
    public TimeoutAwareServiceClient(ILogger<TimeoutAwareServiceClient> logger)
    {
        _logger = logger;
        
        // Configure default timeout
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        
        _logger.LogInformation("HTTP client configured with 5 second timeout");
    }
    
    public async Task<string> GetDataAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create timeout token and combine with caller's token
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
            
            // Pass the combined token to respect both timeout and caller cancellation
            var response = await _httpClient.GetAsync(endpoint, linkedCts.Token);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(linkedCts.Token);
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning($"Request to {endpoint} was canceled by caller");
                throw;
            }
            
            _logger.LogError($"Request to {endpoint} timed out");
            throw new TimeoutException($"Request to {endpoint} timed out", ex);
        }
    }
}
```

## Code Examples: Python

**Before pattern implementation**
```python
import requests

def get_data(endpoint):
    # No timeout specified - could wait indefinitely
    response = requests.get(endpoint)
    response.raise_for_status()
    return response.json()
```

**Pattern implementation**
```python
import requests
import logging

logger = logging.getLogger(__name__)

class TimeoutAwareClient:
    def __init__(self, default_timeout=5.0):
        self.default_timeout = default_timeout
        logger.info(f"Client configured with {default_timeout}s timeout")
    
    def get_data(self, endpoint, timeout=None):
        # Use provided timeout or default
        timeout = timeout or self.default_timeout
        logger.debug(f"Requesting {endpoint} with {timeout}s timeout")
        
        try:
            # Set explicit timeout in seconds
            response = requests.get(endpoint, timeout=timeout)
            response.raise_for_status()
            return response.json()
        except requests.exceptions.Timeout:
            logger.error(f"Request to {endpoint} timed out after {timeout}s")
            raise TimeoutError(f"Request timed out after {timeout}s")
        except requests.exceptions.RequestException as e:
            logger.error(f"Request failed: {str(e)}")
            raise
```

## Related Patterns
- [Azure Architecture Center: Retry Pattern](https://learn.microsoft.com/azure/architecture/patterns/retry)
- [Azure Architecture Center: Circuit Breaker Pattern](https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker)
- [Azure Architecture Center: Throttling Pattern](https://learn.microsoft.com/azure/architecture/patterns/throttling)


