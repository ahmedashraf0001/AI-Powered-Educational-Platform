using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UpdateMaterialProgress
{
    public class UpdateMaterialProgressCommandValidator : AbstractValidator<UpdateMaterialProgressCommand>
    {
        public UpdateMaterialProgressCommandValidator()
        {
            RuleFor(x => x.MaterialId).NotEmpty().WithMessage("Material ID is required.");
            RuleFor(x => x.Position).GreaterThanOrEqualTo(0).WithMessage("Position must be non-negative.");
        }
    }
}
