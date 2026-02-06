using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamById
{
    public class GetExamByIdQueryValidator : AbstractValidator<GetExamByIdQuery>
    {
        public GetExamByIdQueryValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");
        }
    }
}
