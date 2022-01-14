using System;

namespace TwittorQL.GraphQL.Input
{
    public record EditProfileInput
    (
        int id,
        string Fullname,
        DateTime Birth
    );
}
