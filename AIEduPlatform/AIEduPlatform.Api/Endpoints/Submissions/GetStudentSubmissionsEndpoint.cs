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
        Summary(s =>
        {
            s.Summary = "Get my submissions";
            s.Description = "Returns all exam submissions made by the authenticated student.";
            s.Response<List<SubmissionDto>>(200, "Student submissions");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentSubmissionsQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
