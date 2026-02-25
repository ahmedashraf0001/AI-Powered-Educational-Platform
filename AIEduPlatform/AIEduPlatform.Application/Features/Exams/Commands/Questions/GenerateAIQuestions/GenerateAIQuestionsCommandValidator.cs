using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.GenerateAIQuestions
{
    public class GenerateAIQuestionsCommandValidator : AbstractValidator<GenerateAIQuestionsCommand>
    {
        public GenerateAIQuestionsCommandValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");

            RuleFor(x => x.NumberOfQuestions)
                .InclusiveBetween(1, 20).WithMessage("Number of questions must be between 1 and 20.");

            RuleFor(x => x.Difficulty)
                .NotEmpty().WithMessage("Difficulty is required.")
                .Must(x => x is "easy" or "medium" or "hard")
                .WithMessage("Difficulty must be 'easy', 'medium', or 'hard'.");

            RuleFor(x => x.QuestionTypes)
                .NotEmpty().WithMessage("At least one question type is required.")
                .Must(types => types.All(t => Enum.IsDefined(t)))
                .WithMessage("Invalid question type specified.");
        }
    }
}
