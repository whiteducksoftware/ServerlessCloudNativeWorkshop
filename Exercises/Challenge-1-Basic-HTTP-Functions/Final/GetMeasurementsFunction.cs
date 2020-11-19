using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ServerlessWorkshop.Challenge1
{
    public class GetMeasurementsFunction
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("GetMeasurementsFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var response = await httpClient.GetAsync("https://emulatorfunctionapp98f3eeca.azurewebsites.net/api/getstationdata?device_Id=70:ee:50:1b:26:ac&code=K3fQDcXMFOSPKGV1DM8JCzvmtyQtx6C4CG4Ba6Xe1rpN9higlU5S3Q==");

            if (response.IsSuccessStatusCode)
            {
                await using var responseStream = await response.Content.ReadAsStreamAsync();
                var measurements = await JsonSerializer.DeserializeAsync<StationDataModel>(responseStream);

                return new OkObjectResult(measurements);
            }
            else
            {
                var errorMessage = $"Netatmo API call failed. Reason: {response.StatusCode}";
                log.LogError(errorMessage);
                return new BadRequestErrorMessageResult(errorMessage);
            }
        }
    }
}
