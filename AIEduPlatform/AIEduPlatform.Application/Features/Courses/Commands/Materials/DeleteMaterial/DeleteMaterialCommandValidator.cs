using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.DeleteMaterial
{
    public class DeleteMaterialCommandValidator : AbstractValidator<DeleteMaterialCommand>
    {
        public DeleteMaterialCommandValidator()
        {
            RuleFor(x => x.MaterialId)
                .NotEmpty().WithMessage("Material ID is required.");
        }
    }
}
