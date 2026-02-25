using FluentValidation;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.StartSession
{
    public class StartSessionCommandValidator : AbstractValidator<StartSessionCommand>
    {
        public StartSessionCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
