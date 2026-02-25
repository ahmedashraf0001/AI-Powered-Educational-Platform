using AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class GetStudentSubmissionsRequest
{
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetStudentSubmissionsEndpoint : Endpoint<GetStudentSubmissionsRequest, ApiResponse<PagedResult<SubmissionDto>>>
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
            s.Response<ApiResponse<PagedResult<SubmissionDto>>>(200, "Student submissions");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetStudentSubmissionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentSubmissionsQuery
        {
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<SubmissionDto>>.Ok(result), ct);
    }
}
