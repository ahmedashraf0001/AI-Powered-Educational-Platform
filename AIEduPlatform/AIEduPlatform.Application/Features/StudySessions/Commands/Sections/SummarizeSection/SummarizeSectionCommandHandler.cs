using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.SummarizeSection
{
    public class SummarizeSectionCommandHandler : IRequestHandler<SummarizeSectionCommand, Summary>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly ILogger<SummarizeSectionCommandHandler> _logger;

        public SummarizeSectionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            ILogger<SummarizeSectionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _logger = logger;
        }

        public async Task<Summary> Handle(SummarizeSectionCommand request, CancellationToken cancellationToken)
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

            // Filter to section boundaries (time or page range)
            var scopedChunks = SectionChunkFilter.FilterChunksToSection(allChunks, section);

            if (!scopedChunks.Any())
            {
                _logger.LogWarning("No chunks matched section boundaries for section {SectionId}. Using all material chunks.", request.SectionId);
                scopedChunks = allChunks;
            }

            var summary = await _ollamaClient.GenerateSummaryAsync(
                scopedChunks,
                request.SummaryLength,
                request.IncludeKeyPoints,
                cancellationToken);

            // Save messages
            var now = DateTime.UtcNow;
            await _unitOfWork.ChatMessages.AddAsync(new ChatMessage
            {
                SessionId = request.SessionId,
                Role = ChatRole.Student,
                Content = $"Summarize section: {section.Title}",
                CreatedAt = now,
                UpdatedAt = now
            }, cancellationToken);

            await _unitOfWork.ChatMessages.AddAsync(new ChatMessage
            {
                SessionId = request.SessionId,
                Role = ChatRole.Assistant,
                Content = JsonSerializer.Serialize(summary),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Generated section summary for session {SessionId}, section {SectionId}: {Title} ({ChunkCount} chunks)",
                request.SessionId, request.SectionId, section.Title, scopedChunks.Count);

            return summary;
        }
    }
}
