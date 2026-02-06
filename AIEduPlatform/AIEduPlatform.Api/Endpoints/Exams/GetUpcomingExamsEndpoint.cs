using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetUpcomingExams;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetUpcomingExamsRequest
{
    public Guid CourseId { get; set; }
}

public class GetUpcomingExamsEndpoint : Endpoint<GetUpcomingExamsRequest, List<ExamDto>>
{
    private readonly IMediator _mediator;

    public GetUpcomingExamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/upcoming/{courseId}");
        Group<ExamsGroup>();
    }

    public override async Task HandleAsync(GetUpcomingExamsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUpcomingExamsQuery { CourseId = req.CourseId }, ct);
        await SendOkAsync(result, ct);
    }
}
