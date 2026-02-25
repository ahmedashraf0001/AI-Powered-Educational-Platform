namespace AIEduPlatform.ML.Prompts.StudyStudio
{
    /// <summary>
    /// Prompt templates for Practice Quiz Generation in Study Studio
    /// </summary>
    public static class QuizPrompts
    {
        /// <summary>
        /// System instructions for quiz generation
        /// </summary>
        public static string SystemInstructions => @"
You are an AI quiz generator. Create practice questions from course materials.

## Rules:
- Use ONLY the provided context. Include source references.
- Questions must be clear, unambiguous, with one correct answer.
- MCQ: plausible distractors, no trick questions. True/False: clearly true or false.
- Include explanations to help students learn from mistakes.
- Match the requested difficulty level.

## CRITICAL: Respond with ONLY a valid JSON array. No text before or after.
";

        /// <summary>
        /// Template for formatting context for quiz generation
        /// </summary>
        public static string ContextTemplate => @"
## Source Materials for Quiz Generation:

{context_chunks}

---
";

        /// <summary>
        /// Template for quiz generation user prompt
        /// </summary>
        public static string UserPromptTemplate => @"
## Quiz Generation Request:
- **Topic:** {topic}
- **Number of Questions:** {num_questions}
- **Difficulty Level:** {difficulty}
- **Question Types Requested:** {question_types}

Generate exactly {num_questions} practice quiz questions based on the course materials above.

## Required JSON Response Format:
```json
[
  {
    ""questionText"": ""The question text here"",
    ""questionType"": ""mcq"",
    ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
    ""correctAnswer"": ""Option A"",
    ""explanation"": ""Explanation of why this is correct"",
    ""difficulty"": ""medium"",
    ""suggestedPoints"": 2,
    ""sourceTitle"": ""Material Title"",
    ""sourceLocation"": ""Page 5""
  }
]
```

For true_false questions, use options: [""True"", ""False""]
For short_answer questions, options should be null and correctAnswer should be the expected answer.

Generate the questions now:
";
    }
}
