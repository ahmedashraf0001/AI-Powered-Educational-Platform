using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.DeleteVoiceSettings
{
    /// <summary>
    /// Resets the authenticated user's voice settings back to defaults
    /// by deleting their persisted record.
    /// </summary>
    public record DeleteVoiceSettingsCommand : IRequest<Unit>;
}
