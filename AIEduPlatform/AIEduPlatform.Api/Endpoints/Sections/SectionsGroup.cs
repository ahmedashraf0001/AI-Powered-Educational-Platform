using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Sections;

public sealed class SectionsGroup : Group
{
    public SectionsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Semantic Sections"));
        });
    }
}
