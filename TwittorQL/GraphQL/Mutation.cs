using HotChocolate;
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
    }
}
