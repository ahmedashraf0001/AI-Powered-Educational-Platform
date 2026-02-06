using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.AddQuestion
{
    public class AddQuestionCommandValidator : AbstractValidator<AddQuestionCommand>
    {
        public AddQuestionCommandValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid question type.");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Question text is required.")
                .MaximumLength(2000).WithMessage("Question text must not exceed 2000 characters.");

            RuleFor(x => x.CorrectAnswer)
                .NotEmpty().WithMessage("Correct answer is required.")
                .MaximumLength(1000).WithMessage("Correct answer must not exceed 1000 characters.");

            RuleFor(x => x.Points)
                .GreaterThan(0).WithMessage("Points must be greater than 0.");
        }
    }
}
