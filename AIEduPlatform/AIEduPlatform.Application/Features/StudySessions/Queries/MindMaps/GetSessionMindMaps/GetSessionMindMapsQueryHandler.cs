using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps
{
    public class GetSessionMindMapsQueryHandler : IRequestHandler<GetSessionMindMapsQuery, List<MindMapDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSessionMindMapsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<MindMapDto>> Handle(GetSessionMindMapsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only view your own mind maps.");

            var mindMaps = await _unitOfWork.MindMaps
                .GetBySessionIdAsync(request.SessionId, cancellationToken);

            return mindMaps.Select(m => new MindMapDto
            {
                Id = m.Id,
                Topic = m.Topic,
                Nodes = m.Nodes,
                Connections = m.Connections,
                CreatedAt = m.CreatedAt
            }).ToList();
        }
    }
}
