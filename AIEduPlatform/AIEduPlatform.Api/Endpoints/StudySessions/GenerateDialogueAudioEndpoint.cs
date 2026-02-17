using AIEduPlatform.Application.Features.StudySessions.Commands.Dialogue.GenerateDialogueAudio;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GenerateDialogueAudioRequest
{
    public Guid SessionId { get; set; }
    public string? Topic { get; set; }
    public string AudienceLevel { get; set; } = "intermediate";
    public int NumberOfExchanges { get; set; } = 5;
    public string DialogueLength { get; set; } = "medium";
    public bool IncludeExamples { get; set; } = true;
    public bool IncludeSummary { get; set; } = true;
    public string TeachingStyle { get; set; } = "interactive";
    public List<string>? FocusConcepts { get; set; }
    public Guid? LectureId { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateDialogueAudioEndpoint
    : Endpoint<GenerateDialogueAudioRequest, ApiResponse<DialogueAudioResponseDto>>
{
    private readonly IMediator _mediator;

    public GenerateDialogueAudioEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/dialogue-audio");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Generate dialogue audio from course materials";
            s.Description =
                "Generates a teacher-student dialogue using AI (Ollama) based on course materials " +
                "retrieved via RAG, then synthesises the dialogue into audio. " +
                "Returns both the dialogue text and a base64-encoded audio file with per-turn timestamps " +
                "for synchronized playback. Optionally scope by lecture or specific materials.";
            s.Response<ApiResponse<DialogueAudioResponseDto>>(201, "Dialogue and audio generated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
            s.Response(404, "Session not found");
        });
    }

    public override async Task HandleAsync(GenerateDialogueAudioRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateDialogueAudioCommand
        {
            SessionId = req.SessionId,
            Topic = req.Topic,
            AudienceLevel = req.AudienceLevel,
            NumberOfExchanges = req.NumberOfExchanges,
            DialogueLength = req.DialogueLength,
            IncludeExamples = req.IncludeExamples,
            IncludeSummary = req.IncludeSummary,
            TeachingStyle = req.TeachingStyle,
            FocusConcepts = req.FocusConcepts,
            LectureId = req.LectureId,
            MaterialIds = req.MaterialIds
        }, ct);

        await SendAsync(ApiResponse<DialogueAudioResponseDto>.Ok(result), 201, ct);
    }
}
