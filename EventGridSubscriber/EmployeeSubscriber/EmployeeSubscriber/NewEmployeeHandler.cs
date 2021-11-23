using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;

namespace EmployeeSubscriber
{
    public static class NewEmployeeHandler
    {
        public class Employee
        {
            public int Id { get; set; }
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }           
            public string Email { get; set; }
        }

        [FunctionName("newemployeehandler")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function begun");
            string response = string.Empty;
            const string CustomTopicEvent = "https://hrapplicationtopic.eastus-1.eventgrid.azure.net/api/events";

            string requestContent = await req.Content.ReadAsStringAsync();
            log.LogInformation($"Received events: {requestContent}");

            EventGridSubscriber eventGridSubscriber = new EventGridSubscriber();
            eventGridSubscriber.AddOrUpdateCustomEventMapping(CustomTopicEvent, typeof(Employee));
            EventGridEvent[] eventGridEvents = eventGridSubscriber.DeserializeEventGridEvents(requestContent);

            foreach (EventGridEvent eventGridEvent in eventGridEvents)
            {
                if (eventGridEvent.Data is SubscriptionValidationEventData)
                {
                    var eventData = (SubscriptionValidationEventData)eventGridEvent.Data;
                    log.LogInformation($"Got SubscriptionValidation event data, validationCode: {eventData.ValidationCode},  validationUrl: {eventData.ValidationUrl}, topic: {eventGridEvent.Topic}, eventType: { eventGridEvent.EventType}");
                    // Do any additional validation (as required) such as validating that the Azure resource ID of the topic matches
                    // the expected topic and then return back the below response
                    var responseData = new SubscriptionValidationResponse()
                    {
                        ValidationResponse = eventData.ValidationCode
                    };
                    ///log.LogInformation($"Echo response: {JsonConvert.SerializeObject(req.CreateResponse(HttpStatusCode.OK, responseData))}");
                    //return req.CreateResponse(HttpStatusCode.OK, responseData);
                    return req.CreateResponse(HttpStatusCode.OK, new { validationResponse = responseData.ValidationResponse },
                 new JsonMediaTypeFormatter());
                }
                else if (eventGridEvent.Data is StorageBlobCreatedEventData)
                {
                    var eventData = (StorageBlobCreatedEventData)eventGridEvent.Data;
                    log.LogInformation($"Got BlobCreated event data, blob URI {eventData.Url}");
                }
                else if (eventGridEvent.Data is Employee)
                {
                    var eventData = (Employee)eventGridEvent.Data;
                    log.LogInformation($"Got Employee event data {eventData.Name}");
                }
            }

            return req.CreateResponse(HttpStatusCode.OK, response);
        }
    }
}
