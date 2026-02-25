using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.ReorderQuestions
{
    public class ReorderQuestionsCommandValidator : AbstractValidator<ReorderQuestionsCommand>
    {
        public ReorderQuestionsCommandValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");

            RuleFor(x => x.QuestionOrders)
                .NotEmpty().WithMessage("Question orders are required.")
                .Must(orders => orders.Values.Distinct().Count() == orders.Values.Count)
                .WithMessage("Order values must be unique.");
        }
    }
}
