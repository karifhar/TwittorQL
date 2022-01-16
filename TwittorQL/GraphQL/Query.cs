using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TwittorQL.GraphQL.Data;
using TwittorQL.Kafka;
using TwittorQL.Models;

namespace TwittorQL.GraphQL
{
    public class Query
    {
        //[Authorize]
        public async Task<IQueryable<Tweet>> GetTweets(
            [Service] TwittorDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var data = context.Tweets.Select(t => new TweetData
            {
                Id = t.Id,
                Post = t.Post,
                ProfileId = t.ProfileId,
                Created = t.Created,
            });
             var key = "GetTweets-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new {Message = "GrapQL Query GetTweets" }).ToString(Formatting.None);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);
            return context.Tweets;
        }
        public async Task<IQueryable<ProfileData>> GetProfileByName(string name,
            [Service] TwittorDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var profile = context.Profiles.Where(p => p.Fullname == name).Select(p => new ProfileData 
            { 
                Fullname = p.Fullname,
                Birth = p.Birth,
            
            });
            var key = "GetProfilebyName-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GrapQL Query GetProfiles" }).ToString(Formatting.None);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);
            return profile;
        }
        public async Task<IQueryable<UserData>> GetUsers([Service] TwittorDbContext context, [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var users = context.Users.Select(u => new UserData()
            {
                Id = u.Id,
                Username = u.Username
            });
            var key = "GetProfilebyName-" + DateTime.Now.ToString();
            var val = JObject.FromObject(new { Message = "GrapQL Query GetUsers" }).ToString(Formatting.None);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);
            return users;
        }
    }
}
