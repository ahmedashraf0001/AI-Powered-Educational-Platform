using FluentValidation;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamsByCourse
{
    public class GetExamsByCourseQueryValidator : AbstractValidator<GetExamsByCourseQuery>
    {
        public GetExamsByCourseQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
