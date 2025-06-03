# Circuit Breaker Pattern

## Overview
The Circuit Breaker pattern prevents an application from repeatedly attempting operations that are likely to fail. Like an electrical circuit breaker, it detects failures and "trips" to block further requests, allowing the failing component time to recover. After a timeout period, the circuit allows a limited number of test requests to determine if the problem is fixed before returning to normal operation.

## Problem Statement
When a service is struggling or unavailable, repeated failed calls can worsen the situation, leading to cascading failures across dependent systems. These retry attempts consume resources and can prevent the failing service from recovering, creating a negative feedback loop that degrades the entire system's performance and availability.

## Solution
Implement a circuit breaker that monitors for failures. When failures reach a threshold, the circuit "trips" and all further calls to the service fail fast without actually attempting the operation. After a configured timeout, the circuit switches to a "half-open" state, allowing a limited number of test requests. If these succeed, the circuit closes and normal operation resumes; if they fail, the circuit returns to the open state for another timeout period.

## When to Use
- When calling remote services or resources that might fail or become unresponsive
- For operations that might fix themselves through a timeout or retry after delay
- To prevent cascading failures in distributed systems
- When rapid failure responses are preferable to waiting for timeouts

## Benefits
- Prevents cascading system failures
- Enables graceful degradation during partial outages
- Allows failing components time to recover
- Reduces resource consumption from failed operations
- Provides fast failure responses rather than hanging requests

## Code Samples: C#

**Before pattern implementation**
```csharp
public class ServiceClient
{
    private readonly HttpClient _httpClient;
    
    public ServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> GetDataAsync(string endpoint)
    {
        // Every call is attempted regardless of previous failures
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

**Pattern implementation**
```csharp
// The three circuit states
public enum CircuitState { Closed, Open, HalfOpen }

public class CircuitBreaker
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount;
    private DateTime _openedAt;
    private readonly int _failureThreshold;
    private readonly TimeSpan _recoveryTime;
    
    public CircuitBreaker(int failureThreshold = 3, int recoveryTimeSeconds = 30)
    {
        _failureThreshold = failureThreshold;
        _recoveryTime = TimeSpan.FromSeconds(recoveryTimeSeconds);
    }
    
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        // Check if circuit is open (broken)
        if (_state == CircuitState.Open)
        {
            // Check if recovery timeout has elapsed
            if (DateTime.UtcNow - _openedAt > _recoveryTime)
            {
                // Allow one test request through
                _state = CircuitState.HalfOpen;
            }
            else
            {
                // Fail fast without calling the service
                throw new Exception("Circuit breaker is open");
            }
        }
        
        try
        {
            // Attempt the operation
            T result = await operation();
            
            // If successful in half-open state, reset the circuit
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
                _failureCount = 0;
            }
            
            return result;
        }
        catch (Exception)
        {
            // Track failures
            _failureCount++;
            
            // Trip the circuit if threshold reached or already in half-open state
            if (_state == CircuitState.HalfOpen || _failureCount >= _failureThreshold)
            {
                _state = CircuitState.Open;
                _openedAt = DateTime.UtcNow;
            }
            
            throw;
        }
    }
}

public class ServiceClientWithCircuitBreaker
{
    private readonly HttpClient _httpClient;
    private readonly CircuitBreaker _circuitBreaker = new CircuitBreaker();
    
    public ServiceClientWithCircuitBreaker(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public Task<string> GetDataAsync(string endpoint)
    {
        return _circuitBreaker.ExecuteAsync(async () =>
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        });
    }
}
```

## Code Examples: Python

**Before pattern implementation**
```python
import requests

class PaymentService:
    def __init__(self, api_url):
        self.api_url = api_url
        
    def process_payment(self, payment_details):
        # Every call is attempted regardless of previous failures
        response = requests.post(
            f"{self.api_url}/process", 
            json=payment_details,
            timeout=5
        )
        response.raise_for_status()
        return response.json()
```

**Pattern implementation**
```python
import enum
import time
from datetime import datetime, timedelta

class CircuitState(enum.Enum):
    CLOSED = 0      # Normal operation - requests flow through
    OPEN = 1        # Circuit is broken - requests fail fast
    HALF_OPEN = 2   # Testing the service with limited requests

class CircuitBreaker:
    def __init__(self, failure_threshold=3, recovery_time=30):
        self.state = CircuitState.CLOSED
        self.failure_count = 0
        self.failure_threshold = failure_threshold
        self.recovery_time = timedelta(seconds=recovery_time)
        self.opened_at = None
    
    def execute(self, func):
        """Execute the function with circuit breaker logic"""
        # Check if circuit is open
        if self.state == CircuitState.OPEN:
            # Check if we should try a test request
            if self.opened_at and datetime.now() - self.opened_at > self.recovery_time:
                # Transition to half-open to test the service
                self.state = CircuitState.HALF_OPEN
            else:
                # Fail fast without calling the service
                raise Exception("Circuit breaker is open")
        
        try:
            # Attempt the operation
            result = func()
            
            # If successful in half-open state, reset the circuit
            if self.state == CircuitState.HALF_OPEN:
                self.state = CircuitState.CLOSED
                self.failure_count = 0
            
            return result
            
        except Exception as e:
            # Track failures
            self.failure_count += 1
            
            # Trip the circuit if threshold reached or already in half-open state
            if self.state == CircuitState.HALF_OPEN or self.failure_count >= self.failure_threshold:
                self.state = CircuitState.OPEN
                self.opened_at = datetime.now()
            
            # Re-raise the exception
            raise

class PaymentServiceWithCircuitBreaker:
    def __init__(self, api_url):
        self.api_url = api_url
        self.circuit_breaker = CircuitBreaker()
    
    def process_payment(self, payment_details):
        def do_process_payment():
            response = requests.post(
                f"{self.api_url}/process", 
                json=payment_details,
                timeout=5
            )
            response.raise_for_status()
            return response.json()
        
        # Execute with circuit breaker protection
        return self.circuit_breaker.execute(do_process_payment)
```

## Related Patterns
- [Azure Architecture Center: Circuit Breaker Pattern](https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker)
- [Azure Architecture Center: Retry Pattern](https://learn.microsoft.com/azure/architecture/patterns/retry)
- [Azure Architecture Center: Bulkhead Pattern](https://learn.microsoft.com/azure/architecture/patterns/bulkhead)
- [Azure Architecture Center: Fallback Pattern](https://learn.microsoft.com/azure/architecture/patterns/)


