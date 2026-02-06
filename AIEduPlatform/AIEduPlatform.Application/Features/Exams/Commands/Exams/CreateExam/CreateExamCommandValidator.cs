using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.CreateExam
{
    public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
    {
        public CreateExamCommandValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Exam title is required.")
                .MaximumLength(300).WithMessage("Exam title must not exceed 300 characters.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");

            RuleFor(x => x.EndTime)
                .NotEmpty().WithMessage("End time is required.")
                .GreaterThan(x => x.StartTime).WithMessage("End time must be after start time.");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duration must be greater than 0 minutes.")
                .LessThanOrEqualTo(480).WithMessage("Duration must not exceed 480 minutes.");
        }
    }
}
