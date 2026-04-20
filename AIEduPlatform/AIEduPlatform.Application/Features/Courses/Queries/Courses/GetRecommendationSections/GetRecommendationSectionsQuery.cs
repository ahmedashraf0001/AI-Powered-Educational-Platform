using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetRecommendationSections
{
    public record GetRecommendationSectionsQuery : IRequest<RecommendationSectionsDto>
    {
        public int Top { get; init; } = 10;
    }
}