using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Progress;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Progress.GetContinueLearning
{
    public class GetContinueLearningQueryHandler : IRequestHandler<GetContinueLearningQuery, List<ContinueLearningDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetContinueLearningQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<ContinueLearningDto>> Handle(GetContinueLearningQuery request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var enrollments = await _unitOfWork.Enrollments.GetActiveEnrollmentsByStudentAsync(studentId, cancellationToken);
            var result = new List<ContinueLearningDto>();

            foreach (var enrollment in enrollments)
            {
                var course = await _unitOfWork.Courses.GetCourseByIdAsync(enrollment.CourseId, ct: cancellationToken);
                if (course == null) continue;

                var totalMaterials = await _unitOfWork.Courses.GetMaterialsCountAsync(course.Id, cancellationToken);
                var completedMaterials = await _unitOfWork.MaterialProgress.GetCompletedMaterialCountAsync(studentId, course.Id, cancellationToken);

                var progressPercentage = totalMaterials > 0
                    ? Math.Round((double)completedMaterials / totalMaterials * 100, 1)
                    : 0;

                // Skip fully completed courses
                if (completedMaterials >= totalMaterials && totalMaterials > 0)
                    continue;

                var lastAccessed = await _unitOfWork.MaterialProgress.GetLastAccessedMaterialAsync(studentId, course.Id, cancellationToken);

                result.Add(new ContinueLearningDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    ProgressPercentage = progressPercentage,
                    LastMaterialId = lastAccessed?.MaterialId,
                    LastMaterialTitle = lastAccessed?.Material?.Title,
                    ResumePosition = lastAccessed?.LastPosition
                });
            }

            return result;
        }
    }
}
