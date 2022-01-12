namespace TwittorQL.GraphQL.Input
{
    public record CommentInput
    (
        int ProfileId,
        int TweetId,
        string Comment
    );
}
