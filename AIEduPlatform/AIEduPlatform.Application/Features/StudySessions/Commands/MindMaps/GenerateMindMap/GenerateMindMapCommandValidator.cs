using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.MindMaps.GenerateMindMap
{
    public class GenerateMindMapCommandValidator : AbstractValidator<GenerateMindMapCommand>
    {
        public GenerateMindMapCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.CentralTopic)
                .NotEmpty().WithMessage("Central topic is required.")
                .MaximumLength(500).WithMessage("Central topic must not exceed 500 characters.");

            RuleFor(x => x.MaxDepth)
                .InclusiveBetween(1, 5).WithMessage("Max depth must be between 1 and 5.");
        }
    }
}
