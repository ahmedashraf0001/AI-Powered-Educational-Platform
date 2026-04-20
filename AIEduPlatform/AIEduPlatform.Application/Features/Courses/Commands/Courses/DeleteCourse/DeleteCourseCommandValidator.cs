using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse
{
    public class DeleteCourseCommandValidator : AbstractValidator<DeleteCourseCommand>
    {
        public DeleteCourseCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");

            RuleFor(x => x.Reason)
                .IsInEnum().WithMessage("Invalid course removal reason.");
        }
    }
}
