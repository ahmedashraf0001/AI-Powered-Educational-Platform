using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses
{
    public class GetEnrolledCoursesQueryValidator : AbstractValidator<GetEnrolledCoursesQuery>
    {
        public GetEnrolledCoursesQueryValidator()
        {
            // No validation needed - user ID comes from ICurrentUserService
        }
    }
}
