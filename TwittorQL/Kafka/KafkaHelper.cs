using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TwittorQL.Models;

namespace TwittorQL.Kafka
{
    public class KafkaHelper
    {
        public static async Task<bool> SendMessage(KafkaSettings settings, string topic, string key, string value)
        {
            var succeed = false;
            var config = new ProducerConfig
            {
                BootstrapServers = settings.Server,
                ClientId = Dns.GetHostName()
            };
            Console.WriteLine($"{settings.Server}");

            using(var adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    await adminClient.CreateTopicsAsync(new List<TopicSpecification>
                    {
                        new TopicSpecification
                        {
                            Name = topic,
                            NumPartitions = 1,
                            ReplicationFactor = 1
                        }
                    });
                }
                catch (CreateTopicsException ex)
                {

                    if (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
                    {
                        Console.WriteLine($"An error occured creating topic {topic}: {ex.Results[0].Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine("Topic already exists");
                    }
                }
            }

            using (var producer = new ProducerBuilder<string, string>(config).Build())
            {
                producer.Produce(topic, new Message<string, string>
                {
                    Key = key,
                    Value = value
                }, (deliveryReport) =>
                {
                    if (deliveryReport.Error.Code != ErrorCode.NoError)
                    {
                        Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                    }
                    else
                    {
                        Console.WriteLine($"Produced message to: {deliveryReport.TopicPartitionOffset}");
                        succeed = true;
                    }
                });
                producer.Flush(TimeSpan.FromSeconds(10));
            }
            return await Task.FromResult(succeed);
        }
    }
}
