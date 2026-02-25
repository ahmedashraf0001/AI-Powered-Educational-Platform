using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.DeleteExam
{
    public class DeleteExamCommandValidator : AbstractValidator<DeleteExamCommand>
    {
        public DeleteExamCommandValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");
        }
    }
}
