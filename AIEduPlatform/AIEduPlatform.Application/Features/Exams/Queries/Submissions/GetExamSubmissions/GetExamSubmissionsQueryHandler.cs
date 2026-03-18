using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissions
{
    public class GetExamSubmissionsQueryHandler : IRequestHandler<GetExamSubmissionsQuery, PagedResult<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamSubmissionsQueryHandler> _logger;

        public GetExamSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<SubmissionDto>> Handle(GetExamSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view submissions.");

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);
            if (exam == null)
                throw new NotFoundException(nameof(Exam), request.ExamId);

            var course = await _unitOfWork.Courses.GetCourseByIdAsync(exam.CourseId, ct: cancellationToken);

            var submissions = await _unitOfWork.Submissions.GetSubmissionsByExamIdAsync(
                request.ExamId, includeGrades: true, ct: cancellationToken);

            var totalCount = submissions.Count;
            var paged = submissions
                .OrderByDescending(s => s.SubmittedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = new List<SubmissionDto>();
            foreach (var s in paged)
            {
                var student = await _unitOfWork.Users.GetUserByIdAsync(s.StudentId, ct: cancellationToken);
                items.Add(new SubmissionDto
                {
                    Id = s.Id,
                    ExamId = s.ExamId,
                    StudentId = s.StudentId,
                    ExamTitle = exam.Title,
                    CourseName = course?.Title ?? "Unknown Course",
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown Student",
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
