using Confluent.Kafka;
using KafkaApp.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaApp
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();

            var serverConfig = new ConsumerConfig()
            {
                BootstrapServers = config["Settings:KafkaServer"],
                GroupId = "tester",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.WriteLine("----------Welcome Kafka App----------");
            using (var consumer = new ConsumerBuilder<string, string>(serverConfig).Build())
            {
                Console.WriteLine("---->Connected<----");
                var topics = new string[] { 
                    "post-tweet",
                    "delete-tweet",
                    "add-user", 
                    "add-role", 
                    "change-password", 
                    "add-profile", 
                    "add-comment"
                
                };
                consumer.Subscribe(topics);

                Console.WriteLine("Waiting Message...");

                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed record with Topic: {cr.Topic} key: {cr.Message.Key} and value: {cr.Message.Value}");

                        using (var dbContext = new TwittorDbContext())
                        {
                            if (cr.Topic == "post-tweet")
                            {
                                Tweet tweet = JsonConvert.DeserializeObject<Tweet>(cr.Message.Value);
                                dbContext.Tweets.Add(tweet);
                            }
                            if (cr.Topic == "delete-tweet")
                            {
                                Tweet tweet = JsonConvert.DeserializeObject<Tweet>(cr.Message.Value);
                                dbContext.Tweets.Remove(tweet);
                            }
                            if (cr.Topic == "add-user")
                            {
                                User user = JsonConvert.DeserializeObject<User>(cr.Message.Value);
                                dbContext.Users.Add(user);
                            }
                            if (cr.Topic == "add-role")
                            {
                                Role role = JsonConvert.DeserializeObject<Role>(cr.Message.Value);
                                dbContext.Roles.Add(role);
                            }
                            if (cr.Topic == "change-password")
                            {
                                User password = JsonConvert.DeserializeObject<User>(cr.Message.Value);
                                dbContext.Users.Update(password);
                            }
                            if (cr.Topic == "add-profile")
                            {
                                Profile profile = JsonConvert.DeserializeObject<Profile>(cr.Message.Value);
                                dbContext.Profiles.Add(profile);
                            }
                            if (cr.Topic == "add-comment")
                            {
                                Comment comment = JsonConvert.DeserializeObject<Comment>(cr.Message.Value);
                                dbContext.Comments.Add(comment);
                            }
                            await dbContext.SaveChangesAsync();
                            Console.WriteLine("Data was saved into database");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ctrl-C was pressed.
                }
                finally
                {
                    consumer.Close();
                }
            }
            return 1;
        }
    }
}
