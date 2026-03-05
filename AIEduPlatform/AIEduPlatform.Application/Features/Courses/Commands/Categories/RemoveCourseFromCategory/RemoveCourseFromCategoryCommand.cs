using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.RemoveCourseFromCategory
{
    public record RemoveCourseFromCategoryCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
        public Guid CategoryId { get; init; }
    }
}
