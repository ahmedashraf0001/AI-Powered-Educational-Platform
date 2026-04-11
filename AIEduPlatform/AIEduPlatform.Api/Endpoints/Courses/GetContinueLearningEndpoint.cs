using AIEduPlatform.Application.Features.Courses.Queries.Progress.GetContinueLearning;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Progress;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetContinueLearningEndpoint : EndpointWithoutRequest<ApiResponse<List<ContinueLearningDto>>>
{
    private readonly IMediator _mediator;

    public GetContinueLearningEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/continue-learning");
        Roles("Student", "Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Continue learning";
            s.Description = "Returns in-progress courses with resume position for the authenticated student.";
            s.Response<ApiResponse<List<ContinueLearningDto>>>(200, "Continue learning data");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContinueLearningQuery(), ct);
        await SendOkAsync(ApiResponse<List<ContinueLearningDto>>.Ok(result), ct);
    }
}
