using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Courses;

public sealed class CoursesGroup : Group
{
    public CoursesGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Courses"));
        });
    }
}
