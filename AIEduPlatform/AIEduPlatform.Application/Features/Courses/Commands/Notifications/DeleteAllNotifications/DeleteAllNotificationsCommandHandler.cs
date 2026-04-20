using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Notifications.DeleteAllNotifications;

public class DeleteAllNotificationsCommandHandler : IRequestHandler<DeleteAllNotificationsCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAllNotificationsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(DeleteAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("You must be logged in.");

        await _unitOfWork.Notifications.DeleteAllByUserIdAsync(userId, cancellationToken);
        
        // ExecuteDeleteAsync performs immediate db deletion, but calling SaveChangesAsync might be safe/required by architecture
        // Actually, ExecuteDelete doesn't need SaveChanges: it executes directly on the db.
        
        return Unit.Value;
    }
}
