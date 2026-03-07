using AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionQuiz;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Sections;

public class GenerateSectionQuizRequest
{
    public Guid SessionId { get; set; }
    public Guid SectionId { get; set; }
    public int NumberOfQuestions { get; set; } = 5;
    public string Difficulty { get; set; } = "medium";
    public List<string> QuestionTypes { get; set; } = new() { "mcq" };
}

public class GenerateSectionQuizEndpoint : Endpoint<GenerateSectionQuizRequest, ApiResponse<GeneratedQuizDto>>
{
    private readonly IMediator _mediator;

    public GenerateSectionQuizEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/sessions/{SessionId}/sections/{SectionId}/quiz");
        Group<SectionsGroup>();
        Roles("Student");
        Summary(s =>
        {
            s.Summary = "Generate a quiz from a semantic section";
            s.Description = "Generates AI practice quiz questions scoped to a specific semantic section.";
            s.ExampleRequest = new GenerateSectionQuizRequest
            {
                SessionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                SectionId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                NumberOfQuestions = 5,
                Difficulty = "medium",
                QuestionTypes = new List<string> { "mcq", "true_false" }
            };
            s.Response<ApiResponse<GeneratedQuizDto>>(200, "Section quiz generated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
            s.Response(404, "Session or section not found");
        });
    }

    public override async Task HandleAsync(GenerateSectionQuizRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateSectionQuizCommand
        {
            SessionId = req.SessionId,
            SectionId = req.SectionId,
            NumberOfQuestions = req.NumberOfQuestions,
            Difficulty = req.Difficulty,
            QuestionTypes = req.QuestionTypes
        }, ct);

        await SendOkAsync(ApiResponse<GeneratedQuizDto>.Ok(result, "Section quiz generated"), ct);
    }
}
