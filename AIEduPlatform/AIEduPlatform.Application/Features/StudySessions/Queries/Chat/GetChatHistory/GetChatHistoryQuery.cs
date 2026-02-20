using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Chat.GetChatHistory
{
    public record GetChatHistoryQuery : IRequest<PagedResult<ChatMessageDto>>
    {
        public Guid SessionId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 50;
    }
}
