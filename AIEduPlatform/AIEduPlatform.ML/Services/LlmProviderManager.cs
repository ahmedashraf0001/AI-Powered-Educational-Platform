using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEduPlatform.ML.Services;

/// <summary>
/// Thread-safe singleton that tracks which LLM provider is active.
/// Defaults to "ollama". Can be switched at runtime via API endpoint.
/// </summary>
public class LlmProviderManager : ILlmProviderManager
{
    private static readonly IReadOnlyList<string> _supportedProviders =
        new[] { "ollama", "groq" }.AsReadOnly();

    private readonly ILogger<LlmProviderManager> _logger;
    private readonly bool _groqConfigured;
    private string _activeProvider = "groq";
    private readonly object _lock = new();

    public LlmProviderManager(
        IOptions<AIServiceSettings> settings,
        ILogger<LlmProviderManager> logger)
    {
        _logger = logger;

        // Check if Groq is configured (has API key)
        _groqConfigured = !string.IsNullOrWhiteSpace(settings.Value.Groq?.ApiKey);

        // Set the active provider
        _activeProvider = settings.Value.ActiveProvider;

        _logger.LogInformation(
            "LLM Provider Manager initialized. Active: {Provider}, Groq configured: {GroqConfigured}",
            _activeProvider, _groqConfigured);
    }

    public string ActiveProvider
    {
        get
        {
            lock (_lock) return _activeProvider;
        }
    }

    public IReadOnlyList<string> SupportedProviders => _supportedProviders;

    public bool IsGroqConfigured => _groqConfigured;

    public bool SwitchProvider(string provider)
    {
        var normalized = provider?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(normalized) || !_supportedProviders.Contains(normalized))
        {
            _logger.LogWarning("Invalid provider name: '{Provider}'. Supported: {Supported}",
                provider, string.Join(", ", _supportedProviders));
            return false;
        }

        if (normalized == "groq" && !_groqConfigured)
        {
            _logger.LogWarning("Cannot switch to Groq: API key is not configured");
            return false;
        }

        lock (_lock)
        {
            var previous = _activeProvider;
            _activeProvider = normalized;

            _logger.LogInformation(
                "LLM provider switched: {Previous} → {Current}",
                previous, normalized);
        }

        return true;
    }
}
