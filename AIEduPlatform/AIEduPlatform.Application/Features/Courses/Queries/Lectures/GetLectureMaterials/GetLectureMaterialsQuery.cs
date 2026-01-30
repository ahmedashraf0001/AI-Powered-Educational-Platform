using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureMaterials
{
    public record GetLectureMaterialsQuery : IRequest<List<MaterialDto>>
    {
        public Guid LectureId { get; init; }
    }
}
