using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public sealed class NotificationsGroup : Group
{
    public NotificationsGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.Description(x => x.WithTags("Notifications"));
        });
    }
}
