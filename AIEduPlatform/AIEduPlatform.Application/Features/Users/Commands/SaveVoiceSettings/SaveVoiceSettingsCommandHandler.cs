using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Users;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.SaveVoiceSettings
{
    public class SaveVoiceSettingsCommandHandler
        : IRequestHandler<SaveVoiceSettingsCommand, UserVoiceSettingsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public SaveVoiceSettingsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<UserVoiceSettingsDto> Handle(
            SaveVoiceSettingsCommand request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var existing = (await _unitOfWork.VoiceSettings
                .FindAsync(v => v.UserId == userId, cancellationToken))
                .FirstOrDefault();

            if (existing is not null)
            {
                // Update in-place
                existing.TeacherVoiceId = request.TeacherVoiceId;
                existing.StudentVoiceId = request.StudentVoiceId;
                existing.TeacherSpeed = request.TeacherSpeed;
                existing.StudentSpeed = request.StudentSpeed;
                existing.OutputFormat = request.OutputFormat;
                existing.SampleRate = request.SampleRate;
                existing.IncludePauses = request.IncludePauses;
                existing.PauseDurationMs = request.PauseDurationMs;
                existing.PauseMultiplier = request.PauseMultiplier;
                existing.NormalizeAudio = request.NormalizeAudio;

                _unitOfWork.VoiceSettings.Update(existing);
            }
            else
            {
                // First time — create
                var entity = new UserVoiceSettings
                {
                    UserId = userId,
                    TeacherVoiceId = request.TeacherVoiceId,
                    StudentVoiceId = request.StudentVoiceId,
                    TeacherSpeed = request.TeacherSpeed,
                    StudentSpeed = request.StudentSpeed,
                    OutputFormat = request.OutputFormat,
                    SampleRate = request.SampleRate,
                    IncludePauses = request.IncludePauses,
                    PauseDurationMs = request.PauseDurationMs,
                    PauseMultiplier = request.PauseMultiplier,
                    NormalizeAudio = request.NormalizeAudio
                };

                await _unitOfWork.VoiceSettings.AddAsync(entity, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UserVoiceSettingsDto
            {
                TeacherVoiceId = request.TeacherVoiceId,
                StudentVoiceId = request.StudentVoiceId,
                TeacherSpeed = request.TeacherSpeed,
                StudentSpeed = request.StudentSpeed,
                OutputFormat = request.OutputFormat,
                SampleRate = request.SampleRate,
                IncludePauses = request.IncludePauses,
                PauseDurationMs = request.PauseDurationMs,
                PauseMultiplier = request.PauseMultiplier,
                NormalizeAudio = request.NormalizeAudio
            };
        }
    }
}
