# Bulkhead

## Overview
The bulkhead pattern is a resiliency strategy that isolates critical resources, such as threads or connections, into separate pools. This ensures that if one pool is exhausted or fails, the others continue to function, preventing cascading failures and improving overall system stability.

## Problem Statement
In distributed or cloud applications, a single component or resource can become overwhelmed, causing failures that cascade and impact the entire system. Without isolation, resource exhaustion in one area (e.g., a slow external API) can degrade or bring down unrelated parts of the application.

## Solution
With the bulkhead pattern, resources are partitioned into independent pools. Each pool handles a specific workload or service. If one pool is full or fails, only that part of the system is affected, while others continue to operate normally. This containment prevents widespread outages and improves reliability.

## When to Use
- When you need to prevent a single component's failure from affecting the whole system
- To limit the impact of resource exhaustion (e.g., thread, connection, or memory limits)
- In microservices or distributed systems where isolation is critical

## Benefits
- Increases system resilience by containing failures
- Prevents resource exhaustion from cascading
- Enables graceful degradation under load
- Improves reliability and availability

## Code Samples: C#

**Before pattern implementation**
```csharp
public async Task<string> CallExternalServiceAsync()
{
    // No isolation: all requests share the same thread pool
    return await _externalService.GetDataAsync();
}
```

**Pattern implementation**
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;

public class BulkheadService : IDisposable
{
    private readonly SemaphoreSlim _semaphore;

    public BulkheadService(int maxParallel)
    {
        _semaphore = new SemaphoreSlim(maxParallel, maxParallel);
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await operation();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _semaphore?.Dispose();
    }
}

// Usage example:
// var bulkhead = new BulkheadService(3); // Allow max 3 concurrent
// var result = await bulkhead.ExecuteAsync(() => _externalService.GetDataAsync());
```

## Code Examples: Python

**Before pattern implementation**
```python
def call_external_service():
    # No isolation: all requests share the same resource pool
    return external_service.get_data()
```

**Pattern implementation**
```python
import threading

class Bulkhead:
    def __init__(self, max_parallel):
        self.semaphore = threading.Semaphore(max_parallel)

    def execute(self, func, *args, **kwargs):
        with self.semaphore:
            return func(*args, **kwargs)

# Usage example:
# bulkhead = Bulkhead(3)
# result = bulkhead.execute(external_service.get_data)
```

## Related Patterns
- [Azure Architecture Center: Bulkhead Pattern](https://learn.microsoft.com/azure/architecture/patterns/bulkhead)


