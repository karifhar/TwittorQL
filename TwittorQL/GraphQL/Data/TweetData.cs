using System;

namespace TwittorQL.GraphQL.Data
{
    public class TweetData
    {
        public int Id { get; set; }
        public int? ProfileId { get; set; }
        public string Post { get; set; }
        public DateTime Created { get; set; }
    }
}
