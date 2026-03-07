using AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmission;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GradeSubmissionRequest
{
    public Guid SubmissionId { get; set; }
    public float Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

public class GradeSubmissionResponse
{
    public Guid GradeId { get; set; }
}

public class GradeSubmissionEndpoint : Endpoint<GradeSubmissionRequest, ApiResponse<GradeSubmissionResponse>>
{
    private readonly IMediator _mediator;

    public GradeSubmissionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/submissions/{SubmissionId}/grade");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Grade a submission manually";
            s.Description = "Assigns a manual grade (score + feedback) to a student's exam submission.";
            s.ExampleRequest = new GradeSubmissionRequest
            {
                SubmissionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Score = 85.5f,
                Feedback = "Good understanding of core concepts. Review section 3 on regularization techniques."
            };
            s.Response<ApiResponse<GradeSubmissionResponse>>(201, "Grade created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(GradeSubmissionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GradeSubmissionCommand
        {
            SubmissionId = req.SubmissionId,
            Score = req.Score,
            Feedback = req.Feedback
        }, ct);

        await SendCreatedAtAsync<GetGradeBySubmissionEndpoint>(
            new { submissionId = req.SubmissionId },
            ApiResponse<GradeSubmissionResponse>.Ok(new GradeSubmissionResponse { GradeId = result }, "Grade created successfully."),
            cancellation: ct);
    }
}
