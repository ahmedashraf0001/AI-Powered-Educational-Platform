using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Cart;

public sealed class CartGroup : Group
{
    public CartGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Cart"));
        });
    }
}
