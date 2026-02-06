using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.AddBulkQuestions
{
    public class AddBulkQuestionsCommandValidator : AbstractValidator<AddBulkQuestionsCommand>
    {
        public AddBulkQuestionsCommandValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");

            RuleFor(x => x.Questions)
                .NotEmpty().WithMessage("At least one question is required.");

            RuleForEach(x => x.Questions).ChildRules(q =>
            {
                q.RuleFor(x => x.Type)
                    .IsInEnum().WithMessage("Invalid question type.");

                q.RuleFor(x => x.Text)
                    .NotEmpty().WithMessage("Question text is required.")
                    .MaximumLength(2000).WithMessage("Question text must not exceed 2000 characters.");

                q.RuleFor(x => x.CorrectAnswer)
                    .NotEmpty().WithMessage("Correct answer is required.")
                    .MaximumLength(1000).WithMessage("Correct answer must not exceed 1000 characters.");

                q.RuleFor(x => x.Points)
                    .GreaterThan(0).WithMessage("Points must be greater than 0.");
            });
        }
    }
}
