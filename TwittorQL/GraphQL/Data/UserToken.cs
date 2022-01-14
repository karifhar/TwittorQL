namespace TwittorQL.GraphQL.Data
{
#nullable enable
    public record UserToken
    (
        string? Token,
        string? Expired,
        string? Message
    );
}
