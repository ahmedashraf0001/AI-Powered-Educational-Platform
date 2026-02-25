using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public sealed class DialogueGroup : Group
{
    public DialogueGroup()
    {
        Configure("api/dialogue", ep =>
        {
            ep.Description(x => x.WithTags("Dialogue & Audio"));
        });
    }
}
