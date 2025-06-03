# Graceful Error Logging & Propagation

## Overview
The graceful error handling pattern focuses on properly capturing, logging, and responding to errors in a way that maintains system stability, provides actionable information for troubleshooting, and delivers appropriate feedback to users. This pattern ensures that errors are neither silently swallowed nor allowed to crash the entire application, while also preserving security by not exposing sensitive information.

## Problem Statement
Improper error handling can result in several problems: applications crash unexpectedly, sensitive information is leaked to users, errors aren't logged properly for debugging, and upstream systems receive unhelpful error messages. Additionally, poor error handling often leaves systems in inconsistent states where resources aren't properly cleaned up, leading to memory leaks or deadlocks.

## Solution
Implement consistent error handling throughout your application with a multi-layered approach: catch errors at appropriate boundaries, log detailed information for operations teams (without sensitive data), transform technical exceptions into user-friendly messages where needed, and ensure proper resource cleanup. Distinguish between expected errors (which may be part of normal flow) and unexpected errors (which indicate bugs or system failures).

## When to Use
- In any application with external dependencies
- For user-facing applications requiring friendly error messages
- In systems requiring detailed error telemetry
- When different handling strategies are needed for different error types
- In distributed systems where error context can be lost across boundaries

## Benefits
- Improves system resilience and stability
- Provides better diagnostic information for troubleshooting
- Enhances user experience during error situations
- Prevents information leakage of sensitive data
- Ensures consistent resource cleanup and system state

## Code Samples: C#

**Before pattern implementation**
```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public User GetUserById(int id)
    {
        // No error handling - throws raw exceptions
        return _userRepository.GetById(id);
    }
    
    public void UpdateUserEmail(int id, string email)
    {
        // Improper error handling - catches Exception but doesn't log
        try
        {
            var user = _userRepository.GetById(id);
            user.Email = email;
            _userRepository.Update(user);
        }
        catch (Exception)
        {
            // Swallows the exception, losing valuable information
        }
    }
}
```

**Pattern implementation**
```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public User GetUserById(int id)
    {
        try
        {
            var user = _userRepository.GetById(id);
            
            if (user == null)
            {
                // Log with appropriate level for expected "not found" case
                _logger.LogInformation("User with ID {UserId} not found", id);
                throw new NotFoundException($"User with ID {id} not found");
            }
            
            return user;
        }
        catch (NotFoundException)
        {
            // Rethrow expected exceptions without wrapping
            throw;
        }
        catch (DbException ex)
        {
            // Log database exceptions with technical details but not sensitive data
            _logger.LogError(ex, "Database error occurred retrieving user {UserId}", id);
            
            // Transform to application-specific exception to hide implementation details
            throw new ServiceException("Unable to retrieve user information", ex);
        }
        catch (Exception ex)
        {
            // Log unexpected exceptions with full context
            _logger.LogError(ex, "Unexpected error retrieving user {UserId}", id);
            throw new ServiceException("An unexpected error occurred", ex);
        }
    }
    
    public void UpdateUserEmail(int id, string email)
    {
        // Input validation
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            throw new ValidationException("Invalid email format");
        }
        
        try
        {
            var user = _userRepository.GetById(id) ?? 
                throw new NotFoundException($"User with ID {id} not found");
                
            user.Email = email;
            _userRepository.Update(user);
            
            _logger.LogInformation("Email updated for user {UserId}", id);
        }
        catch (NotFoundException ex)
        {
            // Log but don't wrap expected exceptions
            _logger.LogInformation(ex.Message);
            throw;
        }
        catch (DbException ex) when (IsDuplicateKeyError(ex))
        {
            // Special handling for specific database error
            _logger.LogWarning(ex, "Email {Email} already in use", email);
            throw new ValidationException("This email address is already in use", ex);
        }
        catch (Exception ex)
        {
            // For all other exceptions, log with correlation ID for troubleshooting
            var correlationId = Guid.NewGuid().ToString();
            _logger.LogError(ex, "Failed to update email for user {UserId}. Correlation ID: {CorrelationId}", 
                id, correlationId);
                
            // Return user-friendly message with correlation ID for support
            throw new ServiceException(
                $"Unable to update email. Please contact support with code: {correlationId}", ex);
        }
    }
    
    private bool IsDuplicateKeyError(DbException ex)
    {
        // Implementation depends on database provider
        return ex.Message.Contains("duplicate key") || ex.ErrorCode == 2601;
    }
}

// Custom exception types
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception inner) : base(message, inner) { }
}

public class ServiceException : Exception
{
    public ServiceException(string message) : base(message) { }
    public ServiceException(string message, Exception inner) : base(message, inner) { }
}
```

## Code Examples: Python

**Before pattern implementation**
```python
class UserService:
    def __init__(self, user_repository):
        self.user_repository = user_repository
    
    def get_user_by_id(self, user_id):
        # No error handling
        return self.user_repository.get_by_id(user_id)
    
    def update_user_email(self, user_id, email):
        try:
            user = self.user_repository.get_by_id(user_id)
            user.email = email
            self.user_repository.update(user)
        except Exception:
            # Bad practice: swallowing exception
            pass
```

**Pattern implementation**
```python
import logging
import uuid
from typing import Optional

class NotFoundException(Exception):
    """Raised when a resource is not found"""
    pass

class ValidationException(Exception):
    """Raised when input validation fails"""
    pass

class ServiceException(Exception):
    """Generic service layer exception"""
    pass

class UserService:
    def __init__(self, user_repository):
        self.user_repository = user_repository
        self.logger = logging.getLogger(__name__)
    
    def get_user_by_id(self, user_id) -> Optional[dict]:
        try:
            user = self.user_repository.get_by_id(user_id)
            
            if user is None:
                # Log with appropriate level for expected "not found" case
                self.logger.info(f"User with ID {user_id} not found")
                raise NotFoundException(f"User with ID {user_id} not found")
                
            return user
            
        except NotFoundException:
            # Re-raise expected exceptions
            raise
            
        except Exception as e:
            # Log unexpected exceptions with full context
            self.logger.exception(f"Error retrieving user {user_id}: {str(e)}")
            
            # Transform to application-specific exception
            raise ServiceException("Unable to retrieve user information") from e
    
    def update_user_email(self, user_id, email):
        # Input validation
        if not email or '@' not in email:
            raise ValidationException("Invalid email format")
            
        try:
            user = self.user_repository.get_by_id(user_id)
            
            if user is None:
                raise NotFoundException(f"User with ID {user_id} not found")
                
            user.email = email
            self.user_repository.update(user)
            
            self.logger.info(f"Email updated for user {user_id}")
            
        except NotFoundException as e:
            # Log but don't wrap expected exceptions
            self.logger.info(str(e))
            raise
            
        except Exception as e:
            # For other exceptions, log with correlation ID
            correlation_id = str(uuid.uuid4())
            self.logger.exception(
                f"Failed to update email for user {user_id}. "
                f"Correlation ID: {correlation_id}"
            )
            
            # Return user-friendly message with correlation ID
            raise ServiceException(
                f"Unable to update email. Please contact support with code: {correlation_id}"
            ) from e
```

## Related Patterns
- [Azure Architecture Center: Retry Pattern](https://learn.microsoft.com/azure/architecture/patterns/retry)
- [Azure Architecture Center: Circuit Breaker Pattern](https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker)
- [Azure Architecture Center: Gateway Routing Pattern](https://learn.microsoft.com/azure/architecture/patterns/gateway-routing)


