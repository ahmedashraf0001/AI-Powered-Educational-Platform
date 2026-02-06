using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.ApproveGrade
{
    public class ApproveGradeCommandValidator : AbstractValidator<ApproveGradeCommand>
    {
        public ApproveGradeCommandValidator()
        {
            RuleFor(x => x.GradeId)
                .NotEmpty().WithMessage("Grade ID is required.");
        }
    }
}
