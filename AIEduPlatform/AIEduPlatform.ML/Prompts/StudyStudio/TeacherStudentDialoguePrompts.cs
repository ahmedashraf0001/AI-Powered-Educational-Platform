namespace AIEduPlatform.ML.Prompts.StudyStudio
{
    /// <summary>
    /// Prompt templates for Teacher-Student Dialogue Generation in Study Studio.
    /// Generates conversational explanations suitable for audio transcription
    /// with distinct speaker voices.
    /// </summary>
    public static class TeacherStudentDialoguePrompts
    {
        /// <summary>
        /// System instructions for teacher-student dialogue generation
        /// </summary>
        public static string SystemInstructions => @"
You are an AI dialogue script generator. Create natural teacher-student conversations explaining educational content.

## Critical Requirements (for audio transcription):
- Start each turn with EXACTLY ""TEACHER"" or ""STUDENT"".
- Use natural, conversational language suitable for speech.
- No special characters, code blocks, or formatting that doesn't work in audio.
- Vary sentence length for natural pacing.

## Dialogue Structure:
1. Teacher introduces topic warmly.
2. Teacher explains in digestible chunks.
3. Student asks questions at natural points.
4. Teacher gives examples, checks comprehension.
5. Brief recap at end.

## Speakers:
- TEACHER: Friendly, patient, uses clear language, provides examples.
- STUDENT: Curious, engaged, sometimes confused, asks thoughtful questions.

## Turn types: explanation, question, answer, clarification, example, summary.
## Teacher tones: encouraging, enthusiastic, thoughtful, patient.
## Student tones: curious, confused, excited, understanding.

## CRITICAL: Respond with ONLY a valid JSON object. No text before or after.
";

        /// <summary>
        /// Teaching style instructions for Socratic method
        /// </summary>
        public static string SocraticStyleInstructions => @"
## Style: Socratic
Teacher guides through questions, not direct answers. Encourage reasoning. Praise good logic, gently redirect errors.
";

        /// <summary>
        /// Teaching style instructions for Explanatory method
        /// </summary>
        public static string ExplanatoryStyleInstructions => @"
## Style: Explanatory
Teacher gives thorough, clear explanations. Break complex ideas into simple parts. Use analogies. Student asks when clarification is needed.
";

        /// <summary>
        /// Teaching style instructions for Interactive method
        /// </summary>
        public static string InteractiveStyleInstructions => @"
## Style: Interactive
Balance explanation and discussion. Teacher explains, then invites input. Student shares understanding. Collaborative discovery.
";

        /// <summary>
        /// Audience level adjustments for beginner
        /// </summary>
        public static string BeginnerAudienceInstructions => @"
## Level: Beginner
Simple everyday language. Explain all jargon. Many examples and analogies. Small pieces. Extra patient. Student may need repetition.
";

        /// <summary>
        /// Audience level adjustments for intermediate
        /// </summary>
        public static string IntermediateAudienceInstructions => @"
## Level: Intermediate
Use terminology with brief explanations. Assume basic knowledge. Focus on deeper understanding and connections. Include some complexity.
";

        /// <summary>
        /// Audience level adjustments for advanced
        /// </summary>
        public static string AdvancedAudienceInstructions => @"
## Level: Advanced
Use technical terminology freely. Assume strong foundations. Focus on nuances, edge cases, and advanced applications. Deeper analysis.
";

        /// <summary>
        /// Get teaching style instructions based on style name
        /// </summary>
        public static string GetTeachingStyleInstructions(string style)
        {
            return style.ToLowerInvariant() switch
            {
                "socratic" => SocraticStyleInstructions,
                "explanatory" => ExplanatoryStyleInstructions,
                "interactive" => InteractiveStyleInstructions,
                _ => InteractiveStyleInstructions
            };
        }

        /// <summary>
        /// Get audience level instructions based on level name
        /// </summary>
        public static string GetAudienceLevelInstructions(string level)
        {
            return level.ToLowerInvariant() switch
            {
                "beginner" => BeginnerAudienceInstructions,
                "intermediate" => IntermediateAudienceInstructions,
                "advanced" => AdvancedAudienceInstructions,
                _ => IntermediateAudienceInstructions
            };
        }

        /// <summary>
        /// JSON response format template
        /// </summary>
        public static string JsonResponseFormat => @"
## Required JSON Response Format:
```json
{
  ""topic"": ""The main topic being discussed"",
  ""summary"": ""A brief 1-2 sentence summary of what this dialogue covers"",
  ""turns"": [
    {
      ""speaker"": ""TEACHER"",
      ""turnType"": ""explanation"",
      ""content"": ""The actual spoken content here. Keep it natural and conversational."",
      ""tone"": ""encouraging"",
      ""pauseAfterSeconds"": 0.5
    },
    {
      ""speaker"": ""STUDENT"",
      ""turnType"": ""question"",
      ""content"": ""A natural question the student would ask."",
      ""tone"": ""curious"",
      ""pauseAfterSeconds"": 0.3
    }
  ],
  ""sources"": [
    {
      ""title"": ""Source Material Title"",
      ""location"": ""Page 5"",
      ""referencedConcept"": ""Key concept from this source""
    }
  ],
  ""estimatedDurationSeconds"": 300
}
```
";

        /// <summary>
        /// Word count guidelines for dialogue lengths
        /// </summary>
        public static (int minWords, int maxWords, int approxExchanges) GetDialogueLengthGuidelines(string length)
        {
            return length.ToLowerInvariant() switch
            {
                "short" => (300, 500, 3),      // ~2-3 minutes at 150 WPM
                "medium" => (750, 1050, 5),    // ~5-7 minutes
                "long" => (1500, 2250, 8),     // ~10-15 minutes
                _ => (750, 1050, 5)
            };
        }
    }
}
