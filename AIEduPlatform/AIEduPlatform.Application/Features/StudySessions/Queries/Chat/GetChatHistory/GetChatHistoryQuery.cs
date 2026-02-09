using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Chat.GetChatHistory
{
    public record GetChatHistoryQuery : IRequest<List<ChatMessageDto>>
    {
        public Guid SessionId { get; init; }
    }
}
