using AIEduPlatform.Application.Features.Users.Commands.SaveVoiceSettings;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Users;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class SaveVoiceSettingsEndpoint : Endpoint<SaveVoiceSettingsCommand, ApiResponse<UserVoiceSettingsDto>>
{
    private readonly IMediator _mediator;

    public SaveVoiceSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("voice-settings");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Save user voice settings";
            s.Description = "Creates or updates the current user's voice/audio settings for dialogue generation. " +
                            "These settings persist and are automatically applied whenever dialogue audio is generated.";
            s.ExampleRequest = new SaveVoiceSettingsCommand
            {
                TeacherVoiceId = "Damien Black",
                StudentVoiceId = "Daisy Studious",
                TeacherSpeed = 0.95,
                StudentSpeed = 1.0,
                OutputFormat = "mp3",
                SampleRate = 24000,
                IncludePauses = true,
                PauseDurationMs = 500,
                PauseMultiplier = 1.0,
                NormalizeAudio = true
            };
            s.Response<ApiResponse<UserVoiceSettingsDto>>(200, "Settings saved");
            s.Response(400, "Validation error");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(SaveVoiceSettingsCommand req, CancellationToken ct)
    {
        var result = await _mediator.Send(req, ct);
        await SendAsync(ApiResponse<UserVoiceSettingsDto>.Ok(result, "Voice settings saved successfully."), 200, ct);
    }
}
