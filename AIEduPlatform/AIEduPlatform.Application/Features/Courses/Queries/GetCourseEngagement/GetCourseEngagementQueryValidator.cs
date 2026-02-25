using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.GetCourseEngagement
{
    public class GetCourseEngagementQueryValidator : AbstractValidator<GetCourseEngagementQuery>
    {
        public GetCourseEngagementQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty()
                .WithMessage("CourseId is required.");
        }
    }
}
