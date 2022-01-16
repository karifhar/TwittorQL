namespace TwittorQL.GraphQL.Input
{
    public record ChangePassInput
    (
        string username,
        string Password,
        string NewPassword
    );
}
