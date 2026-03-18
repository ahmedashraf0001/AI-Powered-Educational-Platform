using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions
{
    public class GetStudentSubmissionsQueryHandler : IRequestHandler<GetStudentSubmissionsQuery, PagedResult<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentSubmissionsQueryHandler> _logger;

        public GetStudentSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<SubmissionDto>> Handle(GetStudentSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view your submissions.");

            var allSubmissions = await _unitOfWork.Submissions.GetSubmissionsByStudentIdAsync(
                userId.Value, includeExam: true, includeGrade: true, ct: cancellationToken);

            var totalCount = allSubmissions.Count;
            var paged = allSubmissions
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = new List<SubmissionDto>();
            foreach (var s in paged)
            {
                var course = s.Exam?.CourseId != null
                    ? await _unitOfWork.Courses.GetCourseByIdAsync(s.Exam.CourseId, ct: cancellationToken)
                    : null;

                items.Add(new SubmissionDto
                {
                    Id = s.Id,
                    ExamId = s.ExamId,
                    StudentId = s.StudentId,
                    ExamTitle = s.Exam?.Title ?? "Unknown Exam",
                    CourseName = course?.Title ?? "Unknown Course",
                    SubmittedAt = s.SubmittedAt,
                    IsGraded = s.Grade != null,
                    Score = s.Grade?.Score
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
