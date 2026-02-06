using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamById;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetExamByIdRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamByIdEndpoint : Endpoint<GetExamByIdRequest, ExamDetailDto>
{
    private readonly IMediator _mediator;

    public GetExamByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{examId}");
        Group<ExamsGroup>();
    }

    public override async Task HandleAsync(GetExamByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamByIdQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(result, ct);
    }
}
