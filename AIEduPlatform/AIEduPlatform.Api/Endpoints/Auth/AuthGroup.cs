using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Auth;

public sealed class AuthGroup : Group
{
    public AuthGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Auth"));
        });
    }
}
