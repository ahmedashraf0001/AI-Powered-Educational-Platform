using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamsByCourse;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetExamsByCourseRequest
{
    public Guid CourseId { get; set; }
}

public class GetExamsByCourseEndpoint : Endpoint<GetExamsByCourseRequest, List<ExamDto>>
{
    private readonly IMediator _mediator;

    public GetExamsByCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/course/{CourseId}");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get exams by course";
            s.Description = "Returns all exams for a specific course.";
            s.Response<List<ExamDto>>(200, "Course exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetExamsByCourseRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamsByCourseQuery { CourseId = req.CourseId }, ct);
        await SendOkAsync(result, ct);
    }
}
