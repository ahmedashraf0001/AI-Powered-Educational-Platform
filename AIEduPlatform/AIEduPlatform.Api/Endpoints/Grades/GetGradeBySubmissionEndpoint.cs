using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeBySubmission;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetGradeBySubmissionRequest
{
    public Guid SubmissionId { get; set; }
}

public class GetGradeBySubmissionEndpoint : Endpoint<GetGradeBySubmissionRequest, GradeDto>
{
    private readonly IMediator _mediator;

    public GetGradeBySubmissionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/submissions/{SubmissionId}/grade");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Get grade for a submission";
            s.Description = "Returns the grade details for a specific exam submission.";
            s.Response<GradeDto>(200, "Grade details");
            s.Response(401, "Not authenticated");
            s.Response(404, "Grade not found");
        });
    }

    public override async Task HandleAsync(GetGradeBySubmissionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGradeBySubmissionQuery { SubmissionId = req.SubmissionId }, ct);
        await SendOkAsync(result, ct);
    }
}
