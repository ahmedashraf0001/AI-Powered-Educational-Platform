using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetSessionById
{
    public class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, SessionDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetSessionByIdQueryHandler> _logger;

        public GetSessionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetSessionByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<SessionDetailDto> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetSessionByIdAsync(
                request.SessionId,
                includeMessages: true,
                includeFlashcards: true,
                includeQuizzes: true,
                includeMindMaps: true,
                ct: cancellationToken);

            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only view your own study sessions.");

            return new SessionDetailDto
            {
                Id = session.Id,
                CourseId = session.CourseId,
                CourseName = session.Course?.Title ?? string.Empty,
                StartedAt = session.StartedAt,
                LastActivity = session.LastActivity,
                MessageCount = session.ChatMessages?.Count ?? 0,
                FlashcardCount = session.Flashcards?.Count ?? 0,
                QuizCount = session.GeneratedQuizzes?.Count ?? 0,
                MindMapCount = session.MindMaps?.Count ?? 0
            };
        }
    }
}
