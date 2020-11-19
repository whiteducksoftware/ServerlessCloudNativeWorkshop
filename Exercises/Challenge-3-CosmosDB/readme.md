# Challenge 3 - Receive the events from EventGrid and store the data inside a Cosmos DB

In this challenge, you will upgrade the solution to *store the weather data* in a Cosmos DB. You will also create a function that *exposes the historical data* using HTTP:

![Architecture](/Assets/Images/challange-3/architecture.png)

## ⚠️ Challenge

* Provision Azure CosmosDb
* Create an Azure Function with an Event Grid binding
* Subscribe to the EventGrid topic
* Create an HTTP triggered Function to retrieve weather history data

## 💡 Success Criteria

You have a **function** in place that gets triggered for each new Event Grid event and stores the payload (weather data) into a Cosmos DB. You will also need another function that returns all collected weather data and exposes it using HTTP.

This is how your challenge 3 resources should look like:

![Resource group](/Assets/Images/challange-3/resoruce-group.png)

## ℹ️ References

* [Create an Azure Cosmos account, database, container, and items from the Azure portal](https://docs.microsoft.com/en-us/azure/cosmos-db/create-cosmosdb-resources-portal?WT.mc_id=AZ-MVP-5003203)
* [Azure Cosmos DB input binding for Azure Functions 2.x and higher](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-input?WT.mc_id=AZ-MVP-5003203)
* [Create an Azure Function that connects to an Azure Cosmos DB](https://docs.microsoft.com/en-us/azure/azure-functions/scripts/functions-cli-create-function-app-connect-to-cosmos-db?WT.mc_id=AZ-MVP-5003203)

## ✔️ Next Challenge

In the next challenge, you will create a LogicApp which is subscribed to the EventGrid topic and sends an email if measured values exceed a certain limit.
