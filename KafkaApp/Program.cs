using Confluent.Kafka;
using KafkaApp.Models;
using Microsoft.Extensions.Configuration;
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
                .AddJsonFile("appsettings.json", true, true);
            var config = builder.Build();

            var serverConfig = new ConsumerConfig()
            {
                BootstrapServers = config["Settings:KafkaServer"],
                GroupId = "TwittorAPI",
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
                var topic = new string[] { "tweet", "user", "role", "userRole", "profile", "comment" };
                consumer.Subscribe(topic);
                Console.WriteLine("Waiting Message...");

                try
                {
                    while (true)
                    {
                        var cr = consumer.Consume(cts.Token);
                        Console.WriteLine($"Consumed record with Topic: {cr.Topic} key: {cr.Message.Key} and value: {cr.Message.Value}");
                        using (var dbContext = new TwittorDbContext())
                        {
                            switch (cr.Topic)
                            {
                                case "tweet":
                                    Tweet tweet = JsonSerializer.Deserialize<Tweet>(cr.Message.Value);
                                    dbContext.Tweets.Add(tweet);
                                    break;
                                case "user":
                                    User user = JsonSerializer.Deserialize<User>(cr.Message.Value);
                                    dbContext.Users.Add(user);
                                    break;
                                case "role":
                                    Role role = JsonSerializer.Deserialize<Role>(cr.Message.Value);
                                    dbContext.Roles.Add(role);
                                    break;
                                case "userRole":
                                    UserRole userRole = JsonSerializer.Deserialize<UserRole>(cr.Message.Value);
                                    dbContext.UserRoles.Add(userRole);
                                    break;
                                case "profile":
                                    Profile profile = JsonSerializer.Deserialize<Profile>(cr.Message.Value);
                                    dbContext.Profiles.Add(profile);
                                    break;
                                case "comment":
                                    Comment comment = JsonSerializer.Deserialize<Comment>(cr.Message.Value);
                                    dbContext.Comments.Add(comment);
                                    break;
                                default:
                                    break;
                            }
                            await dbContext.SaveChangesAsync();
                            Console.WriteLine("Data was saved into database");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
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
