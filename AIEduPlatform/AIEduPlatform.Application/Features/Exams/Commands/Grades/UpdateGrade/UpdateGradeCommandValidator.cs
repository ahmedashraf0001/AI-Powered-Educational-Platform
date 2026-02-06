using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.UpdateGrade
{
    public class UpdateGradeCommandValidator : AbstractValidator<UpdateGradeCommand>
    {
        public UpdateGradeCommandValidator()
        {
            RuleFor(x => x.GradeId)
                .NotEmpty().WithMessage("Grade ID is required.");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Score must be greater than or equal to 0.")
                .LessThanOrEqualTo(100).WithMessage("Score must not exceed 100.");

            RuleFor(x => x.Feedback)
                .MaximumLength(5000).WithMessage("Feedback must not exceed 5000 characters.");
        }
    }
}
