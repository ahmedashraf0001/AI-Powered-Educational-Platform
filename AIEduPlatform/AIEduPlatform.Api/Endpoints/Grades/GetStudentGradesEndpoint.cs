using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGrades;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetStudentGradesRequest
{
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetStudentGradesEndpoint : Endpoint<GetStudentGradesRequest, ApiResponse<PagedResult<GradeDto>>>
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
            s.Response<ApiResponse<PagedResult<GradeDto>>>(200, "Student grades");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetStudentGradesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentGradesQuery
        {
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 10
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<GradeDto>>.Ok(result), ct);
    }
}
