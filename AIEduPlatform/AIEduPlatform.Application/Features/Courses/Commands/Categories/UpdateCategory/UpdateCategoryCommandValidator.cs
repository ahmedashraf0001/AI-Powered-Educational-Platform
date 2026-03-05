using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.UpdateCategory
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category ID is required.");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
            RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        }
    }
}
