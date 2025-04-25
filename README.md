# Project

Reliability is a major requirement for any application. Building reliable cloud applications is achieved through _resiliency_, which should be implemented at every level of the infrastructure and application. Plenty of [guidelines](https://learn.microsoft.com/azure/well-architected/reliability/principles) exist on how to select the right services and configuration, but writing resilient code is just as essential. After all, there can be no weak links in the stack: Badly written code can easily bring an otherwise well architected application down. 

This project aims to showcase several common anti-patterns in software, encountered in real-life cloud applications around the world. Not only do we provide an explanation, we also include code samples of what (not) to do.

This has been a community effort to create and we would love to see additional contributions to grow the collection of resilient coding patterns.

# The Patterns

The following lists shows several common patterns in cloud applications. Each pattern is linked to its dedicated section that provides a comprehensive description, code samples, and links to sample artifacts that implement the pattern. 

## 01 - The Retry	Pattern
## 02 - The Circuit Breaker	Pattern
## 03 - The Cache Aside	Pattern
## 04 - The Queue Based Load Leveling Pattern
## 05 - The Strangler Fig Pattern
## 06 - The Bulkhead Pattern
## 07 - Scaling to different SKUs
## 08 - Managing Connection Strings and Secrets
## 09 - Handing Timeouts
## 10 - Graceful Error Logging & propagation
## 11 - Fallback Mechanisms


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
