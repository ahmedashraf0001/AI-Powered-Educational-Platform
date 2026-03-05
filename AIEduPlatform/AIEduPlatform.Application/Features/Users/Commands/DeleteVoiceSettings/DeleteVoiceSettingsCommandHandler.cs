using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.DeleteVoiceSettings
{
    public class DeleteVoiceSettingsCommandHandler
        : IRequestHandler<DeleteVoiceSettingsCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteVoiceSettingsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(
            DeleteVoiceSettingsCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var existing = (await _unitOfWork.VoiceSettings
                .FindAsync(v => v.UserId == userId, cancellationToken))
                .FirstOrDefault();

            if (existing is not null)
            {
                _unitOfWork.VoiceSettings.Delete(existing);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}
