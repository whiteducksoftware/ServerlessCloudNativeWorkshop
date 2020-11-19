# Hints for Challenge 2 - Create a Timer Function and send events to an EventGrid

*Table of Content*:

- [Provision Azure EventGrid](#provision-azure-eventgrid)
- [Create a timer-based Azure Function on your local machine](#create-a-timer-based-azure-function-on-your-local-machine)
  - [Initialize the function project](#initialize-the-function-project)
  - [Create a new function](#create-a-new-function)
  - [Modify the function so it is called every 30 seconds](#modify-the-function-so-that-it-is-called-every-30-seconds)
- [Modify the function so it queries the weather station](#modify-the-function-so-that-it-queries-the-weather-station)
- [Add an Azure EventGrid output binding to distribute the weather data](#add-an-azure-eventgrid-output-binding-to-distribute-the-weather-data)
  - [Add the required NuGet package](#add-the-required-nuget-package)
- [Test the function locally](#test-the-function-locally)
  - [Add appropriate function settings](#add-appropriate-function-settings)
  - [Provide an Azure Storage account](#provide-an-azure-storage-account)
  - [Run the function locally](#run-the-function-locally)
- [Deploy the function to Azure](#deploy-the-function-to-azure)
  - [Provision a storage account](#provision-a-storage-account)
  - [Provision the serverless function app](#provision-the-serverless-function-app)
  - [Deploy the function project to Azure](#deploy-the-function-project-to-azure)

## Provision Azure EventGrid

An event grid topic provides an endpoint where the source sends events. The publisher creates the event grid topic, subscribers decide which topics to subscribe to.
To create a new Azure EventGrid Topic, run the following command:

```bash
RESOURCEGROUP=devopenspace-serverless
TOPICNAME=weather-data
LOCATION=GermanyWestCentral

az eventgrid topic create -g $RESOURCEGROUP --name $TOPICNAME -l $LOCATION
```

## Create a timer-based Azure Function on your local machine

### Initialize the function project

In Azure Functions, a function project is a container for one or more separate functions, each of which reacts to a specific trigger. In this section, you create a function project containing a single time-triggered function.

Run the `func init` command, as follows, to create a functions project in a folder named CollectCurrentWeather with the *dotnet* runtime:

```bash
func init CollectCurrentWeather --dotnet
```

### Create a new function

Add a function to your project by using the following command, where the argument --name is the unique name of your function (collector) and the argument --template specifies the trigger of the function (timer trigger):

```bash
func new --name Collector --template "Timer trigger"
```

### Modify the function so it is called every 30 seconds

For a TimerTrigger to work, you provide a schedule in the form of a **cron expression**. A cron expression is a string with six separate expressions that represent a given schedule via patterns.

Go to the `Collector.cs` file and change the value of the `TimeTrigger` attribute to `*/30 * * * * *`:

```csharp
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CollectCurrentWeather
{
    public static class Collector
    {
        [FunctionName("Collector")]
        public static void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
```

## Modify the function so that it queries the weather station

You will use the [HttpClient Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netcore-3.1&WT.mc_id=AZ-MVP-5003203) to query the weather station.

Add a `using directive` to `System.Net.Http` and change the signature of the function to return a **async Task**. Inside the function, use the `HttpClient.GetAsync` method to query weather data:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace CollectCurrentWeather
{
    public static class Collector
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Collector")]
        public static async Task Run([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer, ILogger log)
        {
            var response = await httpClient.GetAsync("https://emulatorfunctionapp98f3eeca.azurewebsites.net/api/getstationdata?device_Id=70:ee:50:1b:26:ac&code=K3fQDcXMFOSPKGV1DM8JCzvmtyQtx6C4CG4Ba6Xe1rpN9higlU5S3Q==");
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now} and received weather: {await response.Content.ReadAsStringAsync()}");
        }
    }
}
```

## Add an Azure EventGrid output binding to distribute the weather data

You will now add an Azure EventGrid output binding to write events (containing the weather data) to your above created EventGrid topic.

### Add the required NuGet package

Working with the trigger and bindings requires you to reference the appropriate package. For the EventGrid binding, we need to install the `Microsoft.Azure.WebJobs.Extensions.EventGrid` package:

```bash
dotnet add package Microsoft.Azure.WebJobs.Extensions.EventGrid
```

Now you have to add the EventGrid attribute and configure the output binding. You will also need to change the signature and return type of the function:

```csharp
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
```

## Test the function locally

Before you can deploy the function to Azure, you should test it locally.

### Add appropriate function settings

In order to run the function locally, you have to **add the necessary function settings** that the EventGrid binding requires. In the above example, the name of the TopicEndpointUri is `WeatherDataTopicUri` and the name of the TopicKeySetting is `WeatherDataTopicKey`.

You can use the Azure CLI to retrieve the values and add the function settings using the Azure Function CLI:

```bash
TOPICKEY=$(az eventgrid topic key list --name $TOPICNAME --resource-group $RESOURCEGROUP --query "key1" --output tsv)
TOPICURL=$(az eventgrid topic show --name $TOPICNAME -g $RESOURCEGROUP --query "endpoint" --output tsv)

func settings add WeatherDataTopicKey $TOPICKEY
func settings add WeatherDataTopicUri $TOPICURL
```

### Provide an Azure Storage account

By default, your Azure function will try to use a local Azure Storage Emulator by using `UseDevelopmentStorage=true` as the connection string. The Azure Functions runtime uses this storage account connection string for all functions except for HTTP triggered functions.
You can see the configuration of the `AzureWebJobsStorage` within the `local.settings.json`:

```json
"AzureWebJobsStorage": "UseDevelopmentStorage=true"
```

If you are connected to the devcontainer, your visual studio code has the Azure Storage Emulator ["Azurite"](https://github.com/Azure/Azurite) pre-installed. You can start it using the `Azurite: Start` Visual Studio Code command (<kbd>CTRL</kbd> + <kbd>SHIFT</kbd> + <kbd>P</kbd>):

![Azurite Storage Emulator](/Assets/Images/challange-2/azurite.png)

Alternatively, an Azure Storage Account can be used.

### Run the function locally

You are now able to start the function using the `func start` command. If you wait for 30 seconds, you should see an output similar to this:

![Function start example](/Assets/Images/challange-2/function-start.png)

To verify that the event grid topic received the messages, go to your EventGrid Topic in the Azure Portal, click on Metrics and set the filter to **Unmatched Events**.
Note: You may have to wait a few minutes before you are able to see the events:
![EventGrid metrics](/Assets/Images/challange-2/event-grid.png)

## Deploy the function to Azure

Before you can deploy your function code to Azure, you must create three resources:

* A **resource group**, which is a logical container for related resources (you already created the resource group in [Challenge 1](/Exercises/Challenge-1-Basic-HTTP-Functions/readme.md)).
* A **storage account** that manages the status and other information about your projects.
* A **function app** that provides the environment for executing your function code.

### Provision a storage account

Create a general-purpose storage account in your resource group and region by using the `az storage account create` command.
In the following example, replace `<STORAGENAME>` with a **globally unique** name appropriate to you. Names must contain three to 24 characters numbers and lowercase letters only.

```bash
STORAGENAME=<STORAGENAME>

az storage account create \
  --name $STORAGENAME \
  --location $LOCATION \
  --resource-group $RESOURCEGROUP \
  --sku Standard_LRS
```

### Provision the serverless function app

You can create the function app using the [`az functionapp create`](https://docs.microsoft.com/en-us/cli/azure/functionapp?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_functionapp_create) command.

In the following example, replace `<APP_NAME>` with a globally unique name appropriate to you. The `<APP_NAME>` is also the default DNS domain for the function app.

```bash
FUNCTIONAPPNAME=<APP_NAME>

az functionapp create \
  --name $FUNCTIONAPPNAME \
  --storage-account $STORAGENAME \
  --consumption-plan-location $LOCATION \
  --resource-group $RESOURCEGROUP \
  --functions-version 3
```

### Deploy the function project to Azure

Now you have every necessary resource in place and are ready to deploy your local functions project to the function app in Azure.
Use the [func azure functionapp publish](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?WT.mc_id=AZ-MVP-5003203#project-file-deployment) command within your local function project.

***Note***: The command will ask you whether you would like to overwrite the `AzureWebJobsStorage` App setting. Choose *no*.

```bash
func azure functionapp publish $FUNCTIONAPPNAME --publish-local-settings
```

![func cli output](/Assets/Images/challange-2/overwrite-func-settings.png)
