using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGrades
{
    public class GetExamGradesQueryValidator : AbstractValidator<GetExamGradesQuery>
    {
        public GetExamGradesQueryValidator()
        {
            RuleFor(x => x.ExamId)
                .NotEmpty().WithMessage("Exam ID is required.");
        }
    }
}
