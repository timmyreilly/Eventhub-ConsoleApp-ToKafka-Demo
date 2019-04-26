# Azure Storage -> Event Grid -> Event Hub -> Kafka 

### Create a storage account 
This is the storage account we're going to monitor for storage changes

### Create an Event Hub
All event grid messages from our storage account will be published to our Event Hub

### Subscribe to storage account events and set Event Hub as the endppoint. 
From the storage account you can choose events and select the endpoint as the Event Hub we created. 

## Getting Kafka wired up: 

Added to .csproj file

My shared access policy from EventHubs: 
`Endpoint=sb://itime-kafka.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=xQo1CViZBPppgdRFG0UQIO/71l6gho3KCkqoubn/Em4=`



# Getting kafka running on a server

Ubuntu 16.04 in Azure

Prereq Resources: 
https://hevodata.com/blog/how-to-install-kafka-on-ubuntu/
https://zookeeper.apache.org/doc/r3.1.2/zookeeperStarted.html 

Run Zookeeper 

`whereis zookeeper`

`sudo /usr/share/zookeeper/bin/zkServer.sh start`

`netstat -ant`

### Start Kafka:

`sudo /opt/kafka/bin/kafka-server-start.sh /opt/kafka/config/server.properties` 

### STOP Kafka:
You'll want to stop it properly or else you'll have to force kill the process. 
`sudo /opt/kafka/bin/kafka-server-stop.sh` 

Run our dotnet core application: 
Need to configure a different storage account to use as a lease for offsets. 

`cd .\event-grid-to-kafka\EventHubContainerApp`
`dotnet run` 

You're now running the Event Processor host for Azure Event Hubs: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-event-processor-host 

It's waiting for events on event hub and will 'publish' those events to a topic on Kafka. 

To see the events making there way into kafka run the consumer on the server side in another terminal window:
`ssh conductor@ip-address`  
`$ /opt/kafka/bin/kafka-console-consumer.sh --topic test-topic --bootstrap-server localhost:9092`



### Create dotnet core application in Ubuntu: 

Instructions: 
Get Ubuntu Version: `lsb_release -a`
Follow dotnet instructions: https://www.microsoft.com/net/download 

Need to have a seperate storage account to use as Event Lock Lease. 
Create another storage account or you'll end up with a neverending loop as the events of updating the checkpoint lease will add another message to the Event Hub... and loop again! 

clone repo

`dotnet run`

### Install Docker:

```
sudo apt-get install \
    apt-transport-https \
    ca-certificates \
    curl \
    software-properties-common
```


### Snippets: 



If you improperly shutdown kafka and you have a lock on temp files you might be in a CLOSE_WAIT State: 
When you try to start Kafka up again: 
`Socket server failed to bind to 0.0.0.0:9092: Address already in use`

You can see the CLOSE_WAIT: 
`sudo netstat -antp`

And go kill the process:
`sudo kill -9 2337` <-your pid


#### Sample Topic Subscription 
We're going to try to recreate this in a terraform module: 
```JSON
{
	"name": "redundant",
	"properties": {
		"topic": "/subscriptions/5c514147-21c3-4f7e-8329-625443da4254/resourceGroups/i-time/providers/Microsoft.Storage/storageAccounts/itime",
		"destination": {
			"endpointType": "EventHub",
			"properties": {
				"resourceId": "/subscriptions/5c514147-21c3-4f7e-8329-625443da4254/resourceGroups/i-time/providers/Microsoft.EventHub/namespaces/going-into-vnet/eventhubs/eventsfromstoragetwo"
			}
		},
		"filter": {
			"includedEventTypes": [
				"All"
			]
		},
		"labels": [],
		"eventDeliverySchema": "EventGridSchema"
	}
}
```


### Old : 

```
az group deployment create \
  --resource-group i-time \
  --template-uri "https://raw.githubusercontent.com/Azure-Samples/azure-event-grid-viewer/master/azuredeploy.json" \
  --parameters siteName=$sitename hostingPlanName=viewerhost
```

```bash
storageid=$(az storage account show --name itime --resource-group i-time --query id --output tsv)
endpoint=https://$sitename.azurewebsites.net/api/updates

az eventgrid event-subscription create --resource-id $storageid --name subscription-itime-woop --endpoint $endpoint
```

```bash
storageName=itime
rg=i-time
export AZURE_STORAGE_ACCOUNT=itime
export AZURE_STORAGE_ACCESS_KEY="$(az storage account keys list --account-name itime --resource-group i-time --query "[0].value" --output tsv)"

az storage container create --name testcontainer

touch testfile.txt
az storage blob upload --file testfile.txt --container-name testcontainer --name testfile.txt
```


```JSON

{
  "topic": "/subscriptions/5c514147-21c3-4f7e-8329-625443da4254/resourceGroups/i-time/providers/Microsoft.Storage/storageAccounts/itime",
  "subject": "/blobServices/default/containers/testcontainer/blobs/testfile.txt",
  "eventType": "Microsoft.Storage.BlobCreated",
  "eventTime": "2018-11-12T23:39:50.6286206Z",
  "id": "246ad0a7-201e-00b3-10e1-7acbe406fd39",
  "data": {
    "api": "PutBlob",
    "clientRequestId": "3ed2fefe-e6d4-11e8-b9dd-0a580af44006",
    "requestId": "246ad0a7-201e-00b3-10e1-7acbe4000000",
    "eTag": "0x8D648F823A0127E",
    "contentType": "text/plain",
    "contentLength": 0,
    "blobType": "BlockBlob",
    "url": "https://itime.blob.core.windows.net/testcontainer/testfile.txt",
    "sequencer": "000000000000000000000000000003E5000000000092f1a9",
    "storageDiagnostics": {
      "batchId": "dd7f0381-38e4-4b7d-9f90-e740d7d5300e"
    }
  },
  "dataVersion": "",
  "metadataVersion": "1"
}
```




`http://ed74fb07.ngrok.io`
`http://ed74fb07.ngrok.io/runtime/webhooks/eventgrid?functionName=EventGridTest`


`az eventgrid event-subscription create --resource-id /subscriptions/aeb4b7cb-b7cb-b7cb-b7cb-b7cbb6607f30/resourceGroups/eg0122/providers/Microsoft.Storage/storageAccounts/egblobstor0122 --name egblobsub0126 --endpoint https://263db807.ngrok.io/admin/extensions/EventGridExtensionConfig?functionName=EventGridTest`

$endpoint=http://ed74fb07.ngrok.io/runtime/webhooks/eventgrid?functionName=EventGridTest

az eventgrid event-subscription create --resource-id $storageid --name subscription-itime-whoop --endpoint $endpoint