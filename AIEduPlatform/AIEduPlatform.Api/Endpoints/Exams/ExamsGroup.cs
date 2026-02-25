using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Exams;

public sealed class ExamsGroup : Group
{
    public ExamsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Exams"));
        });
    }
}
