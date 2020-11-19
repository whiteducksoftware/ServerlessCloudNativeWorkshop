using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace CollectCurrentWeather
{
    public static class Collector
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Collector")]
        [return: EventGrid(TopicEndpointUri = "WeatherDataTopicUri", TopicKeySetting = "WeatherDataTopicKey")]
        public static async Task<EventGridEvent> Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            var response = await httpClient.GetAsync("https://emulatorfunctionapp98f3eeca.azurewebsites.net/api/getstationdata?device_Id=70:ee:50:1b:26:ac&code=K3fQDcXMFOSPKGV1DM8JCzvmtyQtx6C4CG4Ba6Xe1rpN9higlU5S3Q==");
            return new EventGridEvent(Guid.NewGuid().ToString(), "weather", await response.Content.ReadAsStringAsync(), "devopenspace.serverless.weather", DateTime.UtcNow, "1.0");
        }
    }
}
