using System;
using System.Collections.Generic;

#nullable disable

namespace KafkaApp.Models
{
    public partial class Tweet
    {
        public Tweet()
        {
            Comments = new HashSet<Comment>();
        }

        public int Id { get; set; }
        public int ProfileId { get; set; }
        public string Post { get; set; }
        public DateTime Created { get; set; }

        public virtual Profile Profile { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
