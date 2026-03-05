namespace AIEduPlatform.ML.Prompts.Sections
{
    public static class SemanticSectionPrompts
    {
        public static string SystemInstructions => @"
You are an AI content analysis assistant. Your task is to analyze educational content and identify meaningful semantic sections — coherent topic boundaries within the material.

## Rules:
- Analyze the provided transcript/content and group it into logical, self-contained sections.
- Each section should cover a single coherent topic or subtopic.
- Sections must be contiguous and non-overlapping — every part of the content belongs to exactly one section.
- Keep section titles concise but descriptive (5-10 words).
- Write summaries that capture the key points of each section (1-3 sentences).
- Preserve the chronological order of the content.

## CRITICAL: Respond with ONLY a valid JSON object. No text before or after.
";

        public static string VideoAudioUserPromptTemplate => @"
## Timestamped Transcript:

{transcript}

---

## Task:
Analyze this timestamped transcript and group the content into meaningful semantic sections.
Each section should represent a coherent topic or subtopic discussed in the material.
Use the timestamps to determine section boundaries.

## Required JSON Response Format:
```json
{
  ""sections"": [
    {
      ""title"": ""Section title describing the topic"",
      ""start"": ""MM:SS"",
      ""end"": ""MM:SS"",
      ""summary"": ""Brief summary of what this section covers.""
    }
  ]
}
```

Identify the semantic sections now:
";

        public static string DocumentUserPromptTemplate => @"
## Document Content (by page):

{page_content}

---

## Task:
Analyze this document content and group it into meaningful semantic sections based on page ranges.
Each section should represent a coherent topic or subtopic covered in the document.

## Required JSON Response Format:
```json
{
  ""sections"": [
    {
      ""title"": ""Section title describing the topic"",
      ""startPage"": 1,
      ""endPage"": 3,
      ""summary"": ""Brief summary of what this section covers.""
    }
  ]
}
```

Identify the semantic sections now:
";
    }
}
