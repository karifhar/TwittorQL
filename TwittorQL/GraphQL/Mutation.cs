using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TwittorQL.GraphQL.Data;
using TwittorQL.GraphQL.Input;
using TwittorQL.Kafka;
using Newtonsoft.Json;
using TwittorQL.Models;

namespace TwittorQL.GraphQL
{
    public class Mutation 
    {
        // User

        public async Task<TransactionStatus> RegisterUser(RegisterUserInput input, 
            [Service] TwittorDbContext context,
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(e => e.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, ""));
            }

            var newUser = new User()
            {
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
            };
            //context.Users.Add(newUser);
            //await context.SaveChangesAsync();
            var key = "register-user-" + DateTime.Now.ToString();
            var value = JObject.FromObject(newUser).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "register-user", key, value);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, value);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            var userForProfile = context.Users.Where(p => p.Id == newUser.Id).FirstOrDefault();

            if (userForProfile != null)
            {
                var createProfile = new Profile()
                {
                    Fullname = userForProfile.Username,
                    Birth = DateTime.Today,
                    UserId = newUser.Id,
                };

                var key1 = "create-profile-" + DateTime.Now.ToString();
                var value1 = JObject.FromObject(createProfile).ToString(Formatting.None);
                var data = await KafkaHelper.SendMessage(kafkaSettings.Value, "add-profile", key1, value1);
                await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key1, value1);

                var ret1 = new TransactionStatus(data, "");
                if (!data)
                    ret1 = new TransactionStatus(data, "Failed to submit data");
            }


            return await Task.FromResult(ret);
        }
       
        public async Task<UserToken> LoginUser(RegisterUserInput input, [Service] TwittorDbContext context,
            [Service] IOptions<TokenSettings> tokenSettings)
        {
            var user = context.Users.Where(u => u.Username == input.Username).FirstOrDefault();
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);

            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }

            if (!valid)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, user.Username));
            var roles = context.UserRoles.Where(ur => ur.UserId == user.Id).FirstOrDefault();
            var role = context.Roles.Where(r => r.Id == roles.RoleId).FirstOrDefault();
            claims.Add(new Claim(ClaimTypes.Role, role.Name));

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expired = DateTime.Now.AddHours(6);
            var token = new JwtSecurityToken(
                  issuer: tokenSettings.Value.Issuer,
                  audience: tokenSettings.Value.Audience,
                  expires: expired,
                  claims: claims,
                  signingCredentials: credentials
              );
            return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(token),
                    expired.ToString(), null));
        }

        [Authorize(Roles = new[] { "admin", "member" })]
        public async Task<TransactionStatus> ChangePassword(ChangePassInput input, [Service] TwittorDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(u => u.Username == input.username).FirstOrDefault();
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);

            if (user == null && !valid)
            {
               return await Task.FromResult(new TransactionStatus(false, "Username atau password salah"));
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);

            var key = "change-password-" + DateTime.Now.ToString();
            var val = JObject.FromObject(user).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "change-password", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);

        }
        [Authorize(Roles = new[] { "admin" })]
        public async Task<TransactionStatus> AddRole(RoleInput input, [Service] TwittorDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings) 
        {
            var role = context.Roles.Where(r => r.Name == input.Name).FirstOrDefault();
            if (role != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "role tidak ditemukan"));
            }

            var newRole = new Role()
            {
                Name = input.Name,
            };

            var key = "add-role-" + DateTime.Now.ToString();
            var val = JObject.FromObject(newRole).ToString(Formatting.None);
            var result = await KafkaHelper.SendMessage(kafkaSettings.Value, "add-role", key, val);
            await KafkaHelper.SendMessage(kafkaSettings.Value, "logging", key, val);

            var ret = new TransactionStatus(result, "");
            if (!result)
                ret = new TransactionStatus(result, "Failed to submit data");

            return await Task.FromResult(ret);
        }
       [Authorize(Roles = new[] { "admin" })]
        public async Task<UserRole> AddRoleTouser(CreateUserRoleInput input, [Service] TwittorDbContext context)
        {
            var user = context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            var role = context.Roles.Where(r => r.Name == input.Name).FirstOrDefault();
            if (user == null && role == null)
            {
                return await Task.FromResult(new UserRole());
            }
                var userRole = new UserRole()
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                };
                context.UserRoles.Add(userRole);
                await context.SaveChangesAsync();
            return userRole;
        }
        [Authorize(Roles = new string[] { "admin" })]
        public async Task<UserRole> ChangeUserRole(CreateUserRoleInput input, [Service] TwittorDbContext context)
        {
            var userRole = context.UserRoles.Where(ur => ur.UserId == input.UserId).FirstOrDefault();
            var role = context.Roles.Where(r => r.Name == input.Name).FirstOrDefault();
            if (userRole != null && role != null)
            {
                userRole.RoleId = role.Id;
                context.UserRoles.Update(userRole);
                await context.SaveChangesAsync();
                 return await Task.FromResult(userRole);
            }
            return await Task.FromResult(new UserRole());
            
        }

        // Twittor
        [Authorize(Roles = new string[] { "member" })]
        public async Task<TweetData> AddPost(TweetInput input, [Service] TwittorDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var user = context.Users.Where(u => u.Id == input.id).FirstOrDefault();
            var profile = context.Profiles.Where(p => p.UserId == user.Id).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TweetData());
            }
            var twot = new Tweet
            {
                ProfileId = profile.Id,
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
        [Authorize(Roles = new string[] { "member" })]
        public async Task<string> DeletePost(int input, [Service] TwittorDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var post = context.Tweets.Where(e=> e.Id == input).FirstOrDefault();
            bool isSuccess = false;
            if (post != null)
            {
                context.Tweets.Remove(post);
                await context.SaveChangesAsync();
                isSuccess = true;
            }
            if (isSuccess)
            {
                return await Task.FromResult("Post has been deleted");
            }
            return await Task.FromResult("Failed to delete post");
        }
        [Authorize(Roles = new string[] { "member" })]
        public async Task<Comment> AddComment(CommentInput input, [Service] TwittorDbContext context, 
            [Service] IOptions<KafkaSettings> kafkaSettings)
        {
            var tweet = context.Tweets.Where(t => t.Id == input.TweetId).FirstOrDefault();
            var profile = context.Profiles.Where(p => p.Id == input.ProfileId).FirstOrDefault();

            if (tweet != null && profile != null)
            {
                var comment = new Comment
                {
                    TweetId = tweet.Id,
                    ProfileId = profile.Id,
                    Comment1 = input.Comment,
                    Created = DateTime.Now
                };
                context.Comments.Add(comment);
                await context.SaveChangesAsync();
                return comment;
            }
            return null;
        }
        [Authorize(Roles = new string[] { "admin", "member" })]
        public async Task<ProfileData> EditProfile(EditProfileInput input, [Service] TwittorDbContext context)
        {
            var profile = context.Profiles.Where(p => p.Id == input.id).FirstOrDefault();
            if (profile != null)
            {
                profile.Fullname = input.Fullname;
                profile.Birth = input.Birth;
                context.Profiles.Update(profile);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(new ProfileData()
            {
               Fullname = profile.Fullname,
               Birth = profile.Birth,
            });
        }
    }
}
