using AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class SubmitExamRequest
{
    public Guid ExamId { get; set; }
    [FromBody]
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
            s.ExampleRequest = new SubmitExamRequest
            {
                ExamId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Answers = new Dictionary<Guid, string>
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), "ReLU" },
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), "True" },
                    { Guid.Parse("33333333-3333-3333-3333-333333333333"), "Backpropagation is the algorithm used to compute gradients for updating weights in neural networks." }
                }
            };
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
