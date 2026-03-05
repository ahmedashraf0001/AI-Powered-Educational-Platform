using AIEduPlatform.Core.DTOs.Enrollments;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent
{
    public record UnenrollStudentCommand : IRequest<UnenrollmentResultDto>
    {
        public Guid CourseId { get; init; }
    }
}
