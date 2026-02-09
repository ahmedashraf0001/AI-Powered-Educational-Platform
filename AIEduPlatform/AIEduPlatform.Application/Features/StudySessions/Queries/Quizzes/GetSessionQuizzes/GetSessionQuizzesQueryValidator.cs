using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes
{
    public class GetSessionQuizzesQueryValidator : AbstractValidator<GetSessionQuizzesQuery>
    {
        public GetSessionQuizzesQueryValidator()
        {
            RuleFor(x => x.SessionId)
                .NotEmpty().WithMessage("Session ID is required.");
        }
    }
}
