using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services.Utilities;

/// <summary>
/// Reformulates conversational user queries into information-seeking queries
/// that are better suited for embedding search and cross-encoder reranking.
/// 
/// Problem: Users often send conversational messages like "can you help me study rs-485"
/// or "I don't understand UART". These perform poorly with embedding similarity search
/// and cross-encoder rerankers (e.g., bge-reranker-base) because the model measures
/// semantic relevance between the query and passage content — and conversational phrasing
/// doesn't match factual content well.
/// 
/// Solution: Strip conversational fluff and extract the core informational intent.
/// "can you help me study rs-485" → "RS-485 overview key concepts"
/// "explain what UART is"         → "What is UART"
/// </summary>
public static class QueryReformulator
{
    /// <summary>
    /// Conversational prefixes/patterns that add no informational value for retrieval.
    /// Ordered from most specific to least to avoid partial matches.
    /// </summary>
    private static readonly string[] ConversationalPrefixes = new[]
    {
        // Polite requests
        "can you help me understand",
        "can you help me study",
        "can you help me learn",
        "can you help me with",
        "can you explain to me",
        "can you tell me about",
        "can you teach me about",
        "can you teach me",
        "could you help me understand",
        "could you help me study",
        "could you help me learn",
        "could you help me with",
        "could you explain to me",
        "could you tell me about",
        "could you teach me about",
        "could you teach me",
        "would you help me understand",
        "would you help me study",
        "would you help me learn",
        "would you help me with",
        "would you explain",
        "would you tell me about",
        "please help me understand",
        "please help me study",
        "please help me learn",
        "please help me with",
        "please explain to me",
        "please tell me about",
        "please teach me about",
        "please teach me",
        "help me understand",
        "help me study",
        "help me learn about",
        "help me learn",
        "help me with",

        // Simpler request patterns
        "i want to learn about",
        "i want to understand",
        "i want to study",
        "i want to know about",
        "i want to know",
        "i need to learn about",
        "i need to understand",
        "i need to study",
        "i need to know about",
        "i need help with",
        "i need help understanding",
        "i'd like to learn about",
        "i'd like to understand",
        "i'd like to know about",

        // Direct command patterns
        "tell me about",
        "tell me more about",
        "teach me about",
        "teach me",
        "explain to me",
        "explain me",
        "show me",
        "give me information about",
        "give me info about",
        "give me details about",
        "give me an overview of",
        "give me a summary of",

        // Filler patterns
        "i don't understand",
        "i don't get",
        "i'm confused about",
        "i'm struggling with",
        "i'm having trouble with",
        "i'm not sure about",
    };

    /// <summary>
    /// Trailing filler phrases to strip.
    /// </summary>
    private static readonly string[] TrailingFillers = new[]
    {
        "please",
        "thanks",
        "thank you",
        "if you can",
        "if possible",
        "for me",
    };

    /// <summary>
    /// Reformulates a user query for better retrieval performance.
    /// Strips conversational fluff while preserving the core informational intent.
    /// </summary>
    /// <param name="query">The raw user query</param>
    /// <returns>A reformulated query optimized for embedding search and reranking</returns>
    public static string Reformulate(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return query;

        var cleaned = query.Trim();

        // Remove leading question marks / punctuation patterns
        cleaned = cleaned.TrimStart('?', '!', '.', ',');

        // Strip conversational prefixes (case-insensitive)
        foreach (var prefix in ConversationalPrefixes)
        {
            if (cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(prefix.Length).TrimStart(' ', ',', ':');
                break; // Only strip the first (most specific) match
            }
        }

        // Strip trailing filler phrases
        foreach (var filler in TrailingFillers)
        {
            if (cleaned.EndsWith(filler, StringComparison.OrdinalIgnoreCase))
            {
                cleaned = cleaned.Substring(0, cleaned.Length - filler.Length).TrimEnd(' ', ',', '.');
            }
        }

        // Clean up any remaining artifacts
        cleaned = cleaned.Trim(' ', '?', '!', '.', ',');

        // If we stripped everything (e.g., "help me"), return original
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length < 3)
            return query.Trim();

        return cleaned;
    }
}
