using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.MindMaps.GenerateMindMap
{
    public class GenerateMindMapCommandHandler : IRequestHandler<GenerateMindMapCommand, MindMapDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<GenerateMindMapCommandHandler> _logger;

        public GenerateMindMapCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<GenerateMindMapCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<MindMapDto> Handle(GenerateMindMapCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only generate mind maps in your own study sessions.");

            var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
            {
                Query = request.CentralTopic,
                CourseId = session.CourseId,
                LectureIds = request.LectureId.HasValue ? [request.LectureId.Value] : null,
                MaterialIds = request.MaterialIds
            }, cancellationToken);

            var aiMindMap = await _ollamaClient.GenerateMindMapAsync(
                ragResponse.Chunks,
                request.CentralTopic,
                request.MaxDepth,
                cancellationToken);

            var now = DateTime.UtcNow;
            var nodesJson = JsonSerializer.Serialize(aiMindMap);

            var mindMap = new MindMap
            {
                SessionId = request.SessionId,
                Topic = request.CentralTopic,
                Nodes = nodesJson,
                Connections = "[]",
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _unitOfWork.MindMaps.AddAsync(mindMap, cancellationToken);
            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Generated mind map for session {SessionId}, topic: {Topic}",
                request.SessionId, request.CentralTopic);

            return new MindMapDto
            {
                Id = created.Id,
                Topic = created.Topic,
                Nodes = created.Nodes,
                Connections = created.Connections,
                CreatedAt = created.CreatedAt
            };
        }
    }
}
