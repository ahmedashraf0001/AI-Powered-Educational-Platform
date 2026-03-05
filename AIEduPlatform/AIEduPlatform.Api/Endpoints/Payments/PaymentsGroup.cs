using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Payments;

public sealed class PaymentsGroup : Group
{
    public PaymentsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Payments"));
        });
    }
}
