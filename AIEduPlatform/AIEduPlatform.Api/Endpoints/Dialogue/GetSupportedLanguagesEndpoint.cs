using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Dialogue;

public class GetSupportedLanguagesEndpoint : EndpointWithoutRequest<ApiResponse<SupportedLanguagesResult>>
{
    private readonly ITranscriptionService _transcriptionService;

    public GetSupportedLanguagesEndpoint(ITranscriptionService transcriptionService)
        => _transcriptionService = transcriptionService;

    public override void Configure()
    {
        Get("supported-languages");
        Group<DialogueGroup>();
        Summary(s =>
        {
            s.Summary = "Get supported input languages";
            s.Description = "Returns all supported input languages with dialect info and auto-detect capabilities.";
            s.Response<ApiResponse<SupportedLanguagesResult>>(200, "Supported languages info");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var languages = await _transcriptionService.GetSupportedInputLanguagesAsync(ct);
        await SendAsync(ApiResponse<SupportedLanguagesResult>.Ok(languages), 200, ct);
    }
}
