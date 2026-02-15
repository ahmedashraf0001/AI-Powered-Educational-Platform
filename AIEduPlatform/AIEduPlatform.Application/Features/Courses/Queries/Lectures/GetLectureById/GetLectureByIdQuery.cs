using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureById
{
    public record GetLectureByIdQuery : IRequest<LectureDetailDto>
    {
        public Guid LectureId { get; init; }
    }
}
