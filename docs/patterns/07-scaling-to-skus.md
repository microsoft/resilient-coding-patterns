# Scaling to different SKUs

## Overview
The scaling to different SKUs pattern involves designing applications to run efficiently across various hardware or service tiers, adapting functionality based on the capabilities and constraints of each tier. This approach ensures that applications can operate cost-effectively while maximizing the value from each service level, providing flexibility for different deployment scenarios and budget constraints.

## Problem Statement
Cloud services offer multiple pricing tiers (SKUs) with different performance characteristics, features, and costs. Without proper design, applications may fail to function correctly when deployed to different tiers, or they may not utilize the full capabilities of premium tiers while still incurring their costs. This leads to either over-provisioning (paying for unused capabilities) or under-provisioning (performance issues or feature limitations).

## Solution
Design applications to detect and adapt to the capabilities of the current SKU at runtime. Implement feature detection, graceful fallbacks, and dynamic configuration that allows your application to optimize its behavior based on available resources. Scale out operations or utilize premium features only when they are available, while providing alternative implementations for lower tiers.

## When to Use
- When you need to support deployment across different pricing tiers
- To optimize costs while maintaining essential functionality
- When creating solutions for customers with varying budget constraints
- For applications that need to scale from development to production environments with different capabilities

## Benefits
- Cost optimization by right-sizing resources to actual needs
- Flexibility to deploy the same codebase across different environments
- Graceful degradation when moving to lower tiers
- Enhanced functionality when premium features are available
- Better customer experience across different pricing options

## Code Samples: C#

**Before pattern implementation**
```csharp
public class DataProcessor
{
    private readonly CosmosClient _cosmosClient;
    
    public DataProcessor(string connectionString)
    {
        // Hardcoded configuration assuming high-performance tier
        _cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Direct,
            MaxRetryAttemptsOnRateLimitedRequests = 9,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
        });
    }
    
    public async Task ProcessLargeDataSetAsync(IEnumerable<DataItem> items)
    {
        // Always uses parallel processing assuming high-performance hardware
        var container = _cosmosClient.GetContainer("MyDatabase", "MyContainer");
        
        // Always use high throughput with bulk processing - may fail on lower SKUs
        var tasks = items.Select(item => container.UpsertItemAsync(item));
        await Task.WhenAll(tasks);
    }
}
```

**Pattern implementation**
```csharp
public class SKUAwareDataProcessor
{
    private readonly CosmosClient _cosmosClient;
    private readonly ILogger<SKUAwareDataProcessor> _logger;
    private readonly SKUCapabilities _skuCapabilities;
    
    public SKUAwareDataProcessor(
        string connectionString, 
        IConfiguration configuration,
        ILogger<SKUAwareDataProcessor> logger)
    {
        _logger = logger;
        
        // Determine SKU and capabilities from configuration or environment
        _skuCapabilities = DetectSKUCapabilities(configuration);
        _logger.LogInformation($"Running on SKU: {_skuCapabilities.Tier}, Parallelism: {_skuCapabilities.MaxParallelism}");
        
        // Configure client based on detected SKU capabilities
        var options = new CosmosClientOptions
        {
            ConnectionMode = _skuCapabilities.Tier == "Premium" ? 
                ConnectionMode.Direct : ConnectionMode.Gateway,
            MaxRetryAttemptsOnRateLimitedRequests = _skuCapabilities.MaxRetryAttempts,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(_skuCapabilities.MaxRetryWaitTimeSeconds)
        };
        
        _cosmosClient = new CosmosClient(connectionString, options);
    }
    
    public async Task ProcessLargeDataSetAsync(IEnumerable<DataItem> items)
    {
        var container = _cosmosClient.GetContainer("MyDatabase", "MyContainer");
        
        // Adjust batch size and processing strategy based on SKU capabilities
        if (_skuCapabilities.SupportsBulkProcessing)
        {
            _logger.LogInformation("Using bulk processing mode");
            
            // Premium tier: Use parallel bulk processing
            var tasks = items.Select(item => container.UpsertItemAsync(item));
            await Task.WhenAll(tasks);
        }
        else
        {
            _logger.LogInformation("Using sequential processing mode");
            
            // Basic/Standard tier: Process sequentially in smaller batches
            foreach (var batch in items.Chunk(_skuCapabilities.BatchSize))
            {
                foreach (var item in batch)
                {
                    await container.UpsertItemAsync(item);
                }
                
                // Add a delay between batches to avoid rate limiting on lower SKUs
                if (_skuCapabilities.RequiresThrottling)
                {
                    await Task.Delay(_skuCapabilities.ThrottlingDelayMs);
                }
            }
        }
    }
    
    private SKUCapabilities DetectSKUCapabilities(IConfiguration configuration)
    {
        // Read SKU from configuration or environment
        string skuTier = configuration["AzureSKU:Tier"] ?? "Standard";
        
        // Return capability set based on SKU tier
        return skuTier switch
        {
            "Premium" => new SKUCapabilities
            {
                Tier = "Premium",
                MaxParallelism = 100,
                SupportsBulkProcessing = true,
                BatchSize = 100,
                RequiresThrottling = false,
                MaxRetryAttempts = 9,
                MaxRetryWaitTimeSeconds = 30
            },
            "Standard" => new SKUCapabilities
            {
                Tier = "Standard",
                MaxParallelism = 20,
                SupportsBulkProcessing = false,
                BatchSize = 20,
                RequiresThrottling = true,
                ThrottlingDelayMs = 200,
                MaxRetryAttempts = 5,
                MaxRetryWaitTimeSeconds = 15
            },
            _ => new SKUCapabilities
            {
                Tier = "Basic",
                MaxParallelism = 5,
                SupportsBulkProcessing = false,
                BatchSize = 5,
                RequiresThrottling = true,
                ThrottlingDelayMs = 500,
                MaxRetryAttempts = 3,
                MaxRetryWaitTimeSeconds = 10
            }
        };
    }
}

// Support class to encapsulate SKU-specific capabilities
public class SKUCapabilities
{
    public string Tier { get; set; } = "Basic";
    public int MaxParallelism { get; set; } = 5;
    public bool SupportsBulkProcessing { get; set; } = false;
    public int BatchSize { get; set; } = 5;
    public bool RequiresThrottling { get; set; } = true;
    public int ThrottlingDelayMs { get; set; } = 500;
    public int MaxRetryAttempts { get; set; } = 3;
    public int MaxRetryWaitTimeSeconds { get; set; } = 10;
}
```

