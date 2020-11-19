# Challenge 2 - Create a Timer Function and send events to an Event Grid

The Netatmo Weather Station exposes an endpoint to query weather data, but it isn't able to push the data to an endpoint. In this challenge, you will upgrade the solution and periodically query the station and distribute the data using *Azure Event Grid*.

![Architecture](/Assets/Images/challange-2/architecture.png)

## ⚠️ Challenge

* Provision Azure EventGrid
* Create a timer-based Azure Function on your local machine
* Modify the function so it is called every 30 seconds
* Modify the function so it queries the weather station
* Add an Azure EventGrid output binding to distribute the weather data
* Test the function locally
* Deploy the function to Azure

## 💡 Success Criteria

You have a **function** in place that polls the station **every 30 seconds** and publishes the data using an **Azure Event Grid Topic**.

This is how your challenge 2 resources should look like:

![Resource group](/Assets/Images/challange-2/resoruce-group.png)

## ℹ️ References

* [Event Grid overview](https://docs.microsoft.com/en-us/azure/event-grid/overview)
  * [Create Event Grid custom topic with Azure CLI](https://docs.microsoft.com/en-us/azure/event-grid/scripts/event-grid-cli-create-custom-topic?WT.mc_id=AZ-MVP-5003203)
* [Create a local function project](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function-azure-cli?tabs=bash%2Cbrowser&pivots=programming-language-csharp&WT.mc_id=AZ-MVP-5003203#create-a-local-function-project)
  * [Timer trigger for Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp)
* [CRON expression](https://en.wikipedia.org/wiki/Cron#CRON_expression)
* [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=netcore-3.1&WT.mc_id=AZ-MVP-5003203)
* [Azure Event Grid bindings for Azure Functions](https://docs.microsoft.com/en-us/azure/event-grid/post-to-custom-topic?WT.mc_id=AZ-MVP-5003203)
  * [Event Grid output binding C#](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-event-grid-output?tabs=csharp)
* [Create a function app for serverless code execution](https://docs.microsoft.com/en-us/azure/azure-functions/scripts/functions-cli-create-serverless?WT.mc_id=AZ-MVP-5003203)

## 👍 Quick Start

* [Install Docker](https://www.docker.com/get-started)
  * [VS Code Docker extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)
* [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

## ✔️ Next Challenge

In the next challenge, you will receive the events from Event Grid and store the data inside a Cosmos DB.
