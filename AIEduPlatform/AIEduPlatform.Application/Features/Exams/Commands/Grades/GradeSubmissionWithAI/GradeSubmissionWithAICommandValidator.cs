using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmissionWithAI
{
    public class GradeSubmissionWithAICommandValidator : AbstractValidator<GradeSubmissionWithAICommand>
    {
        public GradeSubmissionWithAICommandValidator()
        {
            RuleFor(x => x.SubmissionId)
                .NotEmpty().WithMessage("Submission ID is required.");
        }
    }
}
