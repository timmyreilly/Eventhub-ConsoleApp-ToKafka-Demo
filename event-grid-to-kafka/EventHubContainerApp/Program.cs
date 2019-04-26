
namespace EventHubContainerApp
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.Azure.EventHubs;
    using Microsoft.Azure.EventHubs.Processor;
    using System.Text;
    using Confluent.Kafka;

    public class Program
    {
        private const string EventHubConnectionString = "Endpoint=sb://going-into-vnet.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XotH6VougyXTSuP+GxKvJ3LvPkWcDXUt1cExTtkHy4M=";
        private const string EventHubName = "eventsfromstoragetwo";
        private const string StorageContainerName = "spehubcontainer";
        private const string StorageAccountName = "placeforeph";
        private const string StorageAccountKey = "rcP0tZPxd/BR9F/tN+RS6ehwlwvpOW+PEUnxU4PTOn3oGGtd0gD+2NNtjAGrskS+83OV1go0dCKDNdmjcrtQHw==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);

        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Registering EventProcessor...");

            var eventProcessorHost = new EventProcessorHost(
                EventHubName,
                PartitionReceiver.DefaultConsumerGroupName,
                EventHubConnectionString,
                StorageConnectionString,
                StorageContainerName);



            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();

            // Disposes of the Event Processor Host
            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }


    public class SimpleEventProcessor : IEventProcessor
    {
        private Producer<Null, string> _producer;

        public SimpleEventProcessor()
        {
            //TODO: Get the config information from an env variable, and throw error if not found.
            var config = new ProducerConfig { BootstrapServers = "104.42.47.172:9092" };
            _producer = new Producer<Null, string>(config);
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"SimpleEventProcessor initialized. Partition: '{context.PartitionId}'");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error on Partition: {context.PartitionId}, Error: {error.Message}");
            return Task.CompletedTask;
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var eventData in messages)
            {
                //TODO: use the producer to write to kafka
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");

                try
                {
                    var dr = await _producer.ProduceAsync("test-topic", new Message<Null, string> { Value = $"{data}" });
                    Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");

                    await context.CheckpointAsync();  // when we get a write to Kafka, update the checkpoint 

                }
                catch (KafkaException e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }

            // return await context.CheckpointAsync();
        }
    }
}
