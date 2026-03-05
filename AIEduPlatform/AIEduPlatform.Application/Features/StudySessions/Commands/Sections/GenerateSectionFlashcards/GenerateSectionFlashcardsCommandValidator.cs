using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionFlashcards
{
    public class GenerateSectionFlashcardsCommandValidator : AbstractValidator<GenerateSectionFlashcardsCommand>
    {
        public GenerateSectionFlashcardsCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.SectionId)
                .NotEmpty().WithMessage("Section ID is required.");

            RuleFor(x => x.NumberOfCards)
                .InclusiveBetween(1, 30).WithMessage("Number of cards must be between 1 and 30.");
        }
    }
}
