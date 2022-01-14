using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TwittorQL.GraphQL.Data;
using TwittorQL.GraphQL.Input;
using TwittorQL.Models;

namespace TwittorQL.GraphQL
{
    public class Mutation 
    {
        private TwittorDbContext _context;
        private IOptions<TokenSettings> _tokenSettings;


        // User
        public async Task<UserData> RegisterUserAsync(RegisterUserInput input, [Service] TwittorDbContext context)
        {
            var user = context.Users.Where(e => e.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User()
            {
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password),
            };
            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData()
            {
                Id = newUser.Id,
                Username = newUser.Username,
            });
        }
       
        public async Task<UserToken> LoginUser(RegisterUserInput input)
        {
            var user = _context.Users.Where(u => u.Username == input.Username).FirstOrDefault();
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);

            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }


            if (!valid)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            foreach (var userRole in user.UserRoles)
            {
                var role = _context.Roles.Where(r => r.Id == userRole.UserId).FirstOrDefault();
                if (role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSettings.Value.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expired = DateTime.Now.AddHours(6);
            var token = new JwtSecurityToken(
                  issuer: _tokenSettings.Value.Issuer,
                  audience: _tokenSettings.Value.Audience,
                  expires: expired,
                  claims: claims,
                  signingCredentials: credentials
              );
            return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(token),
                    expired.ToString(), null));
        }
        public async Task<Role> AddRole(RoleInput input) 
        {
            var role = _context.Roles.Where(r => r.Name == input.Name).FirstOrDefault();
            if (role != null)
            {
                return await Task.FromResult(role);
            }

            var newRole = new Role()
            {
                Name = input.Name,
            };
            _context.Roles.Add(newRole);
            await _context.SaveChangesAsync();
            return newRole;
        }
        public async Task<UserRole> AddRoleTouser(CreateUserRoleInput input)
        {
            var user = _context.Users.Where(u => u.Id == input.UserId).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Name == input.Name).FirstOrDefault();
            if (user == null && role == null)
            {
                return await Task.FromResult(new UserRole());
            }
                var userRole = new UserRole()
                {
                    UserId = user.Id,
                    RoleId = role.Id,
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            return userRole;
        }
        

       
        // Twittor
        public async Task<TweetData> AddPost(TweetInput input)
        {
            var user = _context.Users.Where(p => p.Id == input.id).FirstOrDefault();
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
            _context.Tweets.Add(twot);
            await _context.SaveChangesAsync();
            return await Task.FromResult(new TweetData()
            {
                Id = twot.Id,
                Post = twot.Post,
                Created = twot.Created
            });
        }
        public async Task<string> DeletePost(int id)
        {
            var post = _context.Tweets.Where(e=> e.Id == id).FirstOrDefault();
            if (post != null)
            {
                _context.Tweets.Remove(post);
                await _context.SaveChangesAsync();
            }
            return await Task.FromResult("Tweet has deleted");
        }
        public async Task<Comment> AddComment(CommentInput input)
        {
            
            var comment = new Comment
            {
                TweetId = input.TweetId,
                ProfileId = input.ProfileId,
                Comment1 = input.Comment,
                Created = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }
       public async Task<ProfileData> EditProfile(EditProfileInput input)
       {
            var profile = _context.Profiles.Where(p => p.Id == input.id).FirstOrDefault();
            if (profile != null)
            {
                profile.Fullname = input.Fullname;
                profile.Birth = input.Birth;
                _context.Profiles.Update(profile);
                await _context.SaveChangesAsync();
            }
            return await Task.FromResult(new ProfileData()
            {
               Fullname = profile.Fullname,
               Birth = profile.Birth,
            });
        }
    }
}
