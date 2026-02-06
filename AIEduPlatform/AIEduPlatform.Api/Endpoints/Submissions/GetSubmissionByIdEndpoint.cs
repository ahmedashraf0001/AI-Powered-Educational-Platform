using AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetSubmissionById;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class GetSubmissionByIdRequest
{
    public Guid SubmissionId { get; set; }
}

public class GetSubmissionByIdEndpoint : Endpoint<GetSubmissionByIdRequest, SubmissionDetailDto>
{
    private readonly IMediator _mediator;

    public GetSubmissionByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/submissions/{submissionId}");
        Group<SubmissionsGroup>();
    }

    public override async Task HandleAsync(GetSubmissionByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubmissionByIdQuery { SubmissionId = req.SubmissionId }, ct);
        await SendOkAsync(result, ct);
    }
}
