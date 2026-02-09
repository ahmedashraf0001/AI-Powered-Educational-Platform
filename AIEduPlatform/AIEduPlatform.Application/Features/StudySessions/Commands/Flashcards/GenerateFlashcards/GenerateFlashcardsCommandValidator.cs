using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Flashcards.GenerateFlashcards
{
    public class GenerateFlashcardsCommandValidator : AbstractValidator<GenerateFlashcardsCommand>
    {
        public GenerateFlashcardsCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.Topic)
                .NotEmpty().WithMessage("Topic is required.")
                .MaximumLength(500).WithMessage("Topic must not exceed 500 characters.");

            RuleFor(x => x.NumberOfCards)
                .InclusiveBetween(1, 30).WithMessage("Number of cards must be between 1 and 30.");
        }
    }
}
