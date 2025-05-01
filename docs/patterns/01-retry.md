# Retry

## Overview
This pattern focuses on automatically retrying failed operations, such as network calls or database queries, in order to handle transient errors.

## Problem Statement
Applications can sometimes experience intermittent failures or timeouts. Without a retry mechanism, these transient errors can cause the entire process to fail.

## Solution
Implement a retry strategy that captures errors and attempts the operation again after a delay. This helps recover from temporary problems without manual intervention.

## When to Use
- Network calls that occasionally fail due to transient issues.
- Database connections susceptible to temporary timeouts.
- External service calls where responses might be unreliable.

## Benefits
- Reduces downtime by transparently recovering from intermittent failures.
- Increases fault tolerance with minimal code changes.
- Improves the end-user experience by masking transient errors.

## Code Samples: C#

**Before pattern implementation**
```csharp
public async Task<string> GetDataAsync()
{
    // Direct call with no retries
    return await SomeHttpClient.GetStringAsync("https://example.com/data");
}
```

**Pattern implementation**

```csharp
public async Task<string> GetDataWithRetryAsync()
{
    var retryCount = 3;
    for (int i = 0; i < retryCount; i++)
    {
        try
        {
            return await SomeHttpClient.GetStringAsync("https://example.com/data");
        }
        catch (HttpRequestException ex)
        {
            if (i == retryCount - 1) throw;
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
    return string.Empty;
}

