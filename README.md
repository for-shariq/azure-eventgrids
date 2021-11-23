# Azure Event Driven Architecture
Azure Event Grid, .Net Core, Logic Apps

### 1. Creating a Resource Group
```console
az group create -l eastus -n lrn-eventgrid
```
### 2. Create a Custom Topic
After the resource group is create, an Event Grid Topic is provisioned. This will provide an endpoint
to publish custom events from the application. The name of the topic must be unique to the region, as
it will be publically accessible service on Azure.

> * Azure Event Grid is the distribution fabric for discrete “business logic activity” events that stand alone and are valuable outside of a stream context. Because those   events are not as strongly correlated and also don’t require processing in batches, the model for how those events are being dispatched for processing is very different.
> First assumption made is that there is a very large number of different event for different contexts emitted by the application.
> The second assumption is that independent events can generally be processed in a highly parallelized fashion using Web service calls or “serverless” functions. The most efficient model for dispatching events to those handlers is to “push” them out, and have the existing auto-scaling capabilities of the Web site, Azure Functions, or Azure Logic Apps manage the required processing capacity. If Azure Event Grid gets errors indicating that the target is too busy, it will back off for a little time, which allows for more resources to be spun up.
> * Azure Service Bus is the “Swiss Army Knife” service for all other generic messaging tasks. While Azure Event Grid and Azure Event Hubs have a razor-sharp focus on the collection and distribution of events at great scale, and with great velocity, an Azure Service Bus namespace is a host for queues holding jobs of critical business value. It allows for the creation of routes for messages that need to travel between applications and application modules. It is a solid platform for workflow and transaction handling and has robust facilities for dealing with many application fault conditions.

```console
az eventgrid topic create --name HrApplicationTopic \
  --location eastus \
  --resource-group lrn-eventgrid
```
After executing the command to create the topic, details about the resource will be returned. The output will look similar to, but not exactly like, the code here:

```javascript
{
  "endpoint": "https://<topic name>.eastus-1.eventgrid.azure.net/api/events",
  "extendedLocation": null,
  "id": "/subscriptions/xxxxxxx-xxxx-xxxx-xxxx-xxxxxx/resourceGroups/<resource group name>/providers/Microsoft.EventGrid/topics/<topic name>",
  "identity": {
    "principalId": null,
    "tenantId": null,
    "type": "None",
    "userAssignedIdentities": null
  },
  "inboundIpRules": null,
  "inputSchema": "EventGridSchema",
  "inputSchemaMapping": null,
  "kind": "Azure",
  "location": "eastus",
  "metricResourceId": "xxxxxx-xxx-xxxx-x-xxxxx",
  "name": "<Topic Name>",
  "privateEndpointConnections": null,
  "provisioningState": "Succeeded",
  "publicNetworkAccess": "Enabled",
  "resourceGroup": "<resouce group name>",
  "sku": {
    "name": "Basic"
  },
  "systemData": null,
  "tags": null,
  "type": "Microsoft.EventGrid/topics"
}
```
make note of endpoint.

### 3. Get Keys of Topic
You’ll also need one of the two access keys that were generated for authorization. To retrieve the keys, you can list the ones associated with the topic.
```console
az eventgrid topic key list --name HrApplicationTopic --resouce-group <resource-group-name>
```

### 4. Publishing an Event
Before sending the first event, you need to understand the event schema that’s expected by the topic. Each event, regardless of if the publisher is an Azure resource or custom application, will adhere to the structure outlined in the following code.
```javascript
[
  {
    "topic": string,
    "subject": string,   
    "id": string,
    "eventType": string,
    "eventTime": string,
    "data":{
      object-unique-to-each-publisher
    }
  }
]
```
<b>eventType</b> is a value used to uniquely identify the published event type. This property can be used by handlers wishing to subscribe only to specific event types, rather than all types.
<b>subject</b> is a value, like eventType, that’s available to provide additional context about the event, with the option of also providing an additional filter to subscribers
<b>data</b> is a publisher-defined bucket that’s simply an object that can contain one or more properties

> To publish the event, you can use Postman (or a similar tool) to simulate the message coming from the application to the endpoint address. For authorization, you can add an item in the header called aeg-sas-key—it’s value is one of the access keys generated when the topic is created. The body of the request will contain the payload.

> EventPublisher folder contains the console app to generate the Events.

### 5. Event Subscriber - An Azure Function
Our first handler wil lbe an Azure Function. I want to specifically subscribe to events for recently added employees. Additionally, and just as important, this handler must only be invoked for employees that belong to the engineering department.
> Please refer to EmployeeSubscriber > NewEmployeeHandler.cs file to check the code of Azure Function
Event Grid will send to its subscribers two types of requests—SubscriptionValidation and Notification—that you can identify by inspecting a value from the header. The validation request is important to ensure that all subscribers are added explicitly.  Validation requests can also be identified by their event type: Microsoft.EventGrid.SubscriptionValidationEvent. If the event type is Notification, then I proceed with the implementation of the business logic. This defensive programming approach is highly recommended when exposing endpoints to other services.

```c#
var code = gridEvent.Data["validationCode"];
return req.CreateResponse(HttpStatusCode.OK,
  new { validationResponse = code });
```
### 6. Create Event Subscription in Event Grid

```console
az eventgrid event-subscription create --name EmployeeAdded-Subscription \
 --source-resource-id /subscriptions/{SubID}/resourceGroups/{RG}/providers/Microsoft.EventGrid/topics/topic1 \
 --endpoint https://<function-name>.azurewebsites.net/api/<url> \
 --subject-ends-with Engineering \
 --included-event-types EmployeeAdded EmployeeDeleted
```

### 7. Handling Events: Logic App and WebHook
The next event subscription is a Logic App. Like the Azure Function example, it’s only interested in the added employee event type. It won’t leverage the prefix or suffix filters, because I want to send a message to employees from all departments. 
The Complete version of Logic app is shown below:
![alt text](https://github.com/for-shariq/azure-eventgrids/blob/main/Docs/LogicApp.png?raw=true)
