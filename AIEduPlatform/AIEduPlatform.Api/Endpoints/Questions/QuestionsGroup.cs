using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Questions;

public sealed class QuestionsGroup : Group
{
    public QuestionsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Questions"));
        });
    }
}
