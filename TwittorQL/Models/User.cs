using System;
using System.Collections.Generic;

#nullable disable

namespace TwittorQL.Models
{
    public partial class User
    {
        public User()
        {
            UserRoles = new HashSet<UserRole>();
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
