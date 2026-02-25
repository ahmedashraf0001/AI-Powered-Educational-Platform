using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamById;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetExamByIdRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamByIdEndpoint : Endpoint<GetExamByIdRequest, ApiResponse<ExamDetailDto>>
{
    private readonly IMediator _mediator;

    public GetExamByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{ExamId}");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get exam details";
            s.Description = "Returns full exam details including questions and course info.";
            s.Response<ApiResponse<ExamDetailDto>>(200, "Exam details");
            s.Response(401, "Not authenticated");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(GetExamByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamByIdQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(ApiResponse<ExamDetailDto>.Ok(result), ct);
    }
}
