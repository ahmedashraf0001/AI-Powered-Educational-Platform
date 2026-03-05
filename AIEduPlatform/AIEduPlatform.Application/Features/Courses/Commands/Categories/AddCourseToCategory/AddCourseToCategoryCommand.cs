using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.AddCourseToCategory
{
    public record AddCourseToCategoryCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
        public Guid CategoryId { get; init; }
    }
}
