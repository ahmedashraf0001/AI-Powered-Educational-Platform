namespace AIEduPlatform.ML.Prompts.Summarization
{
    /// <summary>
    /// Prompt templates for Content Summarization
    /// </summary>
    public static class SummarizationPrompts
    {
        /// <summary>
        /// System instructions for content summarization
        /// </summary>
        public static string SystemInstructions => @"
You are an AI summarization assistant. Create accurate, structured summaries of course materials.

## Rules:
- Use ONLY the source material. Preserve meaning; don't oversimplify.
- Capture main thesis, major topics, key terms, and concept relationships.
- Length: Brief (100-200 words), Moderate (300-500), Detailed (600-1000).

## Format:
The ""summary"" and ""keyPoints"" values MUST use Markdown: headings (##), **bold** key terms, bullet lists, numbered steps, `code`, blockquotes (>) for quotes. Use \n for paragraph breaks.

## CRITICAL: Respond with ONLY a valid JSON object. No text before or after.
";

        /// <summary>
        /// Template for formatting context for summarization
        /// </summary>
        public static string ContextTemplate => @"
## Content to Summarize:

{context_chunks}

---
";

        /// <summary>
        /// Template for summarization user prompt
        /// </summary>
        public static string UserPromptTemplate => @"
## Summarization Request:

- **Summary Length:** {summary_length}
- **Include Key Points:** {include_key_points}

Please summarize the content above according to the specified length.

## Required JSON Response Format:
```json
{
  ""summary"": ""The main summary text as clear paragraphs..."",
  ""keyPoints"": [
    ""Key point 1"",
    ""Key point 2"",
    ""Key point 3""
  ],
  ""keyTerms"": {
    ""Term 1"": ""Definition of term 1"",
    ""Term 2"": ""Definition of term 2""
  },
  ""sourceTitle"": ""Title of the summarized material"",
  ""originalLength"": ""Approximate word count of original"",
  ""summaryLength"": ""Word count of summary""
}
```

Generate the summary now:
";
    }
}
