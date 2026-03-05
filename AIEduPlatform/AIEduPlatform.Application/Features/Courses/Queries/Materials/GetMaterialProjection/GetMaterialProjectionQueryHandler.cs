using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Materials;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Materials.GetMaterialProjection
{
    public class GetMaterialProjectionQueryHandler : IRequestHandler<GetMaterialProjectionQuery, MaterialProjectionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMaterialProjectionQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<MaterialProjectionDto> Handle(GetMaterialProjectionQuery request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var material = await _unitOfWork.Materials.GetMaterialByIdAsync(request.MaterialId, ct: cancellationToken)
                ?? throw new NotFoundException(nameof(Material), request.MaterialId);

            // Validate enrollment via lecture → course
            var lecture = await _unitOfWork.Lectures.GetByIdAsync(material.LectureId, cancellationToken)
                ?? throw new NotFoundException(nameof(Lecture), material.LectureId);

            if (!await _unitOfWork.Enrollments.IsStudentEnrolledAsync(studentId, lecture.CourseId, cancellationToken))
                throw new BadRequestException("You are not enrolled in this course.");

            // Load progress record
            var progress = await _unitOfWork.MaterialProgress.GetProgressAsync(studentId, request.MaterialId, cancellationToken);

            int current = progress?.LastPosition ?? 0;
            int total = GetTotal(material);
            double percentage = total > 0 ? Math.Round((double)current / total * 100, 1) : 0;
            bool isCompleted = progress?.IsCompleted ?? false;

            // Load semantic sections and find current section
            SemanticSectionDto? currentSection = null;
            if (current > 0)
            {
                var section = await _unitOfWork.SemanticSections.GetSectionAtPositionAsync(request.MaterialId, current, cancellationToken);
                if (section != null)
                {
                    currentSection = new SemanticSectionDto
                    {
                        Id = section.Id,
                        Title = section.Title,
                        Summary = section.Summary,
                        StartSeconds = section.StartSeconds,
                        EndSeconds = section.EndSeconds,
                        StartPage = section.StartPage,
                        EndPage = section.EndPage,
                        OrderIndex = section.OrderIndex
                    };
                }
            }

            return new MaterialProjectionDto
            {
                LessonId = material.LectureId,
                Title = material.Title,
                MaterialType = material.Type.ToString(),
                MaterialUrl = material.FileUrl,
                Progress = new MaterialProgressDto
                {
                    Current = current,
                    Total = total,
                    Percentage = percentage
                },
                IsCompleted = isCompleted,
                CurrentSection = currentSection
            };
        }

        private static int GetTotal(Material material) => material.Type switch
        {
            MaterialType.Video or MaterialType.Audio => material.DurationSeconds ?? 0,
            MaterialType.Document => material.TotalPages ?? 0,
            _ => 0
        };
    }
}
