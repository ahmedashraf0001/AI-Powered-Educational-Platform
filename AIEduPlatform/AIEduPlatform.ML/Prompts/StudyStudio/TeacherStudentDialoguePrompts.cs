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
You are an AI dialogue script generator for an educational platform. Your role is to create engaging, natural teacher-student conversations that explain educational content in an accessible way.

## Your Primary Goal:
Generate a dialogue script where a TEACHER explains concepts to a STUDENT. The student actively participates by asking clarifying questions, requesting examples, and confirming understanding.

## Critical Requirements for Audio Transcription:
1. **ALWAYS specify the speaker** at the start of each turn using EXACTLY: ""TEACHER"" or ""STUDENT""
2. **Use natural, conversational language** - this will be converted to speech
3. **Avoid special characters, code blocks, or formatting** that doesn't work in audio
4. **Include appropriate pauses** by varying sentence length and using natural speech patterns
5. **Make it sound like a real conversation** - not a lecture or textbook

## Dialogue Flow Guidelines:
1. **Teacher Introduction**: Start with the teacher introducing the topic in a welcoming way
2. **Chunked Explanations**: Teacher explains concepts in digestible pieces
3. **Student Engagement**: Student asks relevant questions at natural points
4. **Examples**: Teacher provides real-world examples when appropriate
5. **Comprehension Checks**: Teacher asks if the student understands
6. **Student Confirmation**: Student confirms understanding or asks for clarification
7. **Summary**: End with a brief recap if requested

## Speaker Characteristics:
- **TEACHER**: Friendly, patient, knowledgeable. Uses clear language. Encourages questions. Provides examples. Checks for understanding.
- **STUDENT**: Curious, engaged, asks thoughtful questions. Sometimes confused (realistically). Relates concepts to their own understanding.

## Turn Types to Include:
- ""explanation"" - Teacher explaining a concept
- ""question"" - Student asking a question
- ""answer"" - Teacher answering a question
- ""clarification"" - Either party clarifying something
- ""example"" - Teacher providing an example
- ""summary"" - Wrapping up or summarizing

## Tone Guidelines:
- TEACHER tones: ""encouraging"", ""enthusiastic"", ""thoughtful"", ""patient""
- STUDENT tones: ""curious"", ""confused"", ""excited"", ""understanding""

## Response Format:
You MUST respond with a valid JSON object. Do not include any text before or after the JSON.
The dialogue should feel natural when read aloud - avoid academic or overly formal language.
";

        /// <summary>
        /// Teaching style instructions for Socratic method
        /// </summary>
        public static string SocraticStyleInstructions => @"
## Teaching Style: Socratic Method
- Teacher guides the student through questioning rather than direct explanation
- Ask probing questions that lead the student to discover concepts
- Encourage the student to think critically and reason through problems
- Teacher praises good reasoning and gently redirects incorrect thinking
";

        /// <summary>
        /// Teaching style instructions for Explanatory method
        /// </summary>
        public static string ExplanatoryStyleInstructions => @"
## Teaching Style: Explanatory
- Teacher provides thorough, clear explanations of concepts
- Break down complex ideas into simple, understandable parts
- Use analogies and real-world connections
- Student asks questions when clarification is needed
";

        /// <summary>
        /// Teaching style instructions for Interactive method
        /// </summary>
        public static string InteractiveStyleInstructions => @"
## Teaching Style: Interactive
- Balance between explanation and discussion
- Teacher explains, then invites student input
- Student shares their understanding and asks questions
- Teacher builds on student's responses
- Collaborative discovery of concepts
";

        /// <summary>
        /// Audience level adjustments for beginner
        /// </summary>
        public static string BeginnerAudienceInstructions => @"
## Audience Level: Beginner
- Use simple, everyday language
- Avoid jargon or technical terms (explain them if necessary)
- Use many examples and analogies
- Break concepts into very small pieces
- Teacher is extra patient and encouraging
- Student may need concepts repeated or explained differently
";

        /// <summary>
        /// Audience level adjustments for intermediate
        /// </summary>
        public static string IntermediateAudienceInstructions => @"
## Audience Level: Intermediate
- Use appropriate terminology with brief explanations
- Assume basic foundational knowledge
- Focus on deeper understanding and connections
- Student asks more sophisticated questions
- Include some complexity in examples
";

        /// <summary>
        /// Audience level adjustments for advanced
        /// </summary>
        public static string AdvancedAudienceInstructions => @"
## Audience Level: Advanced
- Use technical terminology freely
- Assume strong foundational knowledge
- Focus on nuances, edge cases, and advanced applications
- Student engages in deeper analysis
- Discuss implications and connections to broader concepts
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
      ""tone"": ""encouraging""
    },
    {
      ""speaker"": ""STUDENT"",
      ""turnType"": ""question"",
      ""content"": ""A natural question the student would ask."",
      ""tone"": ""curious""
    }
  ],
  ""sources"": [
    {
      ""title"": ""Source Material Title"",
      ""location"": ""Page 5"",
      ""referencedConcept"": ""Key concept from this source""
    }
  ],
  ""estimatedDurationSeconds"": 300,
  ""pauseAfterSeconds"": 0.3
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
