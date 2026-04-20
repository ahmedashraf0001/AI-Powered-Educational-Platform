using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionQuiz
{
    public class GenerateSectionQuizCommandHandler : IRequestHandler<GenerateSectionQuizCommand, GeneratedQuizDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<GenerateSectionQuizCommandHandler> _logger;

        public GenerateSectionQuizCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<GenerateSectionQuizCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<GeneratedQuizDto> Handle(GenerateSectionQuizCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only use your own study sessions.");

            var section = await _unitOfWork.SemanticSections.GetByIdAsync(request.SectionId, cancellationToken);
            if (section is null)
                throw new NotFoundException(nameof(SemanticSection), request.SectionId);

            var ragResponse = await _ragService.RetrieveAllSegmentChunksAsync(request.SectionId, cancellationToken);
            var scopedChunks = ragResponse?.Chunks ?? new List<ContextChunk>();

            if (!scopedChunks.Any())
            {
                _logger.LogWarning("No chunks found for section {SectionId}.", request.SectionId);
                throw new BadRequestException("No indexed content found for this section.");
            }

            var questionTypes = request.QuestionTypes
                .Select(qt => qt.ToLowerInvariant() switch
                {
                    "mcq" => QuestionType.MultipleChoice,
                    "true_false" => QuestionType.TrueFalse,
                    "short_answer" => QuestionType.ShortAnswer,
                    "essay" => QuestionType.Essay,
                    _ => QuestionType.MultipleChoice
                })
                .ToList();

            var aiQuestions = await _ollamaClient.GenerateQuizAsync(
                scopedChunks,
                section.Title,
                request.NumberOfQuestions,
                request.Difficulty,
                questionTypes,
                cancellationToken);

            var difficulty = Enum.TryParse<QuizDifficulty>(request.Difficulty, ignoreCase: true, out var d)
                ? d
                : QuizDifficulty.Medium;

            var now = DateTime.UtcNow;
            var quiz = new GeneratedQuiz
            {
                SessionId = request.SessionId,
                Topic = $"Section: {section.Title}",
                Difficulty = difficulty,
                Questions = JsonSerializer.Serialize(aiQuestions),
                StudentAnswers = null!,
                Score = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _unitOfWork.GeneratedQuizzes.AddAsync(quiz, cancellationToken);
            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated section quiz for session {SessionId}, section {SectionId}: {Title} ({ChunkCount} chunks)",
                request.SessionId, request.SectionId, section.Title, scopedChunks.Count);

            return new GeneratedQuizDto
            {
                Id = created.Id,
                Topic = created.Topic,
                Difficulty = created.Difficulty,
                Questions = created.Questions,
                StudentAnswers = created.StudentAnswers,
                Score = created.Score,
                CreatedAt = created.CreatedAt
            };
        }
    }
}
