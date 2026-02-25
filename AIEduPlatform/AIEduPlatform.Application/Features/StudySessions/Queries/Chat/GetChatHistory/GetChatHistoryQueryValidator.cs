using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Chat.GetChatHistory
{
    public class GetChatHistoryQueryValidator : AbstractValidator<GetChatHistoryQuery>
    {
        public GetChatHistoryQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");
        }
    }
}
