using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetActiveExams
{
    public class GetActiveExamsQueryValidator : AbstractValidator<GetActiveExamsQuery>
    {
        public GetActiveExamsQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
