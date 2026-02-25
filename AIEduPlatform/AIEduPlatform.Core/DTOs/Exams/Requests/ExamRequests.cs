using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Exams.Requests
{
    public record CreateExamRequest
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int DurationMinutes { get; init; }
    }

    public record UpdateExamRequest
    {
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int DurationMinutes { get; init; }
    }

    public record AddQuestionRequest
    {
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Options { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
    }

    public record UpdateQuestionRequest
    {
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Options { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
    }

    public record ReorderQuestionsRequest
    {
        public Dictionary<Guid, int> QuestionOrders { get; init; } = new();
    }

    public record GenerateAIQuestionsRequest
    {
        public int NumberOfQuestions { get; init; } = 5;
        public string Difficulty { get; init; } = "medium";
        public List<QuestionType> QuestionTypes { get; init; } = new();
        public List<string>? FocusTopics { get; init; }
        public List<Guid>? LectureIds { get; init; }
        public List<Guid>? MaterialIds { get; init; }
    }

    public record AddBulkQuestionsRequest
    {
        public List<BulkQuestionItemRequest> Questions { get; init; } = new();
    }

    public record BulkQuestionItemRequest
    {
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Options { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
    }

    public record SubmitExamRequest
    {
        public string Answers { get; init; } = string.Empty;
    }

    public record GradeSubmissionRequest
    {
        public float Score { get; init; }
        public string Feedback { get; init; } = string.Empty;
    }

    public record UpdateGradeRequest
    {
        public float Score { get; init; }
        public string Feedback { get; init; } = string.Empty;
    }
}
