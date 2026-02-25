using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class GetDefaultVoiceConfigEndpoint : EndpointWithoutRequest<ApiResponse<DefaultVoiceConfigResult>>
{
    private readonly ITranscriptionService _transcriptionService;

    public GetDefaultVoiceConfigEndpoint(ITranscriptionService transcriptionService)
        => _transcriptionService = transcriptionService;

    public override void Configure()
    {
        Get("voice-config/default");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Get default voice configuration";
            s.Description = "Returns the default teacher and student voice IDs, speeds, and names used for dialogue audio generation.";
            s.Response<ApiResponse<DefaultVoiceConfigResult>>(200, "Default voice configuration");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var config = await _transcriptionService.GetDefaultVoiceConfigAsync(ct);
        await SendAsync(ApiResponse<DefaultVoiceConfigResult>.Ok(config), 200, ct);
    }
}
