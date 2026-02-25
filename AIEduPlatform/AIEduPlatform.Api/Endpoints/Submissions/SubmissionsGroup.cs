using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public sealed class SubmissionsGroup : Group
{
    public SubmissionsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Submissions"));
        });
    }
}
