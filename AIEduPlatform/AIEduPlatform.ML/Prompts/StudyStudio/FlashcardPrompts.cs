namespace AIEduPlatform.ML.Prompts.StudyStudio
{
    /// <summary>
    /// Prompt templates for Flashcard Generation in Study Studio
    /// </summary>
    public static class FlashcardPrompts
    {
        /// <summary>
        /// System instructions for flashcard generation
        /// </summary>
        public static string SystemInstructions => @"
You are an AI flashcard generator. Create effective flashcards from course materials.

## Rules:
- Use ONLY the provided context. No invented facts.
- Front: clear question or term. Back: concise answer (1-3 sentences).
- No yes/no questions. Favor understanding over rote memorization.
- Vary content: definitions, concepts, formulas, applications.
- Include source references.

## CRITICAL: Respond with ONLY a valid JSON array. No text, tags, or fences before/after. Start with [.
";

        /// <summary>
        /// Template for formatting context for flashcard generation
        /// </summary>
        public static string ContextTemplate => @"
## Source Materials for Flashcard Generation:

{context_chunks}

---
";

        /// <summary>
        /// Template for flashcard generation user prompt
        /// </summary>
        public static string UserPromptTemplate => @"
## Flashcard Generation Request:
- **Topic:** {topic}
- **Number of Flashcards:** {num_flashcards}

Generate exactly {num_flashcards} flashcards based on the course materials above for the topic: ""{topic}""

## Required JSON Response Format:
```json
[
  {
    ""front"": ""What is [term/concept]?"",
    ""back"": ""[Clear, concise answer/definition]"",
    ""difficulty"": ""medium"",
    ""sourceTitle"": ""Material Title"",
    ""sourceLocation"": ""Page 5""
  }
]
```

Difficulty should be one of: ""easy"", ""medium"", ""hard""

Generate the flashcards now:
";
    }
}
