using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments
{
    public class GetCourseEnrollmentsQueryValidator : AbstractValidator<GetCourseEnrollmentsQuery>
    {
        public GetCourseEnrollmentsQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course ID is required.");
        }
    }
}
