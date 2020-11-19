using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;

namespace Final
{
    public static class EventCollector
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("EventCollector")]
        [return: EventGrid(TopicEndpointUri = "WeatherDataTopicUri", TopicKeySetting = "WeatherDataTopicKey")]
        public static async Task<EventGridEvent> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var response = await httpClient.GetAsync("https://emulatorfunctionapp98f3eeca.azurewebsites.net/api/getstationdata?device_Id=70:ee:50:1b:26:ac&code=K3fQDcXMFOSPKGV1DM8JCzvmtyQtx6C4CG4Ba6Xe1rpN9higlU5S3Q==");
            return new EventGridEvent(Guid.NewGuid().ToString(), "wheather", await response.Content.ReadAsStringAsync(), "devopenspace.serverless.weather", DateTime.UtcNow, "1.0");
        }
    }
}
