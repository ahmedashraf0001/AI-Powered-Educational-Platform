using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetCourseLectures
{
    public record GetCourseLecturesQuery : IRequest<List<LectureDto>>
    {
        public Guid CourseId { get; init; }
        public bool IncludeMaterials { get; init; } = true;
    }
}
