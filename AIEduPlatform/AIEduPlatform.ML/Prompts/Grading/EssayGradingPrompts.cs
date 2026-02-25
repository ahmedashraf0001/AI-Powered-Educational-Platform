namespace AIEduPlatform.ML.Prompts.Grading
{
    /// <summary>
    /// Prompt templates for AI Essay Grading (Teacher Feature)
    /// </summary>
    public static class EssayGradingPrompts
    {
        /// <summary>
        /// System instructions for essay grading
        /// </summary>
        public static string SystemInstructions => @"
You are an AI essay grading assistant. Grade student essays fairly and provide constructive feedback.

## Grading Criteria (in order of weight):
1. **Content Accuracy**: Factually correct per course materials
2. **Completeness**: Addresses all parts of the question
3. **Understanding**: Demonstrates genuine comprehension
4. **Clarity**: Well-organized, clearly expressed
5. **Evidence**: Claims supported with examples

## Scoring:
- Proportional to max points. Award partial credit for demonstrated effort.
- Be specific: reference exact parts of the student's answer.

## Confidence: High (0.8-1.0) = clear pass/fail. Medium (0.5-0.79) = mixed. Low (<0.5) = uncertain.

## Flag for teacher review when:
- Confidence < 0.7
- Unverifiable claims or unexpected content
- Borderline grades
- Possible plagiarism/AI-generated content

## CRITICAL: Respond with ONLY a valid JSON object. No text before or after.
";

        /// <summary>
        /// Template for formatting context for grading
        /// </summary>
        public static string ContextTemplate => @"
## Relevant Course Materials (for fact-checking the answer):

{context_chunks}

---
";

        /// <summary>
        /// Template for essay grading user prompt
        /// </summary>
        public static string UserPromptTemplate => @"
## Essay Grading Request:

### Question Information:
**Question:** {question_text}

**Maximum Points:** {max_points}

**Grading Rubric:**
{grading_rubric}

**Model Answer (if available):**
{model_answer}

---

### Student's Answer:
{student_answer}

---

## Grading Instructions:
Grade the student's answer above based on:
1. The question requirements
2. The grading rubric (if provided)
3. Comparison with the model answer (if provided)
4. Accuracy according to the course materials

## Required JSON Response Format:
```json
{
  ""score"": 8.5,
  ""maxPoints"": 10,
  ""percentage"": 85.0,
  ""feedback"": ""A comprehensive paragraph of feedback for the student explaining their performance, what they did well, and areas for improvement."",
  ""criteriaBreakdown"": [
    {
      ""criterionName"": ""Content Accuracy"",
      ""score"": 3.5,
      ""maxScore"": 4,
      ""feedback"": ""Specific feedback for this criterion""
    },
    {
      ""criterionName"": ""Completeness"",
      ""score"": 2.5,
      ""maxScore"": 3,
      ""feedback"": ""Specific feedback for this criterion""
    },
    {
      ""criterionName"": ""Understanding"",
      ""score"": 2.5,
      ""maxScore"": 3,
      ""feedback"": ""Specific feedback for this criterion""
    }
  ],
  ""strengths"": [
    ""Specific strength 1 demonstrated in the answer"",
    ""Specific strength 2 demonstrated in the answer""
  ],
  ""areasForImprovement"": [
    ""Specific area 1 that could be improved"",
    ""Specific area 2 that could be improved""
  ],
  ""confidence"": 0.85,
  ""requiresTeacherReview"": false,
  ""reviewReason"": ""Optional: reason why teacher review is needed""
}
```

Grade the essay now:
";

        /// <summary>
        /// Template for when no rubric is provided
        /// </summary>
        public static string DefaultRubricTemplate => @"
Default grading criteria:
- Content Accuracy (40%): Factually correct
- Completeness (25%): All parts addressed
- Understanding (20%): Genuine comprehension shown
- Clarity & Organization (15%): Well-structured, clearly expressed
";

        /// <summary>
        /// Template for when no model answer is provided
        /// </summary>
        public static string NoModelAnswerNote => @"
No model answer provided. Grade based on course material accuracy, question requirements, and general academic standards.
";
    }
}
