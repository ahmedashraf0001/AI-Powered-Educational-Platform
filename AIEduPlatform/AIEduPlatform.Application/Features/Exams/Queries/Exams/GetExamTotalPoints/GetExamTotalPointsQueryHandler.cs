using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamTotalPoints
{
    public class GetExamTotalPointsQueryHandler : IRequestHandler<GetExamTotalPointsQuery, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamTotalPointsQueryHandler> _logger;

        public GetExamTotalPointsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamTotalPointsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<int> Handle(GetExamTotalPointsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view exam details.");

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);
            if (exam == null)
                throw new NotFoundException(nameof(Exam), request.ExamId);

            var totalPoints = await _unitOfWork.Exams.GetTotalPointsAsync(request.ExamId, cancellationToken);

            return totalPoints;
        }
    }
}
