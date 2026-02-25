using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Commands.SendEngagementAlerts
{
    public class SendEngagementAlertsCommandValidator
        : AbstractValidator<SendEngagementAlertsCommand>
    {
        public SendEngagementAlertsCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty()
                .WithMessage("CourseId is required.");

            RuleFor(x => x.CustomMessage)
                .MaximumLength(500)
                .When(x => x.CustomMessage != null)
                .WithMessage("Custom message must not exceed 500 characters.");
        }
    }
}
