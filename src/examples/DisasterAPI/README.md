# DisasterAPI

## Purpose

This repository demonstrates API anti-patterns that static code analysis tools often miss.

## Core Issues

1. **Nested Exception Handling**: Multiple try/catch layers spread across different classes
2. **Hidden Architecture Problems**: Classes that look fine individually but create tangled dependencies
3. **Async/Concurrency Issues**: Improper thread handling and race conditions 
4. **Naming Violations**: Async methods without proper naming conventions
5. **Transient Service Leaks**: Services registered as transient that accumulate state or allocate large buffers

## Why Static Analysis Misses These Problems

Static code analyzers struggle with these issues because:

1. **Cross-Class Problems**: Issues span multiple classes but analyzers often check one class at a time
2. **Runtime Behavior**: Race conditions only appear during execution, not in static code
3. **Semantic Understanding**: Tools lack understanding of the business logic context
4. **Distributed Logic**: Problems emerge from the interaction of components that individually follow patterns
5. **DI Container Behavior**: The relationship between service lifetime (transient vs singleton) and memory allocation isn't visible to static analysis

## Examples

- **Exception Flow**: Try/catch blocks in different classes create a cascade that's hard to trace statically
- **Thread Safety**: Dictionary access without locks appears valid in isolation but fails under concurrent use
- **Task Misuse**: Fire-and-forget tasks look syntactically correct but cause unpredictable behavior
- **Service Lifetime Issues**: Transient services that hold state or allocate large buffers cause memory leaks when resolved frequently in high-traffic scenarios

## Service Lifetime Problems

One particularly sneaky issue is how innocent-looking transient services can cause server crashes:
- A service that looks properly implemented but allocates 1MB+ per instance
- When registered as transient, each request creates a new instance
- At low traffic, no issues appear
- At scale (100+ requests/second), memory quickly exhausts and the application crashes
- Static analysis tools can't identify this because they don't simulate load patterns

## Warning

This code demonstrates deliberate anti-patterns for educational purposes.
