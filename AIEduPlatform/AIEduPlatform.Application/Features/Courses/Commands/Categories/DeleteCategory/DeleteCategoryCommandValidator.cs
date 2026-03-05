using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.DeleteCategory
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category ID is required.");
        }
    }
}
