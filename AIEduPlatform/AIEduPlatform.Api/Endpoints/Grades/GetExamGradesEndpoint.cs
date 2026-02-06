using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGrades;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetExamGradesRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamGradesEndpoint : Endpoint<GetExamGradesRequest, List<GradeDto>>
{
    private readonly IMediator _mediator;

    public GetExamGradesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{examId}/grades");
        Group<GradesGroup>();
    }

    public override async Task HandleAsync(GetExamGradesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamGradesQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(result, ct);
    }
}
