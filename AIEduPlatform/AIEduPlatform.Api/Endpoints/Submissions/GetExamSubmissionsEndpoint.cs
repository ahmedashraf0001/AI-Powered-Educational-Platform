using AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissions;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class GetExamSubmissionsRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamSubmissionsEndpoint : Endpoint<GetExamSubmissionsRequest, List<SubmissionDto>>
{
    private readonly IMediator _mediator;

    public GetExamSubmissionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{examId}/submissions");
        Group<SubmissionsGroup>();
    }

    public override async Task HandleAsync(GetExamSubmissionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamSubmissionsQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(result, ct);
    }
}
