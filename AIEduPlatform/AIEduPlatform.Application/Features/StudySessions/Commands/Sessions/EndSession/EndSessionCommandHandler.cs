using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.EndSession
{
    public class EndSessionCommandHandler : IRequestHandler<EndSessionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<EndSessionCommandHandler> _logger;

        public EndSessionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<EndSessionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(EndSessionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only end your own study sessions.");
            if (session.EndedAt.HasValue)
                throw new BadRequestException("This session has already ended.");

            session.EndedAt = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.StudySessions.Update(session);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Study session {SessionId} ended by student {StudentId}. Duration: {Duration}",
                request.SessionId, userId.Value, session.EndedAt.Value - session.StartedAt);

            return Unit.Value;
        }
    }
}
