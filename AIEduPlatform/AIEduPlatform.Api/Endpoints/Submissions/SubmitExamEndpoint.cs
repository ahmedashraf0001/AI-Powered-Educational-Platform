using AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class SubmitExamRequest
{
    public Guid ExamId { get; set; }
    public Dictionary<Guid, string> Answers { get; set; } = [];
}

public class SubmitExamResponse
{
    public Guid SubmissionId { get; set; }
}

public class SubmitExamEndpoint : Endpoint<SubmitExamRequest, ApiResponse<SubmitExamResponse>>
{
    private readonly IMediator _mediator;

    public SubmitExamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{ExamId}/submit");
        Group<SubmissionsGroup>();
        Summary(s =>
        {
            s.Summary = "Submit exam answers";
            s.Description = "Submits the student's answers for an exam. Answers are a map of questionId to answer text.";
            s.Response<ApiResponse<SubmitExamResponse>>(201, "Submission created");
            s.Response(400, "Exam not active or already submitted");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(SubmitExamRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitExamCommand
        {
            ExamId = req.ExamId,
            Answers = req.Answers
        }, ct);

        await SendCreatedAtAsync<GetSubmissionByIdEndpoint>(
            new { submissionId = result },
            ApiResponse<SubmitExamResponse>.Ok(new SubmitExamResponse { SubmissionId = result }, "Exam submitted successfully."),
            cancellation: ct);
    }
}
