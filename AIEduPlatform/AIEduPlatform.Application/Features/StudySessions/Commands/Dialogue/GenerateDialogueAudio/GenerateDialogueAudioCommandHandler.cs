using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Dialogue.GenerateDialogueAudio
{
    public class GenerateDialogueAudioCommandHandler
        : IRequestHandler<GenerateDialogueAudioCommand, DialogueAudioResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IRAGService _ragService;
        private readonly ILogger<GenerateDialogueAudioCommandHandler> _logger;

        public GenerateDialogueAudioCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            ITranscriptionService transcriptionService,
            IRAGService ragService,
            ILogger<GenerateDialogueAudioCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _transcriptionService = transcriptionService;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<DialogueAudioResponseDto> Handle(
            GenerateDialogueAudioCommand request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            // ── Auth ────────────────────────────────────────────
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions
                .GetByIdAsync(request.SessionId, cancellationToken);

            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);

            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only generate dialogue audio in your own study sessions.");

            // ── RAG retrieval ───────────────────────────────────
            var query = request.Topic ?? "teaching dialogue overview";

            var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
            {
                Query = query,
                CourseId = session.CourseId,
                LectureIds = request.LectureIds,
                MaterialIds = request.MaterialIds
            }, cancellationToken);

            _logger.LogInformation(
                "RAG retrieval completed for dialogue generation. SessionId={SessionId}, Chunks={ChunkCount}",
                request.SessionId, ragResponse.Chunks.Count);

            // ── Step 1: Generate dialogue text via Ollama ───────
            var dialogue = await _ollamaClient.GenerateTeacherStudentDialogueAsync(
                ragResponse.Chunks,
                topic: request.Topic,
                audienceLevel: request.AudienceLevel,
                numberOfExchanges: request.NumberOfExchanges,
                dialogueLength: request.DialogueLength,
                includeExamples: request.IncludeExamples,
                includeSummary: request.IncludeSummary,
                teachingStyle: request.TeachingStyle,
                focusConcepts: request.FocusConcepts,
                ct: cancellationToken);

            _logger.LogInformation(
                "Dialogue generated. SessionId={SessionId}, Topic={Topic}, Turns={TurnCount}",
                request.SessionId, dialogue.Topic, dialogue.Turns.Count);

            // ── Step 2: Get default voice config ────────────────
            DefaultVoiceConfigResult? voiceConfig = null;
            try
            {
                voiceConfig = await _transcriptionService.GetDefaultVoiceConfigAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch default voice config; using service defaults. SessionId={SessionId}",
                    request.SessionId);
            }

            // ── Step 3: Synthesize dialogue to audio ────────────
            var audioResult = await _transcriptionService.GenerateDialogueAudioAsync(
                dialogue,
                voiceConfig,
                cancellationToken);

            if (!audioResult.Success)
            {
                _logger.LogError(
                    "Audio synthesis failed. SessionId={SessionId}, Error={Error}",
                    request.SessionId, audioResult.ErrorMessage);

                throw new ApplicationException(
                    $"Audio synthesis failed: {audioResult.ErrorMessage}");
            }

            sw.Stop();

            // ── Update session activity ─────────────────────────
            await _unitOfWork.StudySessions.UpdateLastActivityAsync(
                request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Dialogue audio generated. SessionId={SessionId}, Duration={DurationSec:F1}s, " +
                "Size={SizeKB:F0}KB, TotalMs={TotalMs}",
                request.SessionId, audioResult.DurationSeconds,
                audioResult.FileSizeBytes / 1024.0, sw.ElapsedMilliseconds);

            return new DialogueAudioResponseDto
            {
                Dialogue = dialogue,
                AudioBase64 = audioResult.AudioBase64,
                Format = audioResult.Format,
                DurationSeconds = audioResult.DurationSeconds,
                FileSizeBytes = audioResult.FileSizeBytes,
                ProcessingTimeMs = sw.ElapsedMilliseconds,
                TurnTimestamps = audioResult.TurnTimestamps
            };
        }
    }
}
