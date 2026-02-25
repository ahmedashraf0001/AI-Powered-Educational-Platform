using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class GetAvailableVoicesEndpoint : EndpointWithoutRequest<ApiResponse<IReadOnlyList<VoiceInfo>>>
{
    private readonly ITranscriptionService _transcriptionService;

    public GetAvailableVoicesEndpoint(ITranscriptionService transcriptionService)
        => _transcriptionService = transcriptionService;

    public override void Configure()
    {
        Get("voices");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Get available TTS voices";
            s.Description = "Returns metadata for all available text-to-speech voices that can be used in dialogue generation.";
            s.Response<ApiResponse<IReadOnlyList<VoiceInfo>>>(200, "List of available voices");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var voices = await _transcriptionService.GetAvailableVoicesAsync(ct);
        await SendAsync(ApiResponse<IReadOnlyList<VoiceInfo>>.Ok(voices), 200, ct);
    }
}
