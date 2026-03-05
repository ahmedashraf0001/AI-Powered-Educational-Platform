using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Categories;

public sealed class CategoriesGroup : Group
{
    public CategoriesGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Categories"));
        });
    }
}
