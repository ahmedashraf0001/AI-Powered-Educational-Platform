using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses
{
    public class GetEnrolledCoursesQueryValidator : AbstractValidator<GetEnrolledCoursesQuery>
    {
        public GetEnrolledCoursesQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
