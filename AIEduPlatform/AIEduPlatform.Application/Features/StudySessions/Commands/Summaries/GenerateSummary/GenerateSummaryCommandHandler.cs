using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Summaries.GenerateSummary
{
    public class GenerateSummaryCommandHandler : IRequestHandler<GenerateSummaryCommand, Summary>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<GenerateSummaryCommandHandler> _logger;

        public GenerateSummaryCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<GenerateSummaryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<Summary> Handle(GenerateSummaryCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only generate summaries in your own study sessions.");

            var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
            {
                Query = request.Topic,
                CourseId = session.CourseId,
                LectureIds = request.LectureId.HasValue ? [request.LectureId.Value] : null,
                MaterialIds = request.MaterialIds
            }, cancellationToken);

            var summary = await _ollamaClient.GenerateSummaryAsync(
                ragResponse.Chunks,
                request.SummaryLength,
                request.IncludeKeyPoints,
                cancellationToken);

            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);

            _logger.LogInformation(
                "Generated summary for session {SessionId}, topic: {Topic}",
                request.SessionId, request.Topic);

            return summary;
        }
    }
}
