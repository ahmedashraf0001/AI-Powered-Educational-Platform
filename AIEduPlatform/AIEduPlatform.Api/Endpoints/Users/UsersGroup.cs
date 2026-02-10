using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Users;

public sealed class UsersGroup : Group
{
    public UsersGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Users"));
        });
    }
}
