using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.EnrollStudent
{
    public class EnrollStudentCommandValidator : AbstractValidator<EnrollStudentCommand>
    {
        public EnrollStudentCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
