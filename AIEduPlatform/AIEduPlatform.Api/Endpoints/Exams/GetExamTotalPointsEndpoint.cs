using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamTotalPoints;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetExamTotalPointsRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamTotalPointsEndpoint : Endpoint<GetExamTotalPointsRequest, ApiResponse<int>>
{
    private readonly IMediator _mediator;

    public GetExamTotalPointsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{ExamId}/total-points");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get exam total points";
            s.Description = "Returns the total points possible for a specific exam (sum of all question points).";
            s.Response<ApiResponse<int>>(200, "Total points");
            s.Response(401, "Not authenticated");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(GetExamTotalPointsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamTotalPointsQuery
        {
            ExamId = req.ExamId
        }, ct);
        await SendOkAsync(ApiResponse<int>.Ok(result), ct);
    }
}
