# Managing Connection Strings and Secrets

## Overview
The connection strings and secrets management pattern focuses on securely storing, accessing, and rotating sensitive configuration data such as database connection strings, API keys, and authentication credentials. This pattern ensures that secrets aren't exposed in source code, configuration files, or logs, reducing the risk of unauthorized access or data breaches.

## Problem Statement
Hard-coding or insecurely storing connection strings and secrets in application code or configuration files creates security vulnerabilities. These credentials can be accidentally committed to source control, exposed in logs, or shared in error. Additionally, rotating or updating credentials becomes difficult when they're scattered across the application.

## Solution
Use dedicated secret management services and follow secure practices to store, retrieve, and refresh sensitive information. Externalize all secrets from the application code and configuration files by replacing them with references to securely stored secrets. Implement automated rotation and secure retrieval mechanisms to minimize exposure.

## When to Use
- When your application connects to databases, external APIs, or services requiring authentication
- To implement a consistent approach to secrets management across your organization
- When deploying applications to production environments where security is critical
- To comply with security standards and compliance requirements

## Benefits
- Prevents accidental exposure of sensitive credentials
- Simplifies secret rotation and updates
- Enables centralized access control and auditing
- Reduces the risk of credential leakage or theft
- Facilitates compliance with security standards

## Code Samples: C#

**Before pattern implementation**
```csharp
public class DatabaseService
{
    // Hardcoded connection string - security vulnerability
    private readonly string _connectionString = 
        "Server=myserver.database.windows.net;Database=mydb;User ID=admin;Password=SuperSecret123!;";
    
    public async Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        // Query database
        return await connection.QueryAsync<Customer>("SELECT * FROM Customers");
    }
}
```

**Pattern implementation**
```csharp
// Program.cs
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

// Set up Key Vault integration during application startup
public static void ConfigureSecrets(WebApplicationBuilder builder)
{
    // Get Key Vault information from environment variables or app settings
    string keyVaultUri = builder.Configuration["KeyVault:Uri"];
    
    if (string.IsNullOrEmpty(keyVaultUri))
    {
        throw new InvalidOperationException("Key Vault URI is not configured");
    }
    
    // Add Key Vault as a configuration source
    // This uses Managed Identity or DefaultAzureCredential for authentication
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        new DefaultAzureCredential());
        
    Console.WriteLine("Key Vault configuration loaded successfully");
}

// Usage in Startup/Program.cs
var builder = WebApplication.CreateBuilder(args);
ConfigureSecrets(builder);
// Configuration now has secrets from Key Vault available

// Database Service
public class DatabaseService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseService> _logger;
    
    // Inject configuration that includes Key Vault secrets
    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        // The key is the name of the secret in Key Vault
        _connectionString = configuration["database-connection-string"];
        _logger = logger;
        
        if (string.IsNullOrEmpty(_connectionString))
        {
            _logger.LogError("Database connection string is missing from Key Vault");
            throw new InvalidOperationException("Missing connection string");
        }
    }
    
    public async Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            // Query database
            return await connection.QueryAsync<Customer>("SELECT * FROM Customers");
        }
        catch (Exception ex)
        {
            // Log the exception but NEVER include the connection string
            _logger.LogError(ex, "Failed to query customers");
            throw;
        }
    }
}
```

## Code Examples: Python

**Before pattern implementation**
```python
def get_customers():
    # Hardcoded connection string - security vulnerability
    conn_string = "postgresql://admin:SuperSecret123!@myserver.postgres.database.azure.com:5432/mydb"
    
    connection = psycopg2.connect(conn_string)
    cursor = connection.cursor()
    cursor.execute("SELECT * FROM customers")
    return cursor.fetchall()
```

**Pattern implementation**
```python
import os
from azure.identity import DefaultAzureCredential
from azure.keyvault.secrets import SecretClient

# Load secrets once at startup
def initialize_secrets():
    # Use environment variables for configuration
    key_vault_url = os.environ.get("KEY_VAULT_URL")
    
    if not key_vault_url:
        raise ValueError("KEY_VAULT_URL environment variable is required")
    
    # Use managed identity or service principal for authentication
    credential = DefaultAzureCredential()
    secret_client = SecretClient(vault_url=key_vault_url, credential=credential)
    
    # Fetch the secret from Key Vault
    db_secret = secret_client.get_secret("database-connection-string")
    
    # Store in a global but not directly accessible location
    os.environ["DB_CONNECTION_STRING"] = db_secret.value
    
    print("Secrets initialized successfully")
    
def get_customers():
    # Get connection string from environment (loaded securely at startup)
    conn_string = os.environ.get("DB_CONNECTION_STRING")
    
    if not conn_string:
        raise ValueError("Database connection string not initialized")
        
    try:
        connection = psycopg2.connect(conn_string)
        cursor = connection.cursor()
        cursor.execute("SELECT * FROM customers")
        return cursor.fetchall()
    except Exception as e:
        # Log the error but NEVER include the connection string
        print(f"Database error: {str(e)}")
        raise
```

## Related Patterns
- [Azure Architecture Center: Key Vault](https://learn.microsoft.com/azure/key-vault/general/basic-concepts)
- [Azure Architecture Center: Valet Key Pattern](https://learn.microsoft.com/azure/architecture/patterns/valet-key)


