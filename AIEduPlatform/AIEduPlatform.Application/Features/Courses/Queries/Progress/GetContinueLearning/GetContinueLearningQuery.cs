using AIEduPlatform.Core.DTOs.Progress;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Progress.GetContinueLearning
{
    public record GetContinueLearningQuery : IRequest<List<ContinueLearningDto>>;
}
