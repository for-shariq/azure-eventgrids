using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EmployeeSubscriber
{
    public static class NewEmployeeHandler
    {
        public class GridEvent<T> where T : class
        {
            public string Id { get; set; }
            public string EventType { get; set; }
            public string Subject { get; set; }
            public DateTime EventTime { get; set; }
            public T Data { get; set; }
            public string Topic { get; set; }
        }

        [FunctionName("newemployeehandler")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("New Employee Handler Triggered");

                string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            var gridEvent = JsonConvert.DeserializeObject<List<GridEvent<Dictionary<string, string>>>>(requestBody)    ;

            //Check to see if the event is available
            if (gridEvent == null)
            {
                return new BadRequestObjectResult(new { Message = $"Missing event details", Type = "Error" });
            }

            // Check the header to identify the type of request
            // from Event Grid. A subscription validation request
            // must echo back the validation code.

            req.Headers.TryGetValue("Aeg-Event-Type", out var gridEventType);
            var grdEvent = gridEvent[0];
            if (gridEventType == "SubscriptionValidation")
            {
                var code = grdEvent.Data["validationCode"];
                return new OkObjectResult(new { validationResponse = code, Message = $"Validation Event", Type = "ValidationCode" });
            }
            else if (gridEventType == "Notification")
            {
                // Pseudo code: place message into a queue
                // for further processing.
                return new OkObjectResult(new { Message = $"New Employee added to queue", Type = "Success" });
            }
            else
            {
                return new BadRequestObjectResult($"Unknown request type");
            }

            //return new BadRequestObjectResult($"Unknown request type");
        }
    }
}
