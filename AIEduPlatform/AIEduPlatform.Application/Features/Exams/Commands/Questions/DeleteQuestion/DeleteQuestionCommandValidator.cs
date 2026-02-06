using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.DeleteQuestion
{
    public class DeleteQuestionCommandValidator : AbstractValidator<DeleteQuestionCommand>
    {
        public DeleteQuestionCommandValidator()
        {
            RuleFor(x => x.QuestionId)
                .NotEmpty().WithMessage("Question ID is required.");
        }
    }
}
