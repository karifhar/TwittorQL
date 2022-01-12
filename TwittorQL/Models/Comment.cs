using System;
using System.Collections.Generic;

#nullable disable

namespace TwittorQL.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public int TweetId { get; set; }
        public int ProfileId { get; set; }
        public string Comment1 { get; set; }
        public DateTime Created { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual Tweet Tweet { get; set; }
    }
}
