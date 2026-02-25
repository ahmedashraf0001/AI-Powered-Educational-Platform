using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial
{
    public class UploadMaterialCommandValidator : AbstractValidator<UploadMaterialCommand>
    {
        public UploadMaterialCommandValidator()
        {
            RuleFor(x => x.LectureId)
                .NotEmpty().WithMessage("Lecture ID is required.");

            RuleFor(x => x.Files)
                .NotEmpty().WithMessage("At least one file is required.");

            RuleForEach(x => x.Files).ChildRules(file =>
            {
                file.RuleFor(f => f.Title)
                    .NotEmpty().WithMessage("Material title is required.")
                    .MaximumLength(200).WithMessage("Material title must not exceed 200 characters.");

                file.RuleFor(f => f.FileStream)
                    .NotNull().WithMessage("File stream is required.");

                file.RuleFor(f => f.FileName)
                    .NotEmpty().WithMessage("File name is required.");
            });
        }
    }
}
