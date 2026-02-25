namespace AIEduPlatform.ML.Prompts.QuestionGeneration
{
    /// <summary>
    /// Prompt templates for AI Exam Question Generation (Teacher Feature)
    /// </summary>
    public static class ExamQuestionPrompts
    {
        /// <summary>
        /// System instructions for exam question generation
        /// </summary>
        public static string SystemInstructions => @"
You are an AI exam question generator. Create high-quality assessment questions from course materials.

## Rules:
- Use ONLY the provided materials. Include model answers and grading guidance.
- Match the requested difficulty: Easy = recall, Medium = application, Hard = analysis/synthesis.

## Question Standards:
- MCQ: clear stem, 4 plausible options, one correct. No ""all/none of the above."" Plausible distractors.
- True/False: clearly true or false. Include explanation.
- Short Answer: specific question, expected answer + acceptable variations.
- Essay: requires analysis/synthesis, clear expectations, grading rubric.

## CRITICAL: Respond with ONLY a valid JSON array. No text before or after.
";

        /// <summary>
        /// Template for formatting context for question generation
        /// </summary>
        public static string ContextTemplate => @"
## Course Materials for Question Generation:

{context_chunks}

---
";

        /// <summary>
        /// Template for exam question generation user prompt
        /// </summary>
        public static string UserPromptTemplate => @"
## Exam Question Generation Request:

### Parameters:
- **Number of Questions:** {num_questions}
- **Difficulty Level:** {difficulty}
- **Question Types:** {question_types}
- **Focus Topics:** {focus_topics}

### Instructions for the Teacher:
Generate {num_questions} high-quality exam questions based on the course materials above.

## Required JSON Response Format:
```json
[
  {
    ""questionText"": ""The complete question text"",
    ""questionType"": ""mcq"",
    ""options"": [""Option A"", ""Option B"", ""Option C"", ""Option D""],
    ""correctAnswer"": ""Option A"",
    ""explanation"": ""Detailed explanation of why this is correct and why others are wrong"",
    ""difficulty"": ""medium"",
    ""suggestedPoints"": 2,
    ""gradingCriteria"": ""For MCQ: Full points for correct answer, 0 for incorrect"",
    ""sourceTitle"": ""Material Title"",
    ""sourceSection"": ""Section Name"",
    ""sourceLocation"": ""Page 5"",
    ""learningObjective"": ""What skill/knowledge this question assesses""
  }
]
```

### For Essay Questions, include additional fields:
```json
{
  ""questionType"": ""essay"",
  ""modelAnswer"": ""A comprehensive model answer that would receive full marks"",
  ""gradingRubric"": [
    {""criterion"": ""Understanding of concept"", ""maxPoints"": 5, ""description"": ""Shows clear understanding...""},
    {""criterion"": ""Use of examples"", ""maxPoints"": 3, ""description"": ""Provides relevant examples...""},
    {""criterion"": ""Clarity and organization"", ""maxPoints"": 2, ""description"": ""Well-structured response...""}
  ]
}
```

Generate the exam questions now:
";

        /// <summary>
        /// Additional instructions for specific question types
        /// </summary>
        public static string MCQSpecificInstructions => @"
## MCQ Guidelines:
- Options grammatically consistent with stem. Vary correct-answer positions.
- Distractors = real student misconceptions. Similar length to correct answer.
";

        /// <summary>
        /// Additional instructions for essay questions
        /// </summary>
        public static string EssaySpecificInstructions => @"
## Essay Guidelines:
- Require critical thinking (analyze, compare, evaluate, discuss).
- Clear scope and expectations. Include rubric with specific criteria.
- Model answer detailed enough to guide grading.
";
    }
}
