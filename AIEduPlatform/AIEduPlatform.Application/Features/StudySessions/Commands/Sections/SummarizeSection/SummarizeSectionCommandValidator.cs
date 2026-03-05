using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.SummarizeSection
{
    public class SummarizeSectionCommandValidator : AbstractValidator<SummarizeSectionCommand>
    {
        public SummarizeSectionCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.SectionId)
                .NotEmpty().WithMessage("Section ID is required.");

            RuleFor(x => x.SummaryLength)
                .InclusiveBetween(100, 2000).WithMessage("Summary length must be between 100 and 2000 words.");
        }
    }
}
