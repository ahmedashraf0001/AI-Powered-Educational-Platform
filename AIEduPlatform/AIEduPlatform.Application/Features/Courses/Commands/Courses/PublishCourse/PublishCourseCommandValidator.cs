using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.PublishCourse
{
    public class PublishCourseCommandValidator : AbstractValidator<PublishCourseCommand>
    {
        public PublishCourseCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
