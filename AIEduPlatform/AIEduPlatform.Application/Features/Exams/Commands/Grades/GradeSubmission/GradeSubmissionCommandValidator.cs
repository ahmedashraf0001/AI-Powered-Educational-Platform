using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmission
{
    public class GradeSubmissionCommandValidator : AbstractValidator<GradeSubmissionCommand>
    {
        public GradeSubmissionCommandValidator()
        {
            RuleFor(x => x.SubmissionId)
                .NotEmpty().WithMessage("Submission ID is required.");

            RuleForEach(x => x.QuestionGrades)
                .Must(kv => kv.Value >= 0).WithMessage("Question grade must be greater than or equal to 0.");

            RuleFor(x => x.Feedback)
                .MaximumLength(5000).WithMessage("Feedback must not exceed 5000 characters.");
        }
    }
}
