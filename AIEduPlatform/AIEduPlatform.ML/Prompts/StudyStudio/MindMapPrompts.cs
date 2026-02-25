namespace AIEduPlatform.ML.Prompts.StudyStudio
{
    /// <summary>
    /// Prompt templates for Mind Map Generation in Study Studio
    /// </summary>
    public static class MindMapPrompts
    {
        /// <summary>
        /// System instructions for mind map generation
        /// </summary>
        public static string SystemInstructions => @"
You are an AI mind map generator. Create hierarchical concept maps from course materials.

## Rules:
- Use ONLY the provided context.
- Central node: main topic (1-3 words). Level 1: 2-5 major subtopics. Level 2+: key concepts and details.
- Node labels: short (1-5 words) with optional description.
- Balance depth and breadth across branches.

## CRITICAL: Respond with ONLY a valid JSON object. No text before or after.
";

        /// <summary>
        /// Template for formatting context for mind map generation
        /// </summary>
        public static string ContextTemplate => @"
## Source Materials for Mind Map Generation:

{context_chunks}

---
";

        /// <summary>
        /// Template for mind map generation user prompt
        /// </summary>
        public static string UserPromptTemplate => @"
## Mind Map Generation Request:
- **Central Topic:** {central_topic}
- **Maximum Depth:** {max_depth} levels

Generate a comprehensive mind map for the topic ""{central_topic}"" based on the course materials above.

## Required JSON Response Format:
```json
{
  ""id"": ""root"",
  ""label"": ""Central Topic"",
  ""description"": ""Brief description of the central topic"",
  ""children"": [
    {
      ""id"": ""branch1"",
      ""label"": ""Subtopic 1"",
      ""description"": ""Description of subtopic"",
      ""sourceTitle"": ""Material Title"",
      ""sourceLocation"": ""Page 5"",
      ""children"": [
        {
          ""id"": ""branch1-1"",
          ""label"": ""Concept"",
          ""description"": ""Detailed explanation"",
          ""children"": []
        }
      ]
    }
  ]
}
```

Generate the mind map now:
";
    }
}
