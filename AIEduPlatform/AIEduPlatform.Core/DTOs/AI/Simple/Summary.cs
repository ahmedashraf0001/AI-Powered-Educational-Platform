using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Simple summarization DTO matching the exact JSON format returned by AI prompts
/// </summary>
public record Summary
{
    /// <summary>
    /// The main summary text as clear paragraphs
    /// </summary>
    [JsonPropertyName("summary")]
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Key points extracted from the content
    /// </summary>
    [JsonPropertyName("keyPoints")]
    public List<string> KeyPoints { get; init; } = new();

    /// <summary>
    /// Key terms and their definitions
    /// </summary>
    [JsonPropertyName("keyTerms")]
    public Dictionary<string, string> KeyTerms { get; init; } = new();

    /// <summary>
    /// Title of the summarized material
    /// </summary>
    [JsonPropertyName("sourceTitle")]
    public string SourceTitle { get; init; } = string.Empty;

    /// <summary>
    /// Approximate word count of original content
    /// </summary>
    [JsonPropertyName("originalLength")]
    public string OriginalLength { get; init; } = string.Empty;

    /// <summary>
    /// Word count of the summary
    /// </summary>
    [JsonPropertyName("summaryLength")]
    public string SummaryLength { get; init; } = string.Empty;
}
