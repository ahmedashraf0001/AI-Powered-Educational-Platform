using AIEduPlatform.Application.Features.Exams.Queries.Questions.GetExamQuestions;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class GetExamQuestionsRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamQuestionsEndpoint : Endpoint<GetExamQuestionsRequest, List<QuestionDto>>
{
    private readonly IMediator _mediator;

    public GetExamQuestionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{examId}/questions");
        Group<QuestionsGroup>();
    }

    public override async Task HandleAsync(GetExamQuestionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamQuestionsQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(result, ct);
    }
}
