# Retry with Exponential Backoff

## Overview

The retry exponential backoff pattern is a common error-handling strategy used in cloud and distributed systems to deal with transient failures (temporary issues like timeouts, throttling, or rate limits).

## Problem Statement

In our distributed cloud application, calls to external APIs and platform services (e.g., Azure Key Vault, Cosmos DB, or third-party APIs) occasionally fail due to transient issues such as throttling, timeouts, or temporary unavailability. These failures are not due to bugs or misconfigurations, but instead are expected behavior under load or during brief service degradation.

Without a proper retry strategy, these failures result in:
- Unnecessary error surfacing to end users
- Increased load on the downstream service due to aggressive retries
- Reduced overall system resilience

## Solution

Implement a retry mechanism with exponential backoff and jitter to:
- Reduce the likelihood of overwhelming services with synchronized retries
- Increase the chances of eventual success without operator intervention
- Improve user experience and system reliability under transient failure conditions
  
## When to Use

- API calls to external services (especially rate-limited ones)
- Reading from cloud storage or queues
- Any operation likely to fail temporarily, not permanently
  
## Benefits

- **Load balancing**: Avoids hammering the service repeatedly when it's under stress
- **Recovery**: Gives the system time to recover between retries
- **Cascading failures**: Helps prevent cascading failures in distributed systems

## Code Samples: C#

**Before pattern implementation**
```csharp
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _httpClient = new HttpClient();

    static async Task Main()
    {
        string url = "https://example.com/api/resource";
        var response = await MakeApiCallAsync(url);
        
        // Only checking for success here (no retry yet)
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("API call succeeded");
        }
        else
        {
            Console.WriteLine($"API call failed with status code: {response.StatusCode}");
        }
    }

    public static async Task<HttpResponseMessage> MakeApiCallAsync(string url)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Observing status codes indicating transient issues
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // Rate limit reached, can retry
                Console.WriteLine("Received 429 (Too Many Requests). Potentially retrying...");
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                // Service unavailable, retry might work
                Console.WriteLine("Received 503 (Service Unavailable). Potentially retrying...");
            }
            else if (response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                // Gateway timeout, could be a network issue
                Console.WriteLine("Received 504 (Gateway Timeout). Potentially retrying...");
            }
            else if (response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                // Network request timeout
                Console.WriteLine("Received 408 (Request Timeout). Potentially retrying...");
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            // Network connectivity issue
            Console.WriteLine($"Request failed due to a network issue: {ex.Message}. Potentially retrying...");
            throw;
        }
    }
}
```

**Pattern implementation**

```csharp

```

## Code Examples: Python

**Before pattern implementation**

```python
import requests

def make_api_call(url):
    try:
        response = requests.get(url)

        # Observing status codes indicating transient issues
        if response.status_code == 429:
            # Rate limit reached, can retry
            print("Received 429 (Too Many Requests). Potentially retrying...")
        elif response.status_code == 503:
            # Service unavailable, retry might work
            print("Received 503 (Service Unavailable). Potentially retrying...")
        elif response.status_code == 504:
            # Gateway timeout, could be a network issue
            print("Received 504 (Gateway Timeout). Potentially retrying...")
        elif response.status_code == 408:
            # Network request timeout
            print("Received 408 (Request Timeout). Potentially retrying...")

        return response
    except requests.exceptions.RequestException as ex:
        # Network connectivity issue
        print(f"Request failed due to a network issue: {ex}. Potentially retrying...")
        raise

def main():
    url = "https://example.com/api/resource"
    try:
        response = make_api_call(url)

        # Only checking for success here (no retry yet)
        if response.status_code >= 200 and response.status_code < 300:
            print("API call succeeded")
        else:
            print(f"API call failed with status code: {response.status_code}")
    except Exception as e:
        print(f"An error occurred: {e}")

if __name__ == "__main__":
    main()
```

**Pattern implementation**

```python

```

## Related Patterns

- [Retry Pattern](01-retry.md)