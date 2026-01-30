using FluentValidation;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor
{
    public class GetCoursesByInstructorQueryValidator : AbstractValidator<GetCoursesByInstructorQuery>
    {
        public GetCoursesByInstructorQueryValidator()
        {
            // InstructorId is optional - if not provided, uses current user from ICurrentUserService
        }
    }
}
