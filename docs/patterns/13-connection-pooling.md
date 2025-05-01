# Connection Pooling Pattern

## Overview
This pattern establishes a pool of connections that can be reused, helping reduce the overhead of repeatedly creating and destroying connections.

## Problem Statement
Creating a new connection for every request can be expensive and time-consuming. Frequent connection creation leads to performance bottlenecks and resource exhaustion.

## Solution
Maintain a centrally managed pool of reusable connections. When a request needs a connection, it can borrow one from the pool and return it once finished.

## When to Use
- Applications that frequently open and close database connections.
- High-traffic environments where managing connections on-demand becomes costly.
- Scenarios where resource consumption must be optimized across multiple requests.

## Benefits
- Reduces response times by eliminating connection setup overhead.
- Minimizes resource usage by reusing existing connections.
- Improves scalability and overall application performance.

## Code Samples: C#

**Before pattern implementation**
```csharp
public class CustomerService
{
    private SqlConnection _connection;

    public CustomerService()
    {
        // Persistent connection kept open throughout the service lifetime
        _connection = new SqlConnection("ConnectionInfo");
        _connection.Open();
    }

    public string GetCustomerName(int customerId)
    {
        SqlCommand cmd = new SqlCommand("SELECT Name FROM Customers WHERE Id = @Id", _connection);
        cmd.Parameters.AddWithValue("@Id", customerId);

        var result = cmd.ExecuteScalar();
        return result?.ToString();
    }

    ~CustomerService()
    {
        _connection?.Close();
    }
}

```

**Pattern implementation**

```csharp
public class CustomerService
{
    private readonly string _connectionString = "ConnectionString";

    public async Task<string> GetCustomerNameAsync(int customerId)
    {
        // Open and close a new SqlConnection each time — this enables connection pooling
        await using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            SqlCommand cmd = new SqlCommand("SELECT Name FROM Customers WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", customerId);

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }
    }
}

```

## Code Examples: Python

**Before pattern implementation**

```python
# ...existing code...
import psycopg2

def get_data():
    conn = psycopg2.connect("dbname=test user=postgres")
    cur = conn.cursor()
    # Execute queries
    # Close when done
    cur.close()
    conn.close()
```

**Pattern implementation**

```python
# ...existing code...
import psycopg2
from psycopg2 import pool

connection_pool = pool.SimpleConnectionPool(
    1, 5, database="test", user="postgres"
)

def get_data_pooled():
    conn = connection_pool.getconn()
    try:
        cur = conn.cursor()
        # Execute queries
        cur.close()
    finally:
        connection_pool.putconn(conn)
```


