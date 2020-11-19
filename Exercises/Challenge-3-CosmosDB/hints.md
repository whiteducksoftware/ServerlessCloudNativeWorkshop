# Hints for Challenge 3 - Receive the events from EventGrid and store the data inside a CosmosDB

_Table of Content_:

- [Provision Azure CosmosDB](#provision-azure-cosmosdb)
  - [Create a database](#create-a-database)
  - [Create a SQL container](#create-a-sql-container)
- [Create an Azure Event Grid triggered Function](#create-a-azure-event-grid-triggered-function)
  - [Initialize the function project](#initialize-the-function-project)
  - [Create a new function](#create-a-new-function)
  - [Add a Cosmos DB output binding](#add-a-cosmos-db-output-binding)
  - [Add appropriate function settings](#add-appropriate-function-settings)
  - [Modify the function so it adds events to a Cosmos DB](#modify-the-function-so-that-it-adds-events-to-a-cosmos-db)
  - [Provision a storage account](#provision-a-storage-account)
  - [Provision the serverless function app](#provision-the-serverless-function-app)
  - [Deploy the function project to Azure](#deploy-the-function-project-to-azure)
- [Subscribe to the EventGrid topic](#subscribe-to-the-eventgrid-topic)
- [Create a HTTP triggered Function to retrieve weather history data](#create-a-http-triggered-function-to-retrieve-weather-history-data)
  - [Create the GetWeatherHistory function](#create-the-getweatherhistory-function)
  - [Add a Cosmos DB input binding](#add-a-cosmos-db-input-binding)
  - [Modify the function so it returns the weather history data](#modify-the-function-so-that-it-returns-the-weather-history-data)
  - [Deploy the GetWeatherHistory function project to Azure](#deploy-the-getweatherhistory-function-project-to-azure)

## Provision Azure CosmosDB

Azure Cosmos DB serverless is a new **consumption-based** offer where you are only charged for the consumed Request Units. At the time of writing this, the serverless Comos DB offering is in **_preview_** and the only supported way to create a new serverless account is by using the **Azure portal**.

Go to the Azure portal to create an Azure Cosmos DB account. Search for and select _Azure Cosmos DB_:

![Architecture](/Assets/Images/challange-3/cosmos-search.png)

Click on "Create Azure Cosmos DB account" and enter the basic settings for your new Account. Be sure to select the **Serverless (preview)** option as the _Capacity mode_. You can skip any further Networking, Backup, or Encryption configuration and create the account using the "Review + Create" button:

![Architecture](/Assets/Images/challange-3/cosmos-basics.png)

### Create a database

To create a SQL API database, you can use the [az cosmosdb sql database create](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/sql/database?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_cosmosdb_sql_database_create) command:

```bash
RESOURCEGROUP=devopenspace-serverless
ACCOUNTNAME=devopenspaceweather
DATABASENAME=devopenspace

az cosmosdb sql database create \
    -a $ACCOUNTNAME \
    -g $RESOURCEGROUP \
    -n $DATABASENAME
```

### Create a SQL container

Now you can create a SQL container under your previously created Azure ComosDB SQL database using the [az cosmosdb sql container create](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/sql/container?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_cosmosdb_sql_container_create) command:

```bash
CONTAINERNAME=weather
PARTITIONKEY='/name'

az cosmosdb sql container create \
    -a $ACCOUNTNAME \
    -g $RESOURCEGROUP \
    -d $DATABASENAME \
    -n $CONTAINERNAME \
    -p $PARTITIONKEY
```

## Create an Azure Event Grid triggered Function

### Initialize the function project

In this section, you create a function project with a function that gets triggered for every event sent to the Event Grid topic we created within the last challenge. This time, you will implement the function in _JavaScript_.

Run the `func init` command, as follows, to create a functions project in a folder named `WeatherHistory` with the _node_ runtime:

```bash
FUNCTIONPROJECTNAME=WeatherHistory

func init $FUNCTIONPROJECTNAME --node
```

### Create a new function

Change your working directory to the `WeatherHistory` folder and add a function to your project by using the following command, where the argument `--name` is the unique name of your function (e. g. `StoreWeatherData`) and the argument `--template` specifies the trigger of the function (`Azure Event Grid trigger`):

```bash
FUNCTIONNAME=StoreWeatherData

func new --name $FUNCTIONNAME --template "Azure Event Grid trigger"
```

Notice that you used the same `func new` command in the last challenge for a C# function. The runtime you have chosen when initializing the local function project is decisive for the code generation language.

### Add a Cosmos DB output binding

The function you scaffolded already contains the event grid trigger input binding, which means that the function runtime can invoke your function on new events and makes it available within your function. This is how your `functions.json` should look like:

```json
{
  "bindings": [
    {
      "type": "eventGridTrigger",
      "name": "eventGridEvent",
      "direction": "in"
    }
  ]
}
```

Now you need to add an **output binding** to the Cosmos DB that you have created before. Be sure that the name of the `databaseName` and `collectionName` property reflects your chosen configuration. Add the binding to the `function.json` file:

```json
{
  "bindings": [
    {
      "type": "eventGridTrigger",
      "name": "eventGridEvent",
      "direction": "in"
    },
    {
      "name": "cosmos",
      "type": "cosmosDB",
      "databaseName": "devopenspace",
      "collectionName": "weather",
      "createIfNotExists": false,
      "connectionStringSetting": "CosmosDb",
      "direction": "out"
    }
  ]
}
```

### Add appropriate function settings

With the `connectionStringSetting` in the output binding, you chose the name of the app setting containing your Azure Cosmos DB connection string. You will need to add the connection string to your Azure Function application settings. In the script below, you will retrieve the connection string using the [az cosmosdb keys list](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/keys?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_cosmosdb_keys_list) command and add it to the app settings using `func settings add`:

```bash

COSMOSDBCONNECTIONSTRING=$(az cosmosdb keys list \
  --name $ACCOUNTNAME \
  --resource-group $RESOURCEGROUP \
  --type connection-strings \
  --query 'connectionStrings[0].connectionString' \
  --output tsv)

func settings add CosmosDb $COSMOSDBCONNECTIONSTRING
```

### Modify the function so it adds events to a Cosmos DB

You have set up everything you need to forward the incoming event to your Cosmos DB. This is how an event looks like:

```json
{
  "id": "9fce2b5f-f5f8-4c8b-9b7e-106f1548b7e8",
  "subject": "wheather",
  "data": "{ \"id\": \"70:ee:50:1b:26:ac\", \"name\": \"ROEB_HOME\", \"dataType\": [ \"Temperature\", \"CO2\", \"Humidity\" ], \"stationModules\": [ { \"id\": \"70:ee:50:1b:26:ac\", \"name\": \"Wohnzimmer\", \"time\": 1603538864, \"temperature\": 26.5, \"co2\": 656, \"humidity\": 33 }, { \"id\": \"03:00:00:03:59:26\", \"name\": \"Küche\", \"time\": 1603538864, \"temperature\": 27.6, \"co2\": 2550, \"humidity\": 76 } ] }",
  "eventType": "devopenspace.serverless.weather",
  "dataVersion": "1.0",
  "metadataVersion": "1",
  "eventTime": "2020-11-10T19:13:30.3091324Z",
  "topic": "/subscriptions/868387ea-d436-4733-aef2-2adc325e4007/resourceGroups/devopenspace-serverless/providers/Microsoft.EventGrid/topics/weather-data"
}
```

Modify your `index.js` by adding the incoming event to the `context.binding.cosmos` object.
Note: the name of the binding `cosmos` must match with the one you have chosen inside the `function.js`:

```javascript
module.exports = async function (context, eventGridEvent) {
  context.bindings.cosmos = JSON.stringify({
    id: eventGridEvent.id,
    data: JSON.parse(eventGridEvent.data),
  });
};
```

### Provision a storage account

Create a general-purpose storage account in your resource group and region by using the `az storage account create` command.
In the following example, replace `<STORAGENAME>` with a **globally unique** name appropriate to you. Names must contain three to 24 characters numbers and lowercase letters only.

```bash
STORAGENAME=<STORAGENAME>
LOCATION=GermanyWestCentral

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
  --functions-version 3 \
  --runtime node
```

### Deploy the function project to Azure

Now you have every necessary resource in place and are ready to deploy your local functions project to the function app in Azure.
Use the [func azure functionapp publish](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?WT.mc_id=AZ-MVP-5003203#project-file-deployment) command within your local function project.

```bash
func azure functionapp publish $FUNCTIONAPPNAME
az functionapp config appsettings set --name $FUNCTIONAPPNAME --resource-group $RESOURCEGROUP --settings "CosmosDb=$COSMOSDBCONNECTIONSTRING"
```

## Subscribe to the EventGrid topic

The function is now deployed and ready to receive events from the event grid. The only part that is missing now is a _subscription_ for our Azure Function to the event grid topic.
Use the [az eventgrid topic show](https://docs.microsoft.com/en-us/cli/azure/eventgrid/topic?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_eventgrid_topic_show) command to retrieve the resource id of the event grid topic. You will also need the resource id of your Azure Function which you can query using the [az functionapp function show](https://docs.microsoft.com/en-us/cli/azure/functionapp/function?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_functionapp_function_show) command. Finally, you have all information to create the subscription using the [az eventgrid event-subscription create](https://docs.microsoft.com/en-us/cli/azure/eventgrid/event-subscription?view=azure-cli-latest&WT.mc_id=AZ-MVP-5003203#az_eventgrid_event_subscription_create) command:

```bash
RESOURCEGROUP=devopenspace-serverless
TOPICNAME=weather-data
SUBSCRIPTIONNAME=persister

EVENTGRIDTOPICRESOURCEID=$(az eventgrid topic show \
    -g $RESOURCEGROUP \
    --name $TOPICNAME \
    --query id \
    --output tsv)

FUNCTIONRESOURCEID=$(az functionapp function show \
    -g $RESOURCEGROUP \
    -n $FUNCTIONAPPNAME \
    --function-name $FUNCTIONNAME \
    --query id \
    --output tsv)

az eventgrid event-subscription create \
    --name $SUBSCRIPTIONNAME \
    --source-resource-id $EVENTGRIDTOPICRESOURCEID \
    --endpoint-type AzureFunction \
    --endpoint $FUNCTIONRESOURCEID
```

## Create an HTTP triggered Function to retrieve weather history data

In this section, you will create an HTTP triggered function that retrieves weather history data.

### Create the GetWeatherHistory function

Like before, change your working directory to the `WeatherHistory` folder and add a function to your project by using the following command, where the argument `--name` is the unique name of your function (e. g. `GetWeatherHistory`) and the argument `--template` specifies the trigger of the function (`HTTP trigger`):

```bash
FUNCTIONNAME=GetWeatherHistory

func new --name $FUNCTIONNAME --template "HTTP trigger"
```

### Add a Cosmos DB input binding

In the StoreWeatherData function, you have used a Cosmos DB output binding. For this function, you will add a _input binding_ that retrieves the whole weather collection. Be sure that the name of the `databaseName` and `collectionName` property reflects your chosen configuration. Add the binding to the `function.json` file:

```json
{
  "bindings": [
    {
      "authLevel": "function",
      "type": "httpTrigger",
      "direction": "in",
      "name": "req",
      "methods": ["get", "post"]
    },
    {
      "type": "http",
      "direction": "out",
      "name": "res"
    },
    {
      "name": "cosmos",
      "type": "cosmosDB",
      "direction": "in",
      "databaseName": "devopenspace",
      "collectionName": "weather",
      "connectionStringSetting": "CosmosDb"
    }
  ]
}
```

### Modify the function so that it returns the weather history data

You receive all weather history documents within the `context.bindings.{name}` object. To return it, you have to add it to the HTTP response body.
Note: the `{name}` must be replaced with the one you have chosen inside the `function.js`:

```javascript
module.exports = async function (context) {
  context.res = {
    body: context.bindings.cosmos,
  };
};
```

### Deploy the GetWeatherHistory function project to Azure

Again, use the [func azure functionapp publish](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?WT.mc_id=AZ-MVP-5003203#project-file-deployment) command to publish the function to Azure.
**_Note_**: This time you don't need to publish the local settings anymore:

```bash
func azure functionapp publish $FUNCTIONAPPNAME
```
