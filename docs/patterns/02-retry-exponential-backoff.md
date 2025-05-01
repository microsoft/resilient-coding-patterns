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
        var response = await MakeApiCallWithRetryAsync(url);

        if (response != null && response.IsSuccessStatusCode)
        {
            Console.WriteLine("API call succeeded");
        }
        else
        {
            Console.WriteLine($"API call ultimately failed. Status: {response?.StatusCode}");
        }
    }

    public static async Task<HttpResponseMessage?> MakeApiCallWithRetryAsync(string url)
    {
        const int maxRetries = 5;
        const int baseDelayMs = 500; // 0.5 seconds

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (IsTransient(response.StatusCode))
                {
                    Console.WriteLine($"Transient error: {response.StatusCode}. Attempt {attempt + 1} of {maxRetries}.");

                    if (attempt == maxRetries)
                        return response;

                    await Task.Delay(GetExponentialBackoffWithJitter(baseDelayMs, attempt));
                    continue;
                }

                return response; // Success or non-retryable failure
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                Console.WriteLine($"Network error: {ex.Message}. Attempt {attempt + 1} of {maxRetries}.");
                await Task.Delay(GetExponentialBackoffWithJitter(baseDelayMs, attempt));
            }
        }

        return null; // All retries failed
    }

    private static bool IsTransient(HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.TooManyRequests ||       // 429
        statusCode == HttpStatusCode.ServiceUnavailable ||    // 503
        statusCode == HttpStatusCode.GatewayTimeout ||        // 504
        statusCode == HttpStatusCode.RequestTimeout;          // 408

    private static TimeSpan GetExponentialBackoffWithJitter(int baseDelayMs, int attempt)
    {
        int exponentialDelay = baseDelayMs * (int)Math.Pow(2, attempt);
        int jitter = new Random().Next(0, 1000); // up to 1s of jitter
        return TimeSpan.FromMilliseconds(Math.Min(exponentialDelay + jitter, 30000)); // cap at 30s
    }
}

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
import requests
import time
import random

MAX_RETRIES = 5
BASE_DELAY = 0.5  # seconds
MAX_DELAY = 30    # seconds

TRANSIENT_STATUSES = {408, 429, 503, 504}

def make_api_call_with_retry(url):
    for attempt in range(MAX_RETRIES + 1):
        try:
            response = requests.get(url)

            if response.status_code in TRANSIENT_STATUSES:
                print(f"Transient error: {response.status_code}. Attempt {attempt + 1} of {MAX_RETRIES}.")

                if attempt == MAX_RETRIES:
                    return response  # Give up after final retry

                delay = exponential_backoff_with_jitter(BASE_DELAY, attempt)
                print(f"Waiting {delay:.2f}s before retrying...")
                time.sleep(delay)
                continue

            return response  # Success or non-transient failure

        except requests.exceptions.RequestException as ex:
            print(f"Network error: {ex}. Attempt {attempt + 1} of {MAX_RETRIES}.")

            if attempt == MAX_RETRIES:
                raise  # Rethrow after last attempt

            delay = exponential_backoff_with_jitter(BASE_DELAY, attempt)
            print(f"Waiting {delay:.2f}s before retrying...")
            time.sleep(delay)

    return None  # All retries failed

def exponential_backoff_with_jitter(base, attempt):
    exponential_delay = base * (2 ** attempt)
    jitter = random.uniform(0, 1.0)
    return min(exponential_delay + jitter, MAX_DELAY)

def main():
    url = "https://example.com/api/resource"
    try:
        response = make_api_call_with_retry(url)

        if response and 200 <= response.status_code < 300:
            print("API call succeeded")
        else:
            print(f"API call ultimately failed. Status code: {response.status_code if response else 'None'}")

    except Exception as e:
        print(f"API call failed with an unrecoverable error: {e}")

if __name__ == "__main__":
    main()

```

## Related Patterns

- [Retry Pattern](01-retry.md)