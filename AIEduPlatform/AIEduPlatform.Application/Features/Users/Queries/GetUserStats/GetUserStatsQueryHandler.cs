using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetUserStats
{
    public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserProfileStats>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public GetUserStatsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<UserProfileStats> Handle(GetUserStatsQuery request, CancellationToken cancellationToken)
        {
            var userId = request.UserId
                ?? _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var user = await _uow.Users.GetUserByIdAsync(userId, ct: cancellationToken)
                ?? throw new NotFoundException("User", userId);

            return await _uow.Users.GetUserStatsAsync(userId, cancellationToken);
        }
    }
}
