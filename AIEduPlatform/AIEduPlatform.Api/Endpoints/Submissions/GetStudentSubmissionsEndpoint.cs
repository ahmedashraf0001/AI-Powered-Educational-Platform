using AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class GetStudentSubmissionsEndpoint : EndpointWithoutRequest<List<SubmissionDto>>
{
    private readonly IMediator _mediator;

    public GetStudentSubmissionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/submissions/student");
        Group<SubmissionsGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentSubmissionsQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
