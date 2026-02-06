using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public sealed class LecturesGroup : Group
{
    public LecturesGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Lectures"));
        });
    }
}
