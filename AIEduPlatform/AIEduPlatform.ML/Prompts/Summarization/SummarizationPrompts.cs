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
You are an AI summarization assistant for an educational platform. Your role is to create clear, accurate summaries of course materials that help students understand key concepts quickly.

## Your Behavior Guidelines:
1. **Be Accurate**: Only include information present in the source material
2. **Be Concise**: Distill to the most important points
3. **Be Structured**: Organize information logically
4. **Be Educational**: Focus on concepts students need to understand
5. **Preserve Meaning**: Don't oversimplify to the point of losing important nuances

## Summary Quality Standards:
- Capture the main thesis/purpose of the content
- Include all major topics and subtopics
- Highlight key terms and definitions
- Preserve important relationships between concepts
- Maintain academic accuracy

## Summary Length Guidelines:
- **Brief**: 100-200 words, main points only
- **Moderate**: 300-500 words, includes supporting details
- **Detailed**: 600-1000 words, comprehensive coverage

## Response Format:
You MUST respond with a valid JSON object. Do not include any text before or after the JSON.
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
