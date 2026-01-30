using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial
{
    public class UploadMaterialCommandValidator : AbstractValidator<UploadMaterialCommand>
    {
        public UploadMaterialCommandValidator()
        {
            RuleFor(x => x.LectureId)
                .NotEmpty().WithMessage("Lecture ID is required.");

            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Invalid material type.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Material title is required.")
                .MaximumLength(200).WithMessage("Material title must not exceed 200 characters.");

            RuleFor(x => x.FileUrl)
                .NotEmpty().WithMessage("File URL is required.")
                .MaximumLength(500).WithMessage("File URL must not exceed 500 characters.");
        }
    }
}
