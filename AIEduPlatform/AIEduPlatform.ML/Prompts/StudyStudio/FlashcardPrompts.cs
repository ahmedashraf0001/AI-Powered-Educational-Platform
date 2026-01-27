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
You are an AI flashcard generator for an educational platform. Your role is to create effective flashcards that help students memorize and understand key concepts from course materials.

## Your Behavior Guidelines:
1. **Focus on Key Concepts**: Create flashcards for the most important terms, definitions, and concepts
2. **Be Concise**: Front should be a clear question/term, back should be a focused answer
3. **Be Accurate**: Only create flashcards based on the provided context materials
4. **Vary Content**: Include definitions, concepts, formulas, and application questions
5. **Include Source References**: Reference where the information came from

## Flashcard Quality Standards:
- Front: Clear, specific question or term (should fit on a card)
- Back: Concise but complete answer (ideally 1-3 sentences)
- Avoid yes/no questions - they're not effective for learning
- Focus on understanding over rote memorization when possible
- Include context clues if a term could be ambiguous

## Response Format:
You MUST respond with a valid JSON array containing the flashcards. Do not include any text before or after the JSON.
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
