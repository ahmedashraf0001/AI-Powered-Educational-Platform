using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.AI;

public sealed class AIGroup : Group
{
    public AIGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("AI Provider"));
        });
    }
}
