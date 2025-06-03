# Strangler Fig

## Overview
The strangler fig pattern enables incremental migration of legacy systems by gradually replacing specific pieces of functionality with new services or components. Over time, the legacy system is "strangled" as more features are redirected to the new implementation, reducing risk and allowing for continuous delivery.

## Problem Statement
Rewriting or replacing a legacy system in a single step is risky, expensive, and often impractical. Large-scale migrations can lead to long development cycles, business disruption, and increased chances of failure.

## Solution
Introduce a facade or routing layer that intercepts requests. Initially, all requests are handled by the legacy system. As new components are developed, the routing layer redirects relevant requests to the new implementation. This allows for gradual migration, testing, and rollback if needed, while the legacy system continues to operate.

## When to Use
- When migrating a monolithic or legacy application to a new architecture
- When you need to minimize risk and disruption during migration
- When incremental delivery and validation are required

## Benefits
- Reduces migration risk by allowing incremental replacement
- Enables continuous delivery and validation
- Minimizes business disruption

## Code Samples: C#

**Before pattern implementation**
```csharp
public class OrderController : Controller
{
    public IActionResult GetOrder(int id)
    {
        // All logic handled by legacy system
        return LegacyOrderSystem.GetOrder(id);
    }
}
```

**Pattern implementation**
```csharp
public class OrderController : Controller
{
    public IActionResult GetOrder(int id)
    {
        if (IsMigratedOrder(id))
        {
            // Route to new service
            return NewOrderService.GetOrder(id);
        }
        else
        {
            // Fallback to legacy system
            return LegacyOrderSystem.GetOrder(id);
        }
    }
    private bool IsMigratedOrder(int id)
    {
        // Logic to determine if order is handled by new system
        return id > 1000;
    }
}
```

## Code Examples: Python

**Before pattern implementation**
```python
def get_order(order_id):
    # All logic handled by legacy system
    return legacy_order_system.get_order(order_id)
```

**Pattern implementation**
```python
def get_order(order_id):
    if is_migrated_order(order_id):
        # Route to new service
        return new_order_service.get_order(order_id)
    else:
        # Fallback to legacy system
        return legacy_order_system.get_order(order_id)

def is_migrated_order(order_id):
    # Logic to determine if order is handled by new system
    return order_id > 1000
```

## Related Patterns
- [Azure Architecture Center: Strangler Fig Pattern](https://learn.microsoft.com/azure/architecture/patterns/strangler-fig)


