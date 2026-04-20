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
        private readonly IRAGService _ragService;

        public SummarizeSectionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            ILogger<SummarizeSectionCommandHandler> logger,
            IRAGService ragService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _logger = logger;
            _ragService = ragService;
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

            var ragResponse = await _ragService.RetrieveAllSegmentChunksAsync(request.SectionId, cancellationToken);
            var scopedChunks = ragResponse?.Chunks ?? new List<ContextChunk>();

            if (!scopedChunks.Any())
            {
                _logger.LogWarning("No chunks matched section boundaries for section {SectionId}.", request.SectionId);
                throw new BadRequestException("No indexed content found for this section.");
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
