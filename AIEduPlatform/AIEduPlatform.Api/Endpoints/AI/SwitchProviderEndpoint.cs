using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.Interfaces.Services;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.AI;

/// <summary>
/// POST /api/ai/provider/switch — switches the active LLM provider at runtime.
/// </summary>
public class SwitchProviderEndpoint : Endpoint<SwitchProviderRequest, ApiResponse<SwitchProviderResponse>>
{
    private readonly ILlmProviderManager _providerManager;

    public SwitchProviderEndpoint(ILlmProviderManager providerManager)
        => _providerManager = providerManager;

    public override void Configure()
    {
        Post("/api/ai/provider/switch");
        Group<AIGroup>();
        Roles("Teacher", "Student", "Admin");
        Summary(s =>
        {
            s.Summary = "Switch LLM provider";
            s.Description = "Switches the active LLM provider between 'ollama' and 'groq'. " +
                            "Groq requires a valid API key to be configured in appsettings.";
            s.ExampleRequest = new SwitchProviderRequest { Provider = "groq" };
            s.Response<ApiResponse<SwitchProviderResponse>>(200, "Provider switched successfully");
            s.Response(400, "Invalid provider or Groq not configured");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not authorized");
        });
    }

    public override async Task HandleAsync(SwitchProviderRequest req, CancellationToken ct)
    {
        var previousProvider = _providerManager.ActiveProvider;
        var success = _providerManager.SwitchProvider(req.Provider);

        if (!success)
        {
            var supported = string.Join(", ", _providerManager.SupportedProviders);

            var errorMessage = req.Provider?.Trim().ToLowerInvariant() == "groq" && !_providerManager.IsGroqConfigured
                ? "Cannot switch to Groq: API key is not configured. Set 'AIService:Groq:ApiKey' in appsettings.json."
                : $"Invalid provider '{req.Provider}'. Supported providers: {supported}";

            await SendAsync(ApiResponse<SwitchProviderResponse>.Fail(errorMessage), 400, ct);
            return;
        }

        var response = new SwitchProviderResponse
        {
            PreviousProvider = previousProvider,
            ActiveProvider = _providerManager.ActiveProvider,
            Message = $"Successfully switched from '{previousProvider}' to '{_providerManager.ActiveProvider}'"
        };

        await SendOkAsync(ApiResponse<SwitchProviderResponse>.Ok(response, response.Message), ct);
    }
}

public record SwitchProviderRequest
{
    /// <summary>
    /// The provider to switch to: "ollama" or "groq"
    /// </summary>
    public string Provider { get; init; } = string.Empty;
}

public record SwitchProviderResponse
{
    public string PreviousProvider { get; init; } = string.Empty;
    public string ActiveProvider { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
