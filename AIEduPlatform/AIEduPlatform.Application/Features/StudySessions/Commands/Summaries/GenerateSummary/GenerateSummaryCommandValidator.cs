using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Summaries.GenerateSummary
{
    public class GenerateSummaryCommandValidator : AbstractValidator<GenerateSummaryCommand>
    {
        public GenerateSummaryCommandValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");

            RuleFor(x => x.Topic)
                .MaximumLength(500).WithMessage("Topic must not exceed 500 characters.");

            RuleFor(x => x.SummaryLength)
                .InclusiveBetween(100, 2000).WithMessage("Summary length must be between 100 and 2000 words.");
        }
    }
}
