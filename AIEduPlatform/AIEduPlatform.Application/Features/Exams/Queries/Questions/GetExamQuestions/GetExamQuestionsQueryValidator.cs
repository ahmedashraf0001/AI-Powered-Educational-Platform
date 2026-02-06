using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Questions.GetExamQuestions
{
    public class GetExamQuestionsQueryValidator : AbstractValidator<GetExamQuestionsQuery>
    {
        public GetExamQuestionsQueryValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");
        }
    }
}
