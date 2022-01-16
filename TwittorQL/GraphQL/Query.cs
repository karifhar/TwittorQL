using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using System.Linq;
using TwittorQL.GraphQL.Data;
using TwittorQL.Models;

namespace TwittorQL.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<TweetData> GetTweets([Service] TwittorDbContext context)
        {
            var data = context.Tweets.Select(t => new TweetData
            {
                Id = t.Id,
                Post = t.Post,
                ProfileId = t.ProfileId,
                Created = t.Created,
            });
            return data;
        }
        public IQueryable<ProfileData> GetProfileByName(string name, [Service] TwittorDbContext context)
        {
            var profile = context.Profiles.Where(p => p.Fullname == name).Select(p => new ProfileData 
            { 
                Fullname = p.Fullname,
                Birth = p.Birth,
            
            });
            return profile;
        }
        public IQueryable<UserData> GetUsers([Service] TwittorDbContext context)
        {
            var users = context.Users.Select(u => new UserData()
            {
                Id = u.Id,
                Username = u.Username
            });
            return users;
        }
    }
}
