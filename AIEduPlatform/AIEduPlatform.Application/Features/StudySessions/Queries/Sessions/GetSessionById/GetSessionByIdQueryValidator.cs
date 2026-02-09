using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetSessionById
{
    public class GetSessionByIdQueryValidator : AbstractValidator<GetSessionByIdQuery>
    {
        public GetSessionByIdQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");
        }
    }
}
