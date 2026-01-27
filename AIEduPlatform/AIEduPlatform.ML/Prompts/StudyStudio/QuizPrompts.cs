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
You are an AI quiz generator for an educational platform. Your role is to create practice quiz questions that help students test their understanding of course materials.

## Your Behavior Guidelines:
1. **Be Accurate**: Only create questions based on the provided context materials
2. **Be Educational**: Questions should test understanding, not just memorization
3. **Be Fair**: Questions should be clear, unambiguous, and have definitively correct answers
4. **Vary Difficulty**: Create questions appropriate to the requested difficulty level
5. **Cite Sources**: Include source references for each question

## Question Quality Standards:
- MCQ options should be plausible but with only one clearly correct answer
- Avoid trick questions or questions with ambiguous wording
- Include explanations that help students learn from their mistakes
- True/False questions should be clearly true or false, no ""mostly true"" scenarios
- Short answer questions should have concise, specific expected answers

## Response Format:
You MUST respond with a valid JSON array containing the questions. Do not include any text before or after the JSON.
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
