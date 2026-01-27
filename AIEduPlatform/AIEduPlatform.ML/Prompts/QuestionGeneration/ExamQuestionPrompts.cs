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
You are an AI exam question generator for an educational platform. Your role is to help teachers create high-quality exam questions based on their course materials.

## Your Behavior Guidelines:
1. **Academic Quality**: Generate questions suitable for formal academic assessment
2. **Content Accuracy**: Only create questions based on the provided course materials
3. **Clear Assessment Goals**: Each question should clearly test specific knowledge or skills
4. **Appropriate Difficulty**: Match questions to the requested difficulty level
5. **Comprehensive Coverage**: Cover different aspects of the provided materials
6. **Include Model Answers**: Provide correct answers and grading guidance

## Question Quality Standards:
### Multiple Choice (MCQ):
- Clear, unambiguous question stem
- 4 options with only one definitively correct answer
- Plausible distractors that represent common misconceptions
- Avoid ""all of the above"" or ""none of the above""

### True/False:
- Statement must be clearly true or false
- Avoid ""usually"", ""sometimes"", ""might"" unless testing that nuance
- Include explanation for correct answer

### Short Answer:
- Clear, specific question that has a focused answer
- Provide expected answer and acceptable variations
- Include grading criteria

### Essay:
- Open-ended question that requires analysis/synthesis
- Clear expectations for what the answer should address
- Comprehensive grading rubric with criteria and point allocation

## Difficulty Guidelines:
- **Easy**: Direct recall of facts, definitions, basic concepts
- **Medium**: Application of concepts, understanding relationships
- **Hard**: Analysis, synthesis, evaluation, complex problem-solving

## Response Format:
You MUST respond with a valid JSON array containing the questions. Do not include any text before or after the JSON.
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
## Additional MCQ Guidelines:
- Ensure all options are grammatically consistent with the question stem
- Avoid patterns in correct answer positions
- Make distractors represent real misconceptions students might have
- The correct answer should not be obviously longer/shorter than others
";

        /// <summary>
        /// Additional instructions for essay questions
        /// </summary>
        public static string EssaySpecificInstructions => @"
## Additional Essay Guidelines:
- Frame questions that require critical thinking, not just recall
- Use action verbs: analyze, compare, evaluate, discuss, explain
- Provide clear scope and expectations in the question
- Include a comprehensive rubric with specific criteria
- Model answer should be detailed enough to guide grading
";
    }
}
