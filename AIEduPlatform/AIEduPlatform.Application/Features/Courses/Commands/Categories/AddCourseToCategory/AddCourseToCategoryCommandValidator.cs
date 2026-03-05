using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.AddCourseToCategory
{
    public class AddCourseToCategoryCommandValidator : AbstractValidator<AddCourseToCategoryCommand>
    {
        public AddCourseToCategoryCommandValidator()
        {
            RuleFor(x => x.CourseId).NotEmpty().WithMessage("Course ID is required.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category ID is required.");
        }
    }
}
