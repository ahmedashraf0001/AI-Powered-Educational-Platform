using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.SubmitQuizAnswers
{
    public class SubmitQuizAnswersCommandValidator : AbstractValidator<SubmitQuizAnswersCommand>
    {
        public SubmitQuizAnswersCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.QuizId)
                .NotEmpty().WithMessage("Quiz ID is required.");

            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("At least one answer is required.");
        }
    }
}
