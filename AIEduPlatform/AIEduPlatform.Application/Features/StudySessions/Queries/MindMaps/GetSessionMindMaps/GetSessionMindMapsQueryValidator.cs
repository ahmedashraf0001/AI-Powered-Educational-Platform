using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps
{
    public class GetSessionMindMapsQueryValidator : AbstractValidator<GetSessionMindMapsQuery>
    {
        public GetSessionMindMapsQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");
        }
    }
}
