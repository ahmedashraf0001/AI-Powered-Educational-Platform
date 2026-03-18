using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetUngradedSubmissions
{
    public class GetUngradedSubmissionsQueryHandler : IRequestHandler<GetUngradedSubmissionsQuery, PagedResult<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetUngradedSubmissionsQueryHandler> _logger;

        public GetUngradedSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetUngradedSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<SubmissionDto>> Handle(GetUngradedSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view ungraded submissions.");

            var ungradedSubmissions = await _unitOfWork.Submissions.GetUngradedSubmissionsAsync(
                request.ExamId, cancellationToken);

            var totalCount = ungradedSubmissions.Count;
            var paged = ungradedSubmissions
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = new List<SubmissionDto>();
            foreach (var s in paged)
            {
                var exam = s.Exam ?? await _unitOfWork.Exams.GetByIdAsync(s.ExamId, cancellationToken);
                var student = await _unitOfWork.Users.GetUserByIdAsync(s.StudentId, ct: cancellationToken);
                var course = exam?.CourseId != null
                    ? await _unitOfWork.Courses.GetCourseByIdAsync(exam.CourseId, ct: cancellationToken)
                    : null;

                items.Add(new SubmissionDto
                {
                    Id = s.Id,
                    ExamId = s.ExamId,
                    StudentId = s.StudentId,
                    ExamTitle = exam?.Title ?? "Unknown Exam",
                    CourseName = course?.Title ?? "Unknown Course",
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown Student",
                    SubmittedAt = s.SubmittedAt,
                    IsGraded = false
                });
            }

            return new PagedResult<SubmissionDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
