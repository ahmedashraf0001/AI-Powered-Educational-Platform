using AIEduPlatform.Core.DTOs.Users;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.SaveVoiceSettings
{
    /// <summary>
    /// Creates or updates the authenticated user's voice settings.
    /// </summary>
    public record SaveVoiceSettingsCommand : IRequest<UserVoiceSettingsDto>
    {
        public string TeacherVoiceId { get; init; } = "Damien Black";
        public string StudentVoiceId { get; init; } = "Daisy Studious";
        public double TeacherSpeed { get; init; } = 0.95;
        public double StudentSpeed { get; init; } = 1.0;
        public string OutputFormat { get; init; } = "mp3";
        public int SampleRate { get; init; } = 24000;
        public bool IncludePauses { get; init; } = true;
        public int PauseDurationMs { get; init; } = 500;
        public double PauseMultiplier { get; init; } = 1.0;
        public bool NormalizeAudio { get; init; } = true;
    }
}
