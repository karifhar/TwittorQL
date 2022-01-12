using HotChocolate;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwittorQL.GraphQL.Data;
using TwittorQL.GraphQL.Input;
using TwittorQL.Models;

namespace TwittorQL.GraphQL
{
    public class Mutation 
    {
        // User
        public async Task<UserData> RegisterUser(RegisterUserInput input, [Service] TwittorDbContext context)
        {
            var user = context.Users.Where(e => e.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password)
            };
            context.Users.Add(newUser);
            await context.SaveChangesAsync();
            return await Task.FromResult(new UserData()
            {
                Id = newUser.Id,
                Username = newUser.Username,
            });
        }
       

        // Twittor
        public async Task<TweetData> AddPost(TweetInput input, [Service] TwittorDbContext context)
        {
            var user = context.Users.Where(p => p.Id == input.id);
            if (user != null)
            {
                return await Task.FromResult(new TweetData());
            }
            var twot = new Tweet
            {
                ProfileId = input.id,
                Post = input.post,
                Created = DateTime.Now
            };
            context.Tweets.Add(twot);
            await context.SaveChangesAsync();
            return await Task.FromResult(new TweetData()
            {
                Id = twot.Id,
                Post = twot.Post,
                Created = twot.Created
            });
        }
        public async Task<string> DeletePost(int id, [Service] TwittorDbContext context)
        {
            var post = context.Tweets.Where(e=> e.Id == id).FirstOrDefault();
            if (post != null)
            {
                context.Tweets.Remove(post);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult("Tweet has deleted");
        }
        public async Task<Comment> AddComment(CommentInput input, [Service] TwittorDbContext context)
        {
            var comment = new Comment
            {
                TweetId = input.TweetId,
                ProfileId = input.ProfileId,
                Comment1 = input.Comment,
                Created = DateTime.Now
            };
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
            return comment;
        }
    }
}
