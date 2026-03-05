using AIEduPlatform.Core.DTOs.Users;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetVoiceSettings
{
    /// <summary>
    /// Returns the authenticated user's persisted voice settings,
    /// or defaults if they haven't saved any yet.
    /// </summary>
    public record GetVoiceSettingsQuery : IRequest<UserVoiceSettingsDto>;
}
