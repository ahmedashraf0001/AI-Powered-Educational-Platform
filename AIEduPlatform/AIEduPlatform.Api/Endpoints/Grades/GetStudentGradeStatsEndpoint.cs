using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGradeStats;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetStudentGradeStatsRequest
{
    public Guid StudentId { get; set; }
    [QueryParam]
    public Guid? CourseId { get; set; }
}

public class GetStudentGradeStatsEndpoint : Endpoint<GetStudentGradeStatsRequest, ApiResponse<StudentGradeStats>>
{
    private readonly IMediator _mediator;

    public GetStudentGradeStatsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/grades/stats/student/{StudentId}");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Get student grade statistics";
            s.Description = "Returns grade statistics for a student, optionally filtered by course.";
            s.Response<ApiResponse<StudentGradeStats>>(200, "Student grade stats");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not authorized");
        });
    }

    public override async Task HandleAsync(GetStudentGradeStatsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentGradeStatsQuery
        {
            StudentId = req.StudentId,
            CourseId = req.CourseId
        }, ct);
        await SendOkAsync(ApiResponse<StudentGradeStats>.Ok(result), ct);
    }
}
