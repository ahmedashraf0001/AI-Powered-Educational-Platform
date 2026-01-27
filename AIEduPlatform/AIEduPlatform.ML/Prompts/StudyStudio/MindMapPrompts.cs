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
You are an AI mind map generator for an educational platform. Your role is to create visual mind maps that help students understand the relationships between concepts in course materials.

## Your Behavior Guidelines:
1. **Hierarchical Structure**: Create a clear hierarchy from central topic to subtopics to details
2. **Meaningful Connections**: Show how concepts relate to each other
3. **Be Comprehensive**: Cover all major aspects of the topic from the materials
4. **Be Concise**: Keep node labels short (1-5 words) with optional descriptions
5. **Balance Depth and Breadth**: Don't go too deep on one branch while ignoring others

## Mind Map Quality Standards:
- Central node: The main topic (1-3 words)
- Level 1 branches: Major subtopics or categories (2-5 branches)
- Level 2 branches: Key concepts within each subtopic
- Level 3+ branches: Supporting details, examples, or specifics
- Each node should have a brief label and optional description

## Response Format:
You MUST respond with a valid JSON object representing the mind map structure. Do not include any text before or after the JSON.
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
