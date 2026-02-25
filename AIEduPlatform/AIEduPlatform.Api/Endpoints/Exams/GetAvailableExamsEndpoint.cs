using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetAvailableExamsForStudent;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetAvailableExamsRequest
{
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetAvailableExamsEndpoint : Endpoint<GetAvailableExamsRequest, ApiResponse<PagedResult<ExamDto>>>
{
    private readonly IMediator _mediator;

    public GetAvailableExamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/available");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get my available exams";
            s.Description = "Returns exams available to the authenticated student based on their enrolled courses.";
            s.Response<ApiResponse<PagedResult<ExamDto>>>(200, "Available exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetAvailableExamsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAvailableExamsForStudentQuery
        {
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<ExamDto>>.Ok(result), ct);
    }
}
