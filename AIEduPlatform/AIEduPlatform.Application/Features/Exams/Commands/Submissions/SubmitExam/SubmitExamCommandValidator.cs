using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam
{
    public class SubmitExamCommandValidator : AbstractValidator<SubmitExamCommand>
    {
        public SubmitExamCommandValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");

            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("Answers are required.");
        }
    }
}
