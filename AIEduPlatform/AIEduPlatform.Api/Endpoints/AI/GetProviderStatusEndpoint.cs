using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.Interfaces.Services;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.AI;

/// <summary>
/// GET /api/ai/provider — returns current provider status and available options.
/// </summary>
public class GetProviderStatusEndpoint : EndpointWithoutRequest<ApiResponse<ProviderStatusResponse>>
{
    private readonly ILlmProviderManager _providerManager;

    public GetProviderStatusEndpoint(ILlmProviderManager providerManager)
        => _providerManager = providerManager;

    public override void Configure()
    {
        Get("/api/ai/provider");
        Group<AIGroup>();
        Roles("Teacher", "Student", "Admin");
        Summary(s =>
        {
            s.Summary = "Get LLM provider status";
            s.Description = "Returns the active LLM provider and all supported providers.";
            s.Response<ApiResponse<ProviderStatusResponse>>(200, "Provider status");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not authorized");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var status = new ProviderStatusResponse
        {
            ActiveProvider = _providerManager.ActiveProvider,
            SupportedProviders = _providerManager.SupportedProviders.ToList(),
            IsGroqConfigured = _providerManager.IsGroqConfigured
        };

        await SendOkAsync(ApiResponse<ProviderStatusResponse>.Ok(status), ct);
    }
}

public record ProviderStatusResponse
{
    public string ActiveProvider { get; init; } = string.Empty;
    public List<string> SupportedProviders { get; init; } = [];
    public bool IsGroqConfigured { get; init; }
}
