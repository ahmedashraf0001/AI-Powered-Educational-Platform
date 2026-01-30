using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse
{
    public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
    {
        public CreateCourseCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Course title is required.")
                .MaximumLength(200).WithMessage("Course title must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Course description is required.")
                .MaximumLength(2000).WithMessage("Course description must not exceed 2000 characters.");
        }
    }
}
