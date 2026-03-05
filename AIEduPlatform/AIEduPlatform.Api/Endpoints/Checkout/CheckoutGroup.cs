using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Checkout;

public sealed class CheckoutGroup : Group
{
    public CheckoutGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Checkout"));
        });
    }
}
