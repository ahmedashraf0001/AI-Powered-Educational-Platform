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

        // Populate PreviewUrl so clients know where to fetch audio samples
        var enriched = voices.Select(v =>
            v with { PreviewUrl = $"/api/dialogue/voice-previews?voice_id={Uri.EscapeDataString(v.VoiceId)}" }
        ).ToList();

        await SendAsync(ApiResponse<IReadOnlyList<VoiceInfo>>.Ok(enriched), 200, ct);
    }
}
