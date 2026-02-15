using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Reviews;

public sealed class ReviewsGroup : Group
{
    public ReviewsGroup()
    {
        Configure(string.Empty, ep => ep.Description(x => x.WithTags("Reviews")));
    }
}
