namespace AIEduPlatform.Core.DTOs.AI.Common
{
    /// <summary>
    /// Difficulty levels for AI-generated content
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard,
        Mixed
    }

    /// <summary>
    /// Question types supported by AI generation
    /// </summary>
    public enum QuestionType
    {
        Mcq,
        TrueFalse,
        ShortAnswer,
        Essay
    }

    /// <summary>
    /// Summary length options
    /// </summary>
    public enum SummaryLength
    {
        /// <summary>
        /// 100-200 words, main points only
        /// </summary>
        Brief,

        /// <summary>
        /// 300-500 words, includes supporting details
        /// </summary>
        Moderate,

        /// <summary>
        /// 600-1000 words, comprehensive coverage
        /// </summary>
        Detailed
    }

    /// <summary>
    /// Roles in conversation
    /// </summary>
    public enum ChatRole
    {
        User,
        Assistant
    }

    /// <summary>
    /// Types of course materials
    /// </summary>
    public enum MaterialType
    {
        Pdf,
        VideoTranscript,
        AudioTranscript,
        Notes,
        Slides,
        Other
    }
}
