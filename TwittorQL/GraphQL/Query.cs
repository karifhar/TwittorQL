using HotChocolate;
using System.Linq;
using TwittorQL.GraphQL.Data;
using TwittorQL.Models;

namespace TwittorQL.GraphQL
{
    public class Query
    {
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
        public IQueryable<ProfileData> GetProfileByName([Service] TwittorDbContext context, string name)
        {
            var profile = context.Profiles.Where(p => p.Fullname == name).Select(p => new ProfileData 
            { 
                Fullname = p.Fullname,
                Birth = p.Birth,
            
            });
            return profile;
        }
    }
}
