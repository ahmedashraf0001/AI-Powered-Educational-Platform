using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class GetVoicePreviewsRequest
{
    /// <summary>Filter to a specific voice by ID (optional).</summary>
    [QueryParam]
    public string? VoiceId { get; set; }

    /// <summary>Custom sample text to synthesize (optional).</summary>
    [QueryParam]
    public string? SampleText { get; set; }

    /// <summary>Audio format for previews. Default: mp3.</summary>
    [QueryParam]
    public string Format { get; set; } = "mp3";

    /// <summary>Sample rate in Hz. Default: 24000.</summary>
    [QueryParam]
    public int SampleRate { get; set; } = 24000;
}

public class GetVoicePreviewsEndpoint : Endpoint<GetVoicePreviewsRequest, ApiResponse<IReadOnlyList<VoicePreview>>>
{
    private readonly ITranscriptionService _transcriptionService;

    public GetVoicePreviewsEndpoint(ITranscriptionService transcriptionService)
        => _transcriptionService = transcriptionService;

    public override void Configure()
    {
        Get("voice-previews");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Get voice previews with audio samples";
            s.Description = "Returns all available voices with a base64-encoded audio sample each. " +
                            "Optionally filter by voice ID or provide custom sample text.";
            s.Response<ApiResponse<IReadOnlyList<VoicePreview>>>(200, "Voice previews with audio");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetVoicePreviewsRequest req, CancellationToken ct)
    {
        var previews = await _transcriptionService.GetVoicePreviewsAsync(
            voiceId: req.VoiceId,
            sampleText: req.SampleText,
            format: req.Format,
            sampleRate: req.SampleRate,
            ct: ct);

        await SendAsync(ApiResponse<IReadOnlyList<VoicePreview>>.Ok(previews), 200, ct);
    }
}
