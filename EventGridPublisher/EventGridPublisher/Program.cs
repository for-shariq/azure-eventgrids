using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.IO;

namespace EventGridPublisher
{
    public class ApplicationSettings
    {
        public string TopicEndpoint { get; set; }
        public string TopicId { get; set; }
        public string TopicName { get; set; }

    }
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) ||
                                devEnvironmentVariable.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsetting.json", optional: true, reloadOnChange: true);
              
            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }
            Configuration = builder.Build();

            var topicEndpoint = Configuration["ApplicationSettings:TopicEndpoint"];

            string topicKey = Configuration["ApplicationSettings:TopicKey"];
            string topicHostname = new Uri(topicEndpoint).Host;
            TopicCredentials topicCredentials = new TopicCredentials(topicKey);
            EventGridClient client = new EventGridClient(topicCredentials);

            client.PublishEventsAsync(topicHostname, GetEventsList()).GetAwaiter().GetResult();
            Console.Write("Published events to Event Grid topic.");
            Console.ReadLine();
        }
        static IList<EventGridEvent> GetEventsList()
        {
            List<EventGridEvent> eventsList = new List<EventGridEvent>();

            for (int i = 0; i < 1; i++)
            {
                eventsList.Add(new EventGridEvent()
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = "EmployeeAdded",
                    Data = new Employee()
                    {
                        Id = 1,
                        Name = "Shariq Nasir",
                        Email = "forshariq@gmail.com"
                    },
                    EventTime = DateTime.Now,
                    Subject = "Department/Technical",
                    DataVersion = "2.0"
                });
            }

            return eventsList;
        }
    }
}
