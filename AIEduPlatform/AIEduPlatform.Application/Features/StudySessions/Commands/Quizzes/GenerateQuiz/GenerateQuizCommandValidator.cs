using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.GenerateQuiz
{
    public class GenerateQuizCommandValidator : AbstractValidator<GenerateQuizCommand>
    {
        private static readonly string[] ValidDifficulties = ["easy", "medium", "hard"];
        private static readonly string[] ValidQuestionTypes = ["mcq", "true_false", "short_answer", "essay"];

        public GenerateQuizCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.Topic)
                .MaximumLength(500).WithMessage("Topic must not exceed 500 characters.");

            RuleFor(x => x.NumberOfQuestions)
                .InclusiveBetween(1, 20).WithMessage("Number of questions must be between 1 and 20.");

            RuleFor(x => x.Difficulty)
                .Must(d => ValidDifficulties.Contains(d.ToLowerInvariant()))
                .WithMessage("Difficulty must be 'easy', 'medium', or 'hard'.");

            RuleFor(x => x.QuestionTypes)
                .NotEmpty().WithMessage("At least one question type is required.")
                .Must(types => types.All(t => ValidQuestionTypes.Contains(t.ToLowerInvariant())))
                .WithMessage("Question types must be 'mcq', 'true_false', 'short_answer', or 'essay'.");
        }
    }
}
