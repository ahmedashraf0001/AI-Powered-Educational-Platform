namespace AIEduPlatform.Core.Interfaces.Services;

/// <summary>
/// Manages the active LLM provider (Ollama or Groq) at runtime.
/// Thread-safe singleton that allows switching between providers via API endpoint.
/// </summary>
public interface ILlmProviderManager
{
    /// <summary>
    /// Gets the currently active provider name ("ollama" or "groq").
    /// </summary>
    string ActiveProvider { get; }

    /// <summary>
    /// Switches the active provider. Thread-safe.
    /// </summary>
    /// <param name="provider">Provider name: "ollama" or "groq"</param>
    /// <returns>True if the switch was successful, false if the provider name is invalid.</returns>
    bool SwitchProvider(string provider);

    /// <summary>
    /// Gets all supported provider names.
    /// </summary>
    IReadOnlyList<string> SupportedProviders { get; }

    /// <summary>
    /// Checks whether Groq is configured (API key present).
    /// </summary>
    bool IsGroqConfigured { get; }
}
