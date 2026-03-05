using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Users;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetVoiceSettings
{
    public class GetVoiceSettingsQueryHandler
        : IRequestHandler<GetVoiceSettingsQuery, UserVoiceSettingsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetVoiceSettingsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<UserVoiceSettingsDto> Handle(
            GetVoiceSettingsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var existing = (await _unitOfWork.VoiceSettings
                .FindAsync(v => v.UserId == userId, cancellationToken))
                .FirstOrDefault();

            if (existing is null)
            {
                // Return defaults — nothing persisted yet
                return new UserVoiceSettingsDto();
            }

            return new UserVoiceSettingsDto
            {
                TeacherVoiceId = existing.TeacherVoiceId,
                StudentVoiceId = existing.StudentVoiceId,
                TeacherSpeed = existing.TeacherSpeed,
                StudentSpeed = existing.StudentSpeed,
                OutputFormat = existing.OutputFormat,
                SampleRate = existing.SampleRate,
                IncludePauses = existing.IncludePauses,
                PauseDurationMs = existing.PauseDurationMs,
                PauseMultiplier = existing.PauseMultiplier,
                NormalizeAudio = existing.NormalizeAudio
            };
        }
    }
}
