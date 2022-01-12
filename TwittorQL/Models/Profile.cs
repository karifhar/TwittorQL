using System;
using System.Collections.Generic;

#nullable disable

namespace TwittorQL.Models
{
    public partial class Profile
    {
        public Profile()
        {
            Comments = new HashSet<Comment>();
            Tweets = new HashSet<Tweet>();
        }

        public int Id { get; set; }
        public string Fullname { get; set; }
        public DateTime Birth { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Tweet> Tweets { get; set; }
    }
}
