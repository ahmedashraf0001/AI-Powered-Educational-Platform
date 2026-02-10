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
        Summary(s =>
        {
            s.Summary = "Get my grades";
            s.Description = "Returns all grades for the authenticated student across all exams.";
            s.Response<List<GradeDto>>(200, "Student grades");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentGradesQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
