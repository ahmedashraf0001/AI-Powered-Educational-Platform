using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissions
{
    public class GetExamSubmissionsQueryValidator : AbstractValidator<GetExamSubmissionsQuery>
    {
        public GetExamSubmissionsQueryValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");
        }
    }
}
