using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Confluent.Kafka.Serialization; 
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Company.Function
{
    public static class EventGridTriggerCSharp
    {
        

        [FunctionName("EventGridTest")]
        public static void EventGridTest([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            const string KAFKA_SERVER = "";
            const string KAFKA_TOPIC = "topic1";
            const string STRATUS_EVENT_SOURCE = ""; 
            log.LogInformation(eventGridEvent.Data.ToString());

            try {

                

            } catch (System.Exception e) {
                log.LogInformation("Caught exception creating and publishing Stratus event: " + e.Message); 

                throw; 
            }

            log.LogInformation("All Done"); 
        }


        public static async Task Producer(string brokerList, string connStr, string topic, string cacertlocation)
        {
            try
            {
                var config = new Dictionary<string, object> {
                    { "bootstrap.servers", brokerList },
                    { "security.protocol", "SASL_SSL" },
                    { "sasl.mechanism", "PLAIN" },
                    { "sasl.username", "$ConnectionString" },
                    { "sasl.password", connStr },
                    { "ssl.ca.location", cacertlocation },
                    //{ "debug", "security,broker,protocol" }       //Uncomment for librdkafka debugging information
                };

                using (var producer = new Producer<long, string>(config, new LongSerializer(), new StringSerializer(Encoding.UTF8)))
                {
                    Console.WriteLine("Sending 10 messages to topic: " + topic + ", broker(s): " + brokerList);
                    for (int x = 0; x < 10; x++)
                    {
                        var msg = string.Format("Sample message #{0} sent at {1}", x, DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.ffff"));
                        var deliveryReport = await producer.ProduceAsync(topic, DateTime.UtcNow.Ticks, msg);
                        Console.WriteLine(string.Format("Message {0} sent (value: '{1}')", x, msg));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Exception Occurred - {0}", e.Message));
            }
        }
    }
}