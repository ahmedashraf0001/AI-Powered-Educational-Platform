using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Dialogue.GenerateDialogueAudio
{
    /// <summary>
    /// Command to generate a teacher-student dialogue from course materials
    /// and synthesize it into audio.
    /// </summary>
    public record GenerateDialogueAudioCommand : IRequest<DialogueAudioResponseDto>
    {
        /// <summary>Study session providing course context for RAG retrieval.</summary>
        public Guid SessionId { get; init; }

        /// <summary>Optional topic to focus the dialogue on.</summary>
        public string? Topic { get; init; }

        /// <summary>Target audience: "beginner", "intermediate", "advanced".</summary>
        public string AudienceLevel { get; init; } = "intermediate";

        /// <summary>Number of teacher-student exchanges.</summary>
        public int NumberOfExchanges { get; init; } = 5;

        /// <summary>Dialogue length: "short", "medium", "long".</summary>
        public string DialogueLength { get; init; } = "medium";

        /// <summary>Whether to include worked examples.</summary>
        public bool IncludeExamples { get; init; } = true;

        /// <summary>Whether to include a summary at the end.</summary>
        public bool IncludeSummary { get; init; } = true;

        /// <summary>Teaching style: "socratic", "explanatory", "interactive".</summary>
        public string TeachingStyle { get; init; } = "interactive";

        /// <summary>Specific concepts the student should ask about.</summary>
        public List<string>? FocusConcepts { get; init; }

        /// <summary>Optional scoping to specific lectures.</summary>
        public List<Guid>? LectureIds { get; init; }

        /// <summary>Optional scoping to specific materials.</summary>
        public List<Guid>? MaterialIds { get; init; }
    }
}
