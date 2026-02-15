using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetUpcomingExams;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetUpcomingExamsRequest
{
    public Guid CourseId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetUpcomingExamsEndpoint : Endpoint<GetUpcomingExamsRequest, ApiResponse<PagedResult<ExamDto>>>
{
    private readonly IMediator _mediator;

    public GetUpcomingExamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/upcoming/{CourseId}");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get upcoming exams";
            s.Description = "Returns exams scheduled in the future for a specific course.";
            s.Response<ApiResponse<PagedResult<ExamDto>>>(200, "Upcoming exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetUpcomingExamsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUpcomingExamsQuery
        {
            CourseId = req.CourseId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<ExamDto>>.Ok(result), ct);
    }
}