## Code Examples: Python

**Before pattern implementation**
```python
import azure.cosmos.cosmos_client as cosmos_client

class DataProcessor:
    def __init__(self, connection_string):
        # Hardcoded configuration assuming high-performance tier
        self.client = cosmos_client.CosmosClient.from_connection_string(
            connection_string,
            connection_policy={
                "RequestTimeout": 60,
                "MaxRetryAttempts": 9,
                "RetryOptions": {"MaxRetryAttemptsOnThrottledRequests": 9}
            }
        )
        
    def process_large_dataset(self, items):
        # Always assumes premium resources - may fail on lower SKUs
        container = self.client.get_database_client("MyDatabase").get_container_client("MyContainer")
        
        # Process all items in parallel - could overwhelm lower SKUs
        import concurrent.futures
        with concurrent.futures.ThreadPoolExecutor(max_workers=100) as executor:
            futures = [executor.submit(container.upsert_item, item) for item in items]
            concurrent.futures.wait(futures)
```

**Pattern implementation**
```python
import os
import time
import azure.cosmos.cosmos_client as cosmos_client
import logging

class SKUAwareDataProcessor:
    def __init__(self, connection_string, config=None):
        self.logger = logging.getLogger("DataProcessor")
        
        # Determine SKU and capabilities from configuration or environment
        self.sku_capabilities = self._detect_sku_capabilities(config)
        self.logger.info(f"Running on SKU: {self.sku_capabilities['tier']}, "
                         f"Parallelism: {self.sku_capabilities['max_parallelism']}")
        
        # Configure client based on detected SKU capabilities
        connection_policy = {
            "RequestTimeout": self.sku_capabilities["timeout_seconds"],
            "MaxRetryAttempts": self.sku_capabilities["max_retry_attempts"],
            "RetryOptions": {
                "MaxRetryAttemptsOnThrottledRequests": self.sku_capabilities["max_retry_attempts"]
            }
        }
        
        # Use direct mode only for premium SKU
        if self.sku_capabilities["tier"] == "Premium":
            connection_policy["ConnectionMode"] = 0  # Direct mode
        
        self.client = cosmos_client.CosmosClient.from_connection_string(
            connection_string,
            connection_policy=connection_policy
        )
        
    def process_large_dataset(self, items):
        container = self.client.get_database_client("MyDatabase").get_container_client("MyContainer")
        
        # Use different processing strategies based on SKU
        if self.sku_capabilities["supports_bulk_processing"]:
            self.logger.info("Using bulk processing mode")
            
            # Premium tier: Process in parallel
            import concurrent.futures
            with concurrent.futures.ThreadPoolExecutor(
                max_workers=self.sku_capabilities["max_parallelism"]
            ) as executor:
                futures = [executor.submit(container.upsert_item, item) for item in items]
                concurrent.futures.wait(futures)
        else:
            self.logger.info("Using sequential processing mode")
            
            # Basic/Standard tier: Process in batches with throttling
            batch_size = self.sku_capabilities["batch_size"]
            for i in range(0, len(items), batch_size):
                batch = items[i:i + batch_size]
                
                for item in batch:
                    container.upsert_item(item)
                
                # Add delay between batches for lower SKUs
                if self.sku_capabilities["requires_throttling"]:
                    time.sleep(self.sku_capabilities["throttling_delay_sec"])
    
    def _detect_sku_capabilities(self, config):
        # Get SKU from environment, config, or instance metadata
        sku_tier = os.environ.get("AZURE_SKU_TIER")
        
        if not sku_tier and config:
            sku_tier = config.get("azure_sku", {}).get("tier")
            
        if not sku_tier:
            # Default to Standard if not specified
            sku_tier = "Standard"
            
        # Define capabilities for each SKU tier
        if sku_tier == "Premium":
            return {
                "tier": "Premium",
                "max_parallelism": 100,
                "supports_bulk_processing": True,
                "batch_size": 100,
                "requires_throttling": False,
                "timeout_seconds": 60,
                "max_retry_attempts": 9
            }
        elif sku_tier == "Standard":
            return {
                "tier": "Standard",
                "max_parallelism": 20,
                "supports_bulk_processing": False,
                "batch_size": 20,
                "requires_throttling": True,
                "throttling_delay_sec": 0.2,
                "timeout_seconds": 30,
                "max_retry_attempts": 5
            }
        else:  # Basic
            return {
                "tier": "Basic",
                "max_parallelism": 5,
                "supports_bulk_processing": False,
                "batch_size": 5,
                "requires_throttling": True,
                "throttling_delay_sec": 0.5,
                "timeout_seconds": 15,
                "max_retry_attempts": 3
            }
```

## Related Patterns
- [Azure Architecture Center: Throttling Pattern](https://learn.microsoft.com/azure/architecture/patterns/throttling)
- [Azure Architecture Center: Health Endpoint Monitoring](https://learn.microsoft.com/azure/architecture/patterns/health-endpoint-monitoring)


