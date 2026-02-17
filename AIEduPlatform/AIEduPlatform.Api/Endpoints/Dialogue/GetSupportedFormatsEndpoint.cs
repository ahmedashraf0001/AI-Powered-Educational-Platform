using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class GetSupportedFormatsEndpoint : EndpointWithoutRequest<ApiResponse<SupportedFormatsResult>>
{
    private readonly ITranscriptionService _transcriptionService;

    public GetSupportedFormatsEndpoint(ITranscriptionService transcriptionService)
        => _transcriptionService = transcriptionService;

    public override void Configure()
    {
        Get("supported-formats");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Get supported audio formats";
            s.Description = "Returns the list of supported audio formats, max duration, and sample rate for the transcription service.";
            s.Response<ApiResponse<SupportedFormatsResult>>(200, "Supported formats info");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var formats = await _transcriptionService.GetSupportedFormatsAsync(ct);
        await SendAsync(ApiResponse<SupportedFormatsResult>.Ok(formats), 200, ct);
    }
}
