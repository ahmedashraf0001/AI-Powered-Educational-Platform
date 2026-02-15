using AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmissionWithAI;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GradeSubmissionWithAIRequest
{
    public Guid SubmissionId { get; set; }
}

public class GradeSubmissionWithAIEndpoint : Endpoint<GradeSubmissionWithAIRequest, ApiResponse<GradeSubmissionWithAIResult>>
{
    private readonly IMediator _mediator;

    public GradeSubmissionWithAIEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/submissions/{SubmissionId}/grade-ai");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Grade a submission with AI";
            s.Description = "Uses AI to automatically grade an exam submission. The grade is marked as AI-graded and requires teacher approval.";
            s.Response<ApiResponse<GradeSubmissionWithAIResult>>(200, "AI grading result");
            s.Response(400, "AI grading failed");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(GradeSubmissionWithAIRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GradeSubmissionWithAICommand
        {
            SubmissionId = req.SubmissionId
        }, ct);

        if (!result.Success)
        {
            await SendAsync(ApiResponse<GradeSubmissionWithAIResult>.Fail(result.Error ?? "AI grading failed."), 400, ct);
            return;
        }

        await SendOkAsync(ApiResponse<GradeSubmissionWithAIResult>.Ok(result), ct);
    }
}
