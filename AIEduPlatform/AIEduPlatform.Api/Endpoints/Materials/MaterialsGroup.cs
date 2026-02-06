using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Materials;

public sealed class MaterialsGroup : Group
{
    public MaterialsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Materials"));
        });
    }
}
