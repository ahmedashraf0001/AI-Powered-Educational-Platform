namespace AIEduPlatform.Core.DTOs.AI.Requests.Dialogue
{
    /// <summary>
    /// Request for Teacher-Student Dialogue Generation.
    /// Maps to PromptBuilder.BuildTeacherStudentDialoguePrompt parameters.
    /// </summary>
    public class TeacherStudentDialogueRequest : RequestBase
    {
        /// <summary>
        /// The study session ID for tracking
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The specific topic to explain in the dialogue.
        /// If not provided, the dialogue will cover the main concepts from the context.
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Target audience level: "beginner", "intermediate", "advanced"
        /// Affects the complexity of explanations and questions
        /// </summary>
        public string AudienceLevel { get; set; } = "intermediate";

        /// <summary>
        /// Approximate number of dialogue exchanges (teacher-student pairs)
        /// One exchange = teacher explanation + student question + teacher answer
        /// Default: 5 exchanges
        /// </summary>
        public int NumberOfExchanges { get; set; } = 5;

        /// <summary>
        /// Target dialogue length: "short" (2-3 min), "medium" (5-7 min), "long" (10-15 min)
        /// </summary>
        public string DialogueLength { get; set; } = "medium";

        /// <summary>
        /// Whether to include examples in the explanations
        /// </summary>
        public bool IncludeExamples { get; set; } = true;

        /// <summary>
        /// Whether to include a summary at the end
        /// </summary>
        public bool IncludeSummary { get; set; } = true;

        /// <summary>
        /// The teaching style to use: "socratic", "explanatory", "interactive"
        /// - socratic: Teacher guides through questions
        /// - explanatory: Teacher explains thoroughly
        /// - interactive: Mix of explanation and discussion
        /// </summary>
        public string TeachingStyle { get; set; } = "interactive";

        /// <summary>
        /// Specific concepts or terms the student should ask about
        /// If not provided, the LLM will determine what questions are appropriate
        /// </summary>
        public List<string>? FocusConcepts { get; set; }
    }
}
