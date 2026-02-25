using AIEduPlatform.Application.Features.Exams.Queries.Questions.GetExamQuestions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class GetExamQuestionsRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamQuestionsEndpoint : Endpoint<GetExamQuestionsRequest, ApiResponse<List<QuestionDto>>>
{
    private readonly IMediator _mediator;

    public GetExamQuestionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{ExamId}/questions");
        Group<QuestionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get exam questions";
            s.Description = "Returns all questions for an exam. Students see questions during the exam; teachers see them for management.";
            s.Response<ApiResponse<List<QuestionDto>>>(200, "Exam questions");
            s.Response(401, "Not authenticated");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(GetExamQuestionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamQuestionsQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(ApiResponse<List<QuestionDto>>.Ok(result), ct);
    }
}
