using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Flashcards.GetSessionFlashcards
{
    public class GetSessionFlashcardsQueryValidator : AbstractValidator<GetSessionFlashcardsQuery>
    {
        public GetSessionFlashcardsQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");
        }
    }
}
