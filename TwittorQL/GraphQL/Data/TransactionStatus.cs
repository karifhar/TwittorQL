namespace TwittorQL.GraphQL
{
    public record TransactionStatus
    (
        bool IsSucceed,
        string? Message
    );
}
