using AIEduPlatform.Application.Features.Users.Commands.DeleteVoiceSettings;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class DeleteVoiceSettingsEndpoint : EndpointWithoutRequest<ApiResponse<string>>
{
    private readonly IMediator _mediator;

    public DeleteVoiceSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("voice-settings");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Reset voice settings to defaults";
            s.Description = "Deletes the current user's persisted voice settings, " +
                            "so future dialogue generation will use the system defaults.";
            s.Response<ApiResponse<string>>(200, "Settings reset");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await _mediator.Send(new DeleteVoiceSettingsCommand(), ct);
        await SendAsync(ApiResponse<string>.Ok("Voice settings reset to defaults."), 200, ct);
    }
}
