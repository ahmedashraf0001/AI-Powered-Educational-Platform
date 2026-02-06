using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGrades;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetStudentGradesEndpoint : EndpointWithoutRequest<List<GradeDto>>
{
    private readonly IMediator _mediator;

    public GetStudentGradesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/grades/student");
        Group<GradesGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentGradesQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
