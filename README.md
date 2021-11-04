# Azure Event Driven Architecture
Azure Event Grid, .Net Core, Logic Apps

### 1. Creating a Resource Group
```console
az group create -l eastus -n Lrn-EventGrid
```
### 2. Create a Custom Topic
After the resource group is create, an Event Grid Topic is provisioned. This will provide an endpoint
to publish custom events from the application. The name of the topic must be unique to the region, as
it will be publically accessible service on Azure.

> * Azure Event Grid is the distribution fabric for discrete “business logic activity” events that stand alone and are valuable outside of a stream context. Because those   events are not as strongly correlated and also don’t require processing in batches, the model for how those events are being dispatched for processing is very different.
> First assumption made is that there is a very large number of different event for different contexts emitted by the application.
> The second assumption is that independent events can generally be processed in a highly parallelized fashion using Web service calls or “serverless” functions. The most efficient model for dispatching events to those handlers is to “push” them out, and have the existing auto-scaling capabilities of the Web site, Azure Functions, or Azure Logic Apps manage the required processing capacity. If Azure Event Grid gets errors indicating that the target is too busy, it will back off for a little time, which allows for more resources to be spun up.
> * Azure Service Bus is the “Swiss Army Knife” service for all other generic messaging tasks. While Azure Event Grid and Azure Event Hubs have a razor-sharp focus on the collection and distribution of events at great scale, and with great velocity, an Azure Service Bus namespace is a host for queues holding jobs of critical business value. It allows for the creation of routes for messages that need to travel between applications and application modules. It is a solid platform for workflow and transaction handling and has robust facilities for dealing with many application fault conditions.