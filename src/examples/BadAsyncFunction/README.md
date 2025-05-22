## Bad Async Pattern Sample

The sample application demonstrates a bad async pattern. The application is designed to leave some operations running in the background (call to log) and cause possible failures in the main thread. This is a bad practice and should be avoided in production code.

## Deploy the Application

Deploy the application to an exiting Azure Function App running on an Elastic Premium plan.

## Load Test

Use Azure load testing to run a load test against the function app. A 15 minute test with as much load as possible should be enough to make the issue surface.

## Check the Bad Async Pattern detector

Run the Bad Async Pattern detector to see if the issue is detected. If the issue has surface you'll get the list of invocations IDs that are affected by the issue.

Run the following query to understand what function caused the issue:

```kql
union traces | union exceptions| where customDimensions['InvocationId'] == '<operation id>'| order by timestamp asc
```
