using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Simple mind map node DTO matching the exact JSON format returned by AI prompts
/// </summary>
public record MindMapNode
{
    /// <summary>
    /// Unique identifier for this node
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The label/text of this node (1-5 words)
    /// </summary>
    [JsonPropertyName("label")]
    public string Label { get; init; } = string.Empty;

    /// <summary>
    /// Brief description or details about this node
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Title of the source material (optional for child nodes)
    /// </summary>
    [JsonPropertyName("sourceTitle")]
    public string? SourceTitle { get; init; }

    /// <summary>
    /// Location within the source material (optional)
    /// </summary>
    [JsonPropertyName("sourceLocation")]
    public string? SourceLocation { get; init; }

    /// <summary>
    /// Child nodes (branches) of this node
    /// </summary>
    [JsonPropertyName("children")]
    public List<MindMapNode> Children { get; init; } = new();
}
