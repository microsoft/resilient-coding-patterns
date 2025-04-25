# Project

Reliability is a major requirement for any application. Building reliable cloud applications is achieved through _resiliency_, which should be implemented at every level of the infrastructure and application. Plenty of [guidelines](https://learn.microsoft.com/azure/well-architected/reliability/principles) exist on how to select the right services and configuration, but writing resilient code is just as essential. After all, there can be no weak links in the stack: Badly written code can easily bring an otherwise well architected application down. 

This project aims to showcase several common anti-patterns in software, encountered in real-life cloud applications around the world. Not only do we provide an explanation, we also include code samples of what (not) to do.

This has been a community effort to create and we would love to see additional contributions to grow the collection of resilient coding patterns.

# The Patterns

The following lists shows several common patterns in cloud applications. Each pattern is linked to its dedicated section that provides a comprehensive description, code samples, and links to sample artifacts that implement the pattern. 

## [01 - The Retry Pattern](./docs/patterns/01-retry.md)

Retry patterns are of key importance in cloud applications. Because of the size and scale of applications and infrastructure, transient network errors occur frequently. This means that connections to dependencies must be re-established effectively in case they are lost.

## 02 - The Circuit Breaker Pattern

The Circuit Breaker pattern is used to detect failures and encapsulate the logic of preventing a failure from constantly recurring during maintenance, temporary unavailability of dependencies, or other unexpected difficulties. By "breaking the circuit," the application can fail fast and avoid making requests that are likely to fail, allowing time for the external system to recover.

## 03 - The Cache Aside Pattern

The Cache Aside pattern improves performance and scalability by loading data into a cache only when necessary. Applications retrieve data from the cache if available; otherwise, they fetch it from the data store and populate the cache for subsequent requests, ensuring that frequently accessed data is quickly available.

## 04 - The Queue Based Load Leveling Pattern

The Queue Based Load Leveling pattern uses a queue to buffer requests between a task and a service that processes those requests. This helps to smooth out intermittent heavy loads, prevent service overload, and improve the reliability and scalability of the application.

## 05 - The Strangler Fig Pattern

The Strangler Fig pattern enables incremental migration of legacy systems by gradually replacing specific pieces of functionality with new services or components. Over time, the old system is "strangled" as more features are redirected to the new implementation, reducing risk and downtime.

## 06 - The Bulkhead Pattern

The Bulkhead pattern isolates critical resources, such as connections or threads, into separate pools so that if one pool fails, the others continue to function. This containment prevents failures in one part of the system from cascading and affecting the entire application.

## 07 - Scaling to different SKUs

Scaling to different SKUs involves designing applications to run efficiently across various hardware or service tiers, allowing for cost optimization and flexibility. This pattern ensures that the application can adapt to different performance and capacity requirements without significant changes.

## 08 - Managing Connection Strings and Secrets

Managing Connection Strings and Secrets focuses on securely storing and accessing sensitive configuration data, such as database connection strings and API keys. This pattern emphasizes the use of secure storage solutions and best practices to prevent accidental exposure and unauthorized access.

## 09 - Handling Timeouts

Handling Timeouts is about setting appropriate time limits for operations that interact with external resources or services. By defining timeouts, applications can avoid waiting indefinitely for responses, improve reliability, and provide better user experiences during transient failures.

## 10 - Graceful Error Logging & Propagation

Graceful Error Logging & Propagation ensures that errors are captured, logged, and communicated in a way that aids troubleshooting without exposing sensitive information. This pattern helps maintain application stability and provides actionable insights for developers and operators.

## 11 - Fallback Mechanisms

Fallback Mechanisms provide alternative actions or responses when a primary operation fails. By implementing fallbacks, applications can maintain functionality or degrade gracefully, improving resilience and user experience during partial outages or failures.

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
