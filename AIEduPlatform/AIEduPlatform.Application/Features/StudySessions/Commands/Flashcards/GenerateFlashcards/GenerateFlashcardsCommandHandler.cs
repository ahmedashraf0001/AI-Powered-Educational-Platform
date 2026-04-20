using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Flashcards.GenerateFlashcards
{
    public class GenerateFlashcardsCommandHandler : IRequestHandler<GenerateFlashcardsCommand, List<FlashcardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<GenerateFlashcardsCommandHandler> _logger;

        public GenerateFlashcardsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<GenerateFlashcardsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<List<FlashcardDto>> Handle(GenerateFlashcardsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only generate flashcards in your own study sessions.");

            var chunks = new List<ContextChunk>();
            if (string.IsNullOrWhiteSpace(request.Topic) && request.MaterialIds != null && request.MaterialIds.Any())
            {
                foreach (var materialId in request.MaterialIds)
                {
                    var response = await _ragService.RetrieveAllMaterialChunksAsync(materialId, cancellationToken);
                    if (response?.Chunks != null)
                        chunks.AddRange(response.Chunks);
                }
            }
            else
            {
                var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
                {
                    Query = request.Topic,
                    CourseId = session.CourseId,
                    LectureIds = request.LectureIds,
                    MaterialIds = request.MaterialIds
                }, cancellationToken);
                
                if (ragResponse?.Chunks != null)
                    chunks.AddRange(ragResponse.Chunks);
            }

            var aiFlashcards = await _ollamaClient.GenerateFlashcardsAsync(
                chunks,
                request.Topic,
                request.NumberOfCards,
                cancellationToken);

            var now = DateTime.UtcNow;
            var entities = aiFlashcards.Select(f => new Flashcard
            {
                SessionId = request.SessionId,
                Topic = request.Topic,
                FrontText = f.Front,
                BackText = f.Back,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            await _unitOfWork.Flashcards.AddRangeAsync(entities, cancellationToken);
            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Generated {Count} flashcards for session {SessionId}, topic: {Topic}",
                entities.Count, request.SessionId, request.Topic);

            return entities.Select(e => new FlashcardDto
            {
                Id = e.Id,
                Topic = e.Topic,
                FrontText = e.FrontText,
                BackText = e.BackText,
                CreatedAt = e.CreatedAt
            }).ToList();
        }
    }
}
