namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record QuizResultDto
    {
        public Guid QuizId { get; init; }
        public float Score { get; init; }
        public int TotalQuestions { get; init; }
        public int CorrectCount { get; init; }
        public List<QuizAnswerResultDto> Results { get; init; } = new();
    }

    public record QuizAnswerResultDto
    {
        public int QuestionIndex { get; init; }
        public string StudentAnswer { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public bool IsCorrect { get; init; }
        public string Explanation { get; init; } = string.Empty;
        public float? AiScore { get; init; }
        public string? AiFeedback { get; init; }
    }
}
