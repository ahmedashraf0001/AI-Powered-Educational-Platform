using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public sealed class StudySessionsGroup : Group
{
    public StudySessionsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Study Sessions"));
        });
    }
}
