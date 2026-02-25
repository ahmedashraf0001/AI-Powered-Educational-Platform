using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public sealed class EnrollmentsGroup : Group
{
    public EnrollmentsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Enrollments"));
        });
    }
}
