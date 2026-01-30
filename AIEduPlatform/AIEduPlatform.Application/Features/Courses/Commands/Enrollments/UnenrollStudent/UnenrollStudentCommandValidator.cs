using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent
{
    public class UnenrollStudentCommandValidator : AbstractValidator<UnenrollStudentCommand>
    {
        public UnenrollStudentCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
