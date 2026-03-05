using AIEduPlatform.Application.Features.Users.Queries.GetVoiceSettings;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Users;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class GetVoiceSettingsEndpoint : EndpointWithoutRequest<ApiResponse<UserVoiceSettingsDto>>
{
    private readonly IMediator _mediator;

    public GetVoiceSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("voice-settings");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Get user voice settings";
            s.Description = "Returns the current user's persisted voice/audio settings for dialogue generation. " +
                            "Returns defaults if the user has not saved any settings yet.";
            s.Response<ApiResponse<UserVoiceSettingsDto>>(200, "User voice settings");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVoiceSettingsQuery(), ct);
        await SendAsync(ApiResponse<UserVoiceSettingsDto>.Ok(result), 200, ct);
    }
}
