using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Grades;

public sealed class GradesGroup : Group
{
    public GradesGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Grades"));
        });
    }
}
