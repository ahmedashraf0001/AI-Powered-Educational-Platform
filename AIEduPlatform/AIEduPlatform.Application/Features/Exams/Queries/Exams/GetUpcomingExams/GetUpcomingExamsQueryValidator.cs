using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetUpcomingExams
{
    public class GetUpcomingExamsQueryValidator : AbstractValidator<GetUpcomingExamsQuery>
    {
        public GetUpcomingExamsQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
