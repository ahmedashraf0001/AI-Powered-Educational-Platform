using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionFlashcards
{
    public class GenerateSectionFlashcardsCommandHandler : IRequestHandler<GenerateSectionFlashcardsCommand, List<FlashcardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly ILogger<GenerateSectionFlashcardsCommandHandler> _logger;

        public GenerateSectionFlashcardsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            ILogger<GenerateSectionFlashcardsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _logger = logger;
        }

        public async Task<List<FlashcardDto>> Handle(GenerateSectionFlashcardsCommand request, CancellationToken cancellationToken)
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

            // Load ALL chunks for this material directly from DB
            var material = await _unitOfWork.Materials.GetMaterialByIdAsync(section.MaterialId, includeChunks: true, cancellationToken);
            if (material == null || material.Chunks == null || !material.Chunks.Any())
                throw new BadRequestException("No indexed content found for this material.");

            // Map MaterialChunk → ContextChunk
            var allChunks = material.Chunks.Select(c => new ContextChunk
            {
                Content = c.Content,
                Metadata = new ChunkMetadata
                {
                    SourceTitle = material.Title,
                    MaterialId = material.Id,
                    PageOrTimestamp = c.PageOrTimestamp ?? string.Empty,
                    Section = c.Section ?? string.Empty,
                    LectureName = c.LectureName ?? string.Empty,
                    CourseName = c.CourseName ?? string.Empty
                },
                AdditionalData = c.AdditionalData,
                RelevanceScore = 1.0f
            }).ToList();

            // Filter to section boundaries
            var scopedChunks = SectionChunkFilter.FilterChunksToSection(allChunks, section);
            if (!scopedChunks.Any())
            {
                _logger.LogWarning("No chunks matched section boundaries for section {SectionId}. Using all material chunks.", request.SectionId);
                scopedChunks = allChunks;
            }

            var aiFlashcards = await _ollamaClient.GenerateFlashcardsAsync(
                scopedChunks,
                section.Title,
                request.NumberOfCards,
                cancellationToken);

            var now = DateTime.UtcNow;
            var entities = aiFlashcards.Select(f => new Flashcard
            {
                SessionId = request.SessionId,
                Topic = $"Section: {section.Title}",
                FrontText = f.Front,
                BackText = f.Back,
                CreatedAt = now,
                UpdatedAt = now
            }).ToList();

            await _unitOfWork.Flashcards.AddRangeAsync(entities, cancellationToken);
            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated {Count} section flashcards for session {SessionId}, section {SectionId}: {Title} ({ChunkCount} chunks)",
                entities.Count, request.SessionId, request.SectionId, section.Title, scopedChunks.Count);

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
